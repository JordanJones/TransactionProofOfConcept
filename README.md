# Transactions proof of concept
C# Transactions proof of concept using a System.Data library and NoSQL library

--

The important bits are:

1. Requires .Net 4.5.1 in order to support Async/Await
2. NoSql requires a 1:1 between attempted command and rollback command (insert -> delete)
