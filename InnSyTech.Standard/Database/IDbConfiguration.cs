using System;

namespace InnSyTech.Standard.Database
{
    /// <summary>
    /// Define la estrutura que tiene que tener una instancia de configuración para la comunicación
    /// con un tipo de base de datos.
    /// </summary>
    public interface IDbDialect
    {
        /// <summary>
        /// Obtiene la cadena de conexión a la base de datos.
        /// </summary>
        String ConnectionString { get; }

        /// <summary>
        /// Obtiene la función del último insertado según el motor de base de datos utilizado.
        /// </summary>
        String LastInsertFunctionName { get; }

        /// <summary>
        /// Obtiene el número de transacciones permitidas de manera concurrente.
        /// </summary>
        int TransactionPerConnection { get; }
    }
}