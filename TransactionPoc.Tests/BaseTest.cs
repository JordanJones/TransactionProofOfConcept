using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;

using LinqToDB;

using TransactionPoc.Tests.Models;

namespace TransactionPoc.Tests
{
    public abstract class BaseTest : IDisposable
    {

        private const string SqliteBaseConnectionString = "Version=3; DateTimeFormat=ISO8601; DateTimeKind=Utc; Pooling=true; JournalMode=Off;";

        private readonly string _tempPath;

        protected BaseTest()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
            SqlDataFile = Path.Combine(_tempPath, "sqldata.db");
            NoSqlDataFile = Path.Combine(_tempPath, "nosqldata.db");

            SqlContext = CreateSqlDataContext();
            NoSqlContext = CreateNoSqlDataContext();
        }

        public NoSqlDataContext NoSqlContext { get; private set; }

        public string NoSqlDataFile { get; }

        public SimpleModel NoSqlInitialModel { get; private set; }

        public SqlDataContext SqlContext { get; private set; }

        public string SqlDataFile { get; }

        public SimpleModel SqlInitialModel { get; private set; }

        public void Dispose()
        {
            DoDispose();
            if (NoSqlContext != null)
            {
                try
                {
                    NoSqlContext.Dispose();
                    NoSqlContext = null;
                }
                catch
                {
                    /* ignored */
                }
            }
            if (SqlContext != null)
            {
                try
                {
                    SqlContext.Connection.Close();
                    SqlContext.Connection.Dispose();
                    SqlContext.Dispose();
                    SqlContext = null;
                }
                catch
                {
                    System.Diagnostics.Debugger.Break();
                    /* ignored */
                }
                SQLiteConnection.ClearAllPools();
            }

            if (Directory.Exists(_tempPath))
            {
                var count = 0;
                while (count < 20)
                {
                    try
                    {
                        Directory.Delete(_tempPath, true);
                        break;
                    }
                    catch
                    {
                        count++;
                        Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    }
                }
            }
        }

        private static SimpleModel CreateInitialModel()
        {
            return new SimpleModel
            {
                Name = Guid.NewGuid().ToString("N"),
                Number = 1
            };
        }

        private NoSqlDataContext CreateNoSqlDataContext()
        {
            using (var ctx = new NoSqlDataContext(NoSqlDataFile))
            {
                ctx.Simple.Insert(CreateInitialModel());
                NoSqlInitialModel = ctx.Simple.FindAll().First();
            }
            return new NoSqlDataContext(NoSqlDataFile);
        }

        private SqlDataContext CreateSqlDataContext()
        {
            if (!File.Exists(SqlDataFile))
            {
                SQLiteConnection.CreateFile(SqlDataFile);
            }

            var connBuilder = new SQLiteConnectionStringBuilder(SqliteBaseConnectionString)
            {
                DataSource = SqlDataFile
            };

            using (var ctx = new SqlDataContext(connBuilder.ConnectionString))
            {
                using (var cmd = ctx.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS Simple(Id INTEGER PRIMARY KEY ASC AUTOINCREMENT, Name TEXT NOT NULL, Number INTEGER NOT NULL)";
                    cmd.ExecuteNonQuery();
                }
                ctx.Insert(CreateInitialModel());
                SqlInitialModel = ctx.Simple.First();
            }

            return new SqlDataContext(connBuilder.ConnectionString);
        }

        protected virtual void DoDispose() {}

    }
}
