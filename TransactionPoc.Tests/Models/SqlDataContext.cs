using System;
using System.Data.Common;
using System.Linq;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.SQLite;

namespace TransactionPoc.Tests.Models
{
    public class SqlDataContext : DataConnection
    {

        public SqlDataContext(string configurationString) : base(GetDataProvider(), configurationString)
        {
        }

        public ITable<SimpleModel> Simple
        {
            get { return GetTable<SimpleModel>(); }
        }

        private static IDataProvider GetDataProvider()
        {
            return new SQLiteDataProvider();
        }


        public DbConnection GetConnection()
        {
            return Connection as DbConnection;
        }

    }
}
