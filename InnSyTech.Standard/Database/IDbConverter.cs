using System;

namespace InnSyTech.Standard.Database
{
    /// <summary>
    /// Define la estructura de un convertidor de datos para traducir el valor de una propiedad a un campo de una tabla en la base de datos.
    /// </summary>
    public interface IDbConverter
    {
        /// <summary>
        /// Obtiene el valor de un campo de la base de datos convertido al tipo de la propiedad.
        /// </summary>
        /// <param name="data">Valor obtenido de la base de datos.</param>
        /// <returns>El valor de la propiedad.</returns>
        Object ConverterFromDb(Object data);

        /// <summary>
        /// Obtiene el valor de la propiedad para traducirlo a un valor valido en la base de datos.
        /// </summary>
        /// <param name="property">Valor de la propiedad.</param>
        /// <returns>El valor para guardar en la base de datos.</returns>
        Object ConverterToDbData(Object property);
    }
}