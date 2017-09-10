using System;

namespace InnSyTech.Standard.Database.Utils
{
    /// <summary>
    /// Define un convertidor de datos para enumeraciones a enteros y así poder ingresarlos a la base de datos.
    /// </summary>
    /// <typeparam name="T">Tipo de la enumeración.</typeparam>
    public class DbEnumConverter<T> : IDbConverter
    {
        /// <summary>
        /// Obtiene el valor de un campo de la base de datos convertido al tipo <see cref="T"/>.
        /// </summary>
        /// <param name="data">Valor obtenido de la base de datos.</param>
        /// <returns>El valor del tipo <see cref="T"/></returns>
        public object ConverterFromDb(object data)
        {
            if (data.GetType() != typeof(Int16) && data.GetType() != typeof(Int32) && data.GetType() != typeof(Int64))
                throw new ArgumentException("El tipo de dato extraido de la base de datos debe ser Enteros (Int16, Int32 o Int64).");

            return (T)Convert.ChangeType(data, typeof(Int32));
        }

        /// <summary>
        /// Obtiene el valor de la propiedad para traducirlo a un valor del tipo <see cref="Int32"/>.
        /// </summary>
        /// <param name="property">Valor de la propiedad.</param>
        /// <returns>Un valor del tipo <see cref="Int32"/>.</returns>
        public object ConverterToDbData(object property)
        {
            if (property.GetType() != typeof(T))
                throw new ArgumentException("El tipo de dato de la propiedad debe ser igual al definido por T.");

            return (Int32)property;
        }
    }
}
