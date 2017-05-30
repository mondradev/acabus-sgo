using System;
using System.Data.Common;

namespace InnSyTech.Standard.Database
{
    /// <summary>
    /// Define una estrutura para realizar operaciones a una base de datos.
    /// </summary>
    public class DbManager
    {

        /// <summary>
        /// Campo que provee a la propiedad 'Session'.
        /// </summary>
        private DbSession _session;

        /// <summary>
        /// Obtiene o establece la sesión de la conexión a la base de datos.
        /// </summary>
        public DbSession Session => _session;

        /// <summary>
        /// Inicializa el administrador de la conexión a la base de datos.
        /// </summary>
        /// <param name="dbType">Tipo de la base de datos a conectar.</param>
        /// <param name="configuration">Configuración utilizada para la conexión.</param>
        /// <returns>Una instancia de administrador de base de datos.</returns>
        public static DbManager Initialize(Type dbType, IDbConfiguration configuration)
        {
            return new DbManager()
            {
                _session = new DbSession((DbConnection)Activator.CreateInstance(dbType, new object[] { configuration.ConnectionString }))
                {
                    Configuration = configuration
                }
            };
        }
    }
}