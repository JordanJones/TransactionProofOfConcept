# Transactions proof of concept
C# Transactions proof of concept using a System.Data library and a NoSQL library without built in transaction support.

--

The important bits are:

1. Requires .Net 4.5.1 in order to support Async/Await
2. NoSql requires a 1:1 between attempted command and rollback command (insert -> delete)


Transactional code should run the most risky action last.

### Example
```csharp
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


    var sqlTrans = trans.ChildTransaction();
    sqlTrans.EnlistSqlResourceManager(() => SqlContext.GetConnection());

    SqlContext.Update(sqlTry);
    sqlTrans.Complete();


    var nosqlTrans = trans.ChildTransaction();
    var nosqlRm = nosqlTrans.EnlistNosqlResourceManager();

    nosqlRm.Add(() => NoSqlContext.Simple.Update(nosqlTry), () => NoSqlContext.Simple.Update(nosqlExpected));
    nosqlTrans.Complete();

    trans.Rollback();
}
var nosqlResults = NoSqlContext.Simple.FindAll().ToList();
nosqlResults.Count.Should().Be(1);
nosqlResults.First().Should().Be(nosqlExpected);

var sqlResults = SqlContext.Simple.ToList();
sqlResults.Count.Should().Be(1);
sqlResults.First().Should().Be(sqlExpected);
```
