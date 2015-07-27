using System;
using System.Collections.Generic;
using System.Transactions;

namespace TransactionPoc
{
    public class NoSqlResourceManager : IEnlistmentNotification, INoSqlResourceManager
    {
        private readonly List<Action> _commands = new List<Action>();
        private readonly List<Action> _rollbackCommands = new List<Action>();

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            foreach (var x in _commands)
            {
                x();
            }
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            foreach (var x in _rollbackCommands)
            {
                x();
            }
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            Rollback(enlistment);
        }

        public INoSqlResourceManager Add(Action command, Action rollbackCommand)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (rollbackCommand == null)
            {
                throw new ArgumentNullException("rollbackCommand");
            }
            _commands.Add(command);
            _rollbackCommands.Add(rollbackCommand);
            return this;
        }

    }
}