using System;
using System.Data.SQLite;

using FluentAssertions;

using System.Linq;
using System.Threading.Tasks;

using LinqToDB;

using TransactionPoc.Tests.Models;

using Xunit;

namespace TransactionPoc.Tests.Tests
{
    public class TransactionTest : BaseTest
    {

        [Fact]
        public void Should_Be_Able_To_Commit_NoSql_Transaction()
        {
            var initial = NoSqlInitialModel;
            using (var trans = TransactionFactory.NewTransaction())
            {
                var rm = trans.EnlistNosqlResourceManager();

                var toTry = new SimpleModel
                {
                    Id = initial.Id,
                    Name = initial.Name,
                    Number = 10
                };

                toTry.Should().NotBe(initial);

                rm.Add(() => NoSqlContext.Simple.Update(toTry), () => NoSqlContext.Simple.Update(initial));

                trans.Commit();
            }

            var results = NoSqlContext.Simple.FindAll().ToList();
            results.Count.Should().Be(1);
            results.First().Should().NotBe(initial);
            results.First().Number.Should().Be(10);
        }

        [Fact]
        public void Should_Be_Able_To_Rollback_Async_Transaction()
        {
            var expected = SqlInitialModel;

            using (var trans = TransactionFactory.NewTransaction())
            {
                var ctx = SqlContext;
                trans.EnlistSqlResourceManager(() => ctx.GetConnection());

                var toTry = new SimpleModel
                {
                    Id = expected.Id,
                    Name = expected.Name,
                    Number = expected.Number + 1
                };

                toTry.Should().NotBe(expected);
                Task.Factory.StartNew(
                    () => { ctx.Update(toTry); });

                trans.Rollback();
            }

            var results = SqlContext.Simple.ToList();
            results.Count.Should().Be(1);
            results.First().Should().Be(expected);
        }

        [Fact]
        public void Should_Be_Able_To_Rollback_Native_Sql_Transaction()
        {
            var expected = SqlInitialModel;

            using (var trans = TransactionFactory.NewTransaction())
            {
                trans.EnlistSqlResourceManager(() => SqlContext.GetConnection());

                var toTry = new SimpleModel
                {
                    Id = expected.Id,
                    Name = expected.Name,
                    Number = expected.Number + 1
                };

                toTry.Should().NotBe(expected);
                var nativeTrans = SqlContext.BeginTransaction();
                SqlContext.Update(toTry);

                trans.Rollback();

                nativeTrans.Invoking(x => x.Commit()).ShouldThrow<SQLiteException>();
            }

            var results = SqlContext.Simple.ToList();
            results.Count.Should().Be(1);
            results.First().Should().Be(expected);
        }

        [Fact]
        public void Should_Be_Able_To_Rollback_NoSql_Transaction()
        {
            var expected = NoSqlInitialModel;
            using (var trans = TransactionFactory.NewTransaction())
            {
                var rm = trans.EnlistNosqlResourceManager();

                var toTry = new SimpleModel
                {
                    Id = expected.Id,
                    Name = expected.Name,
                    Number = expected.Number + 1
                };

                toTry.Should().NotBe(expected);

                rm.Add(() => NoSqlContext.Simple.Update(toTry), () => NoSqlContext.Simple.Update(expected));

                trans.Rollback();
            }

            var results = NoSqlContext.Simple.FindAll().ToList();
            results.Count.Should().Be(1);
            results.First().Should().Be(expected);
        }

