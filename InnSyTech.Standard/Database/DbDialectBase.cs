using System;

namespace InnSyTech.Standard.Database
{
    /// <summary>
    /// Define la estrutura base que debe tener una instancia de configuración para la comunicación
    /// con un tipo de base de datos.
    /// </summary>
    public abstract class DbDialectBase
    {
        /// <summary>
        /// Crea una instancia nueva de <see cref="DbDialectBase"/> especificando su cadena de conexión.
        /// </summary>
        /// <param name="connectionString">Cadena de conexión a la base de datos.</param>
        public DbDialectBase(String connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Obtiene la cadena de conexión a la base de datos.
        /// </summary>
        public String ConnectionString { get; }

        /// <summary>
        /// Obtiene el convertidor de fecha y hora utilizado para leer y escribir fecha/hora en la
        /// base de datos. Si es null, se utilizará <see cref="DateTime.ToString()" /> para guardar la fecha/hora.
        /// </summary>
        public abstract IDbConverter DateTimeConverter { get; }

        /// <summary>
        /// Obtiene la función del último insertado según el motor de base de datos utilizado.
        /// </summary>
        public abstract String LastInsertFunctionName { get; }

        /// <summary>
        /// Obtiene el número de transacciones permitidas de manera concurrente.
        /// </summary>
        public abstract int TransactionPerConnection { get; }
    }
}