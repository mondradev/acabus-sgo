using InnSyTech.Standard.Database.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace InnSyTech.Standard.Database.Linq
{
    internal class DbSqlReader<T> : IEnumerable<T>, IEnumerable where T : class, new()
    {
        private DbSqlEnumerator _enumerator;

        internal DbSqlReader(DbDataReader reader, DbCommand command, DbConnection connection)
            => _enumerator = new DbSqlEnumerator(reader, command, connection);

        public IEnumerator<T> GetEnumerator()
        {
            DbSqlEnumerator e = _enumerator;

            if (e == null)
                throw new InvalidOperationException("No se puede enumerar más de una vez.");

            _enumerator = null;

            return e;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private class DbSqlEnumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            private T _currrent;
            private DbDataReader _dbReader;
            private DbField[] _fields;
            private DbField _primaryKey;
            private DbCommand _command;
            private DbConnection _connection;

            internal DbSqlEnumerator(DbDataReader reader, DbCommand command, DbConnection connection)
            {
                _dbReader = reader;
                _command = command;
                _connection = connection;

                _fields = DbField.GetFields(typeof(T)).Where(f => !f.IsPrimaryKey).ToArray();
                _primaryKey = DbField.GetPrimaryKey(typeof(T));
            }

            public T Current => _currrent;

            object IEnumerator.Current => _currrent;

            public void Dispose()
                => _dbReader.Dispose();

            public bool MoveNext()
            {
                if (_dbReader.Read())
                {
                    T instance = (T)DbHelper.ToInstance(typeof(T), _dbReader);

                    _currrent = instance;

                    return true;
                }

                _dbReader.Close();
                _command.Dispose();
                _connection.Close();

                return false;
            }

            public void Reset()
            {
            }
        }
    }
}