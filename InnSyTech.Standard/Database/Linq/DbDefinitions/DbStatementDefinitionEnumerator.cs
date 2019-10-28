using System.Collections;
using System.Collections.Generic;

namespace InnSyTech.Standard.Database.Linq.DbDefinitions
{
    internal class DbStatementDefinitionEnumerator : IEnumerator<DbStatementDefinition>
    {
        private DbStatementDefinition _current;
        private DbStatementDefinition _root;

        public DbStatementDefinitionEnumerator(DbStatementDefinition root)
        {
            _root = root;
        }

        public DbStatementDefinition Current => _current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _current = null;
            _root = null;
        }

        public bool MoveNext()
        {
            if (Current is null)
                return (_current = _root) != null;

            return (_current = Current.SubStatementDefinition) != null;
        }

        public void Reset()
            => _current = _root;
    }
}