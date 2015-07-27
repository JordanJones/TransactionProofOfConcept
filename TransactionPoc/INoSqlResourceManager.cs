using System;
using System.Linq;

namespace TransactionPoc
{
    public interface INoSqlResourceManager
    {

        INoSqlResourceManager Add(Action command, Action rollbackCommand);

    }
}
