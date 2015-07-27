using System;
using System.Linq;

using LiteDB;

namespace TransactionPoc.Tests.Models
{
    public class NoSqlDataContext : IDisposable
    {

        public NoSqlDataContext(string connection)
        {
            Database = new LiteDatabase(connection);
        }

        public LiteDatabase Database { get; }

        public LiteCollection<SimpleModel> Simple
        {
            get { return Database.GetCollection<SimpleModel>("simple"); }
        }

        public void Dispose()
        {
            Database.Dispose();
        }

    }
}
