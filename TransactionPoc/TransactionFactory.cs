using System;
using System.Data.Common;
using System.Linq;
using System.Transactions;

namespace TransactionPoc
{
    public static class TransactionFactory
    {

        public static byte[] ToToken(this Transaction This)
        {
            return TransactionInterop.GetTransmitterPropagationToken(This);
        }

        public static Transaction FromToken(byte[] token)
        {
            return TransactionInterop.GetTransactionFromTransmitterPropagationToken(token);
        }

        public static DependentTransaction ChildTransaction(this Transaction This)
        {
            return This.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
        }

        public static CommittableTransaction NewTransaction()
        {
            return new CommittableTransaction();
        }


        public static INoSqlResourceManager EnlistNosqlResourceManager(this Transaction This)
        {
            var rm = new NoSqlResourceManager();
            This.EnlistVolatile(rm, EnlistmentOptions.None);
            return rm;
        }

        public static T EnlistSqlResourceManager<T>(this Transaction This, Func<T> producer)
            where T : DbConnection
        {
            var dbc = producer();
            dbc.EnlistTransaction(This);
            return dbc;
        }

    }
}
