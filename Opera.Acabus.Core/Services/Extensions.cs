using InnSyTech.Standard.Utils;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Opera.Acabus.Core.Services
{
    public static class Extensions
    {
        /// <summary>
        /// Convierte una cadena con notación de objetos javascript (JSON) en una instancia <see cref="Credential"/>.
        /// </summary>
        public static Credential ParseFromJson(String json)
        {
            json = json.Trim();

            if (!json.StartsWith("{"))
                throw new FormatException("La cadena JSON no presenta el formato adecuado.");

            if (!json.EndsWith("}"))
                throw new FormatException("La cadena JSON no presenta el formato adecuado.");

            json = json.Substring(1, json.Length - 1);

            var properties = json.Split(',');

            if (properties.Length < 1)
                throw new FormatException("La cadena JSON no presenta el formato adecuado.");

            var type = typeof(Credential);
            var instance = Activator.CreateInstance<Credential>();

            foreach (var property in properties)
            {
                string[] keyAndValue = property.Split(':');

                if (keyAndValue.Length < 2)
                    throw new FormatException("La cadena JSON no presenta el formato adecuado.");

                if (!Regex.Match(keyAndValue[0], "[0-9A-Za-z]*").Success)
                    throw new FormatException("La cadena JSON no presenta el formato adecuado.");

                if (!Regex.Match(keyAndValue[1], "[0-9A-Za-z]*").Success)
                    throw new FormatException("La cadena JSON no presenta el formato adecuado.");

                var propertyRef = type.GetProperty(keyAndValue[0]);

                if (propertyRef is null)
                    throw new FormatException("La cadena JSON no presenta el formato adecuado.");

                propertyRef.SetValue(instance, Convert.ChangeType(keyAndValue[1], propertyRef.PropertyType));
            }

            return instance;
        }

        /// <summary>
        /// Convierte una instancia <see cref="Credential"/> en notación de objectos Javascript (JSON).
        /// </summary>
        /// <returns>Una cadena JSON que representa la instancia convertida.</returns>
        public static string ToJson(this Credential credential)
        {
            StringBuilder json = new StringBuilder();
            json.AppendFormat("{Username:{0},Password:{1},Type:{2},IsRoot:{3}}", credential.Username, credential.Password, credential.Type, credential.IsRoot);
            return json.ToString();
        }
    }
}