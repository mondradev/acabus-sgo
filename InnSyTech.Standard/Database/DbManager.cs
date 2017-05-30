using System;
using System.Data.Common;

namespace InnSyTech.Standard.Database
{
    public static class DbManager
    {
        private static String _lastInsertFunctionName;

        /// <summary>
        /// Campo que provee a la propiedad 'Session'.
        /// </summary>
        private static DbSession _session;

        public static String LastInsertFunctionName {
            get {
                if (_lastInsertFunctionName is null)
                    throw new Exception("No se ha especificado la función de ultimo insertado (DbManager.LastInsertFunctionName).");
                return _lastInsertFunctionName;
            }
            set {
                _lastInsertFunctionName = value;
            }
        }

        /// <summary>
        /// Obtiene o establece la sesión de la conexión a la base de datos.
        /// </summary>
        internal static DbSession Session => _session;

        public static void Initialize(String connectionString, Type dbType, Int64 transactionPerConnection = 1)
        {
            if (_session != null)
                return;

            _session = new DbSession((DbConnection)Activator.CreateInstance(dbType, new object[] { connectionString }))
            {
                TransactionPerConnection = transactionPerConnection
            };
        }
    }
}