        [Fact]
        public void Should_Be_Able_To_Rollback_Parent_Transaction()
        {
            var sqlExpected = SqlInitialModel;
            var nosqlExpected = NoSqlInitialModel;
            using (var trans = TransactionFactory.NewTransaction())
            {
                var sqlTry = new SimpleModel
                {
                    Id = sqlExpected.Id,
                    Name = sqlExpected.Name + "-" + sqlExpected.Name,
                    Number = sqlExpected.Number + 10
                };
                sqlTry.Should().NotBe(sqlExpected);

                var nosqlTry = new SimpleModel
                {
                    Id = nosqlExpected.Id,
                    Name = nosqlExpected.Name + "-" + nosqlExpected.Name,
                    Number = nosqlExpected.Number + 10
                };
                nosqlTry.Should().NotBe(nosqlExpected);


                var nosqlTrans = trans.ChildTransaction();
                var nosqlRm = nosqlTrans.EnlistNosqlResourceManager();

                nosqlRm.Add(() => NoSqlContext.Simple.Update(nosqlTry), () => NoSqlContext.Simple.Update(nosqlExpected));
                nosqlTrans.Complete();


                var sqlTrans = trans.ChildTransaction();
                sqlTrans.EnlistSqlResourceManager(() => SqlContext.GetConnection());

                SqlContext.Update(sqlTry);
                sqlTrans.Complete();

                trans.Rollback();
            }

            var nosqlResults = NoSqlContext.Simple.FindAll().ToList();
            nosqlResults.Count.Should().Be(1);
            nosqlResults.First().Should().Be(nosqlExpected);

            var sqlResults = SqlContext.Simple.ToList();
            sqlResults.Count.Should().Be(1);
            sqlResults.First().Should().Be(sqlExpected);
        }

        [Fact]
        public void Should_Be_Able_To_Rollback_Serialized_Transaction()
        {
            var expected = NoSqlInitialModel;
            using (var trans = TransactionFactory.NewTransaction())
            {
                var transToken = trans.ChildTransaction().ToToken();
                var deserializedTrans = TransactionFactory.FromToken(transToken);
                var rm = deserializedTrans.EnlistNosqlResourceManager();

                var toTry = new SimpleModel
                {
                    Id = expected.Id,
                    Name = expected.Name,
                    Number = expected.Number + 1
                };

                toTry.Should().NotBe(expected);

                rm.Add(() => NoSqlContext.Simple.Update(toTry), () => NoSqlContext.Simple.Update(expected));

                trans.Rollback();
            }

            var results = NoSqlContext.Simple.FindAll().ToList();
            results.Count.Should().Be(1);
            results.First().Should().Be(expected);
        }

        [Fact]
        public void Should_Be_Able_To_Rollback_Sql_Transaction()
        {
            var expected = SqlInitialModel;

            using (var trans = TransactionFactory.NewTransaction())
            {
                trans.EnlistSqlResourceManager(() => SqlContext.GetConnection());

                var toTry = new SimpleModel
                {
                    Id = expected.Id,
                    Name = expected.Name,
                    Number = expected.Number + 1
                };

                toTry.Should().NotBe(expected);
                SqlContext.Update(toTry);

                trans.Rollback();
            }

            var results = SqlContext.Simple.ToList();
            results.Count.Should().Be(1);
            results.First().Should().Be(expected);
        }

        [Fact]
        public void Should_Be_Able_To_Rollback_Insert()
        {
            var nosqlResults = NoSqlContext.Simple.FindAll().ToList();
            nosqlResults.Count.Should().Be(1);

            var sqlResults = SqlContext.Simple.ToList();
            sqlResults.Count.Should().Be(1);

            using (var trans = TransactionFactory.NewTransaction())
            {
                var @try = new SimpleModel
                {
                    Id = 23,
                    Name = Guid.NewGuid().ToString("N"),
                    Number = new Random().Next()
                };


                var nosqlTrans = trans.ChildTransaction();
                var nosqlRm = nosqlTrans.EnlistNosqlResourceManager();

                nosqlRm.Add(() => NoSqlContext.Simple.Insert(@try), () => NoSqlContext.Simple.Delete(x => x.Name == @try.Name && x.Number == @try.Number));
                nosqlTrans.Complete();


                var sqlTrans = trans.ChildTransaction();
                sqlTrans.EnlistSqlResourceManager(() => SqlContext.GetConnection());
                SqlContext.Insert(@try);
                sqlTrans.Complete();

                trans.Rollback();
            }

            nosqlResults = NoSqlContext.Simple.FindAll().ToList();
            nosqlResults.Count.Should().Be(1);

            sqlResults = SqlContext.Simple.ToList();
            sqlResults.Count.Should().Be(1);
        }

    }
}
