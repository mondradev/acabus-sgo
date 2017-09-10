﻿using InnSyTech.Standard.Database.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace InnSyTech.Standard.Database.Linq
{
    /// <summary>
    /// Proporciona un lector de datos obtenidos a través del proveedor <see cref="DbSqlProvider"/> que utiliza las consultas <see cref="DbSqlQuery{TData}"/>.
    /// </summary>
    /// <typeparam name="TData">Tipo de dato del elemento a leer.</typeparam>
    internal class DbSqlReader<TData> : IEnumerable<TData>, IEnumerable where TData : class, new()
    {
        /// <summary>
        /// Enumerador de la lectura de datos.
        /// </summary>
        private DbSqlEnumerator _enumerator;

        /// <summary>
        /// Crea una instancia nueva de <see cref="DbSqlReader{TData}"/>, especificando el lector de datos, el comando usado y la conexión a la base de datos actual.
        /// </summary>
        /// <param name="reader">Lector de datos.</param>
        /// <param name="command">Comando utilizado.</param>
        /// <param name="connection">Conexión a la base de datos.</param>
        internal DbSqlReader(DbDataReader reader, DbCommand command, DbConnection connection)
            => _enumerator = new DbSqlEnumerator(reader, command, connection);

        /// <summary>
        /// Obtiene el enumerador genéricode la lectura de datos, solo puede ser enumerado una vez.
        /// </summary>
        /// <returns>El enumerador de la lectura de datos.</returns>
        /// <exception cref="InvalidOperationException">No puede ser enumerado más de una vez.</exception>
        public IEnumerator<TData> GetEnumerator()
        {
            DbSqlEnumerator e = _enumerator;

            if (e == null)
                throw new InvalidOperationException("No se puede enumerar más de una vez.");

            _enumerator = null;

            return e;
        }

        /// <summary>
        /// Obtiene el enumerador la lectura de datos, solo puede ser enumerado una vez.
        /// </summary>
        /// <returns>El enumerador de la lectura de datos.</returns>
        /// <exception cref="InvalidOperationException">No puede ser enumerado más de una vez.</exception>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Define el enumerador de la lectura de datos, obteniendo cada registro en cuanto avanza el cursor del enumerador.
        /// </summary>
        private class DbSqlEnumerator : IEnumerator<TData>, IEnumerator, IDisposable
        {
            /// <summary>
            /// Comando utilizado en la lectura.
            /// </summary>
            private DbCommand _command;

            /// <summary>
            /// Conexión a la base de datos.
            /// </summary>
            private DbConnection _connection;

            /// <summary>
            /// Elemento actual de la enumeración.
            /// </summary>
            private TData _currrent;

            /// <summary>
            /// Lector de datos-
            /// </summary>
            private DbDataReader _dbReader;

            /// <summary>
            /// Campos de la instancia.
            /// </summary>
            private DbFieldInfo[] _fields;

            /// <summary>
            /// Llave primaria de la instancia.
            /// </summary>
            private DbFieldInfo _primaryKey;

            /// <summary>
            /// Crea una instancia nueva del enumerador <see cref="DbSqlEnumerator"/>.
            /// </summary>
            /// <param name="reader">Lector de datos.</param>
            /// <param name="command">Comando utilizado para la lectura.</param>
            /// <param name="connection">Conexión a la base de datos.</param>
            internal DbSqlEnumerator(DbDataReader reader, DbCommand command, DbConnection connection)
            {
                _dbReader = reader;
                _command = command;
                _connection = connection;

                _fields = DbFieldInfo.GetFields(typeof(TData)).Where(f => !f.IsPrimaryKey).ToArray();
                _primaryKey = DbFieldInfo.GetPrimaryKey(typeof(TData));
            }

            /// <summary>
            /// Obtiene el elemento actual de la enumeración.
            /// </summary>
            public TData Current => _currrent;

            /// <summary>
            /// Obtiene el elemento actual de la enumeración.
            /// </summary>
            object IEnumerator.Current => _currrent;

            /// <summary>
            /// Desecha el enumerador actual, desechando también el lector de datos.
            /// </summary>
            public void Dispose()
                => _dbReader.Dispose();

            /// <summary>
            /// Avanza al proximo elemento de la enumeración y devuelve false en caso de llegar al final.
            /// </summary>
            /// <returns>Un valor true si se logró avanzar.</returns>
            public bool MoveNext()
            {
                if (_dbReader.Read())
                {
                    TData instance = (TData)DbHelper.ToInstance(typeof(TData), _dbReader);

                    _currrent = instance;

                    return true;
                }

                _dbReader.Close();
                _command.Dispose();
                _connection.Close();

                return false;
            }

            /// <summary>
            /// Reinicia la enumeración actual. NO hace nada ya que no se puede reiniciar la enumeración.
            /// </summary>
            public void Reset()
            {
            }
        }
    }
}