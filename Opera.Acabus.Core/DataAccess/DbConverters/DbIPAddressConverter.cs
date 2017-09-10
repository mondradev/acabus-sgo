using InnSyTech.Standard.Database;
using System.Net;

namespace Opera.Acabus.Core.DataAccess.DbConverters
{
    /// <summary>
    /// Convierte una instancia <see cref="IPAddress"/> a una cadena para almacenar en una base de datos.
    /// </summary>
    public sealed class DbIPAddressConverter : IDbConverter
    {
        /// <summary>
        /// Convierte la instancia pasada por parametro en una instancia <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="data">Una cadena con el formato valido para una dirección IP.</param>
        /// <returns>Una instancia <see cref="IPAddress"/>.</returns>
        public object ConverterFromDb(object data)
        {
            var ipString = data.ToString();
            if (IPAddress.TryParse(ipString, out IPAddress address))
                return address;
            return IPAddress.Parse("0.0.0.0");
        }

        /// <summary>
        /// Convierte la instancia <see cref="IPAddress"/> pasada por parametro en una cadena valida
        /// para almacenar en la base de datos.
        /// </summary>
        /// <param name="property">Una instancia <see cref="IPAddress"/>.</param>
        /// <returns>Una cadena que representa la instancia <see cref="IPAddress"/>.</returns>
        public object ConverterToDbData(object property)
        {
            if (property is IPAddress)
                return (property as IPAddress).ToString();
            return null;
        }
    }
}