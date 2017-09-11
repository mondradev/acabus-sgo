using InnSyTech.Standard.Configuration;
using System;

namespace InnSyTech.Standard.Configurations
{
    /// <summary>
    /// Representa un constructor que facilita la creación de una instancia <see cref="ISetting"/>.
    /// </summary>
    public sealed class SettingBuilder
    {
        /// <summary>
        /// Instancia de configuración que se construye.
        /// </summary>
        private ISetting _setting;

        /// <summary>
        /// Crea una instancia de constructor de configuración.
        /// </summary>
        public SettingBuilder()
        {
            _setting = new Setting(null);
        }

        /// <summary>
        /// Convierte una instancia de configuración en <see cref="SettingBuilder"/>.
        /// </summary>
        /// <param name="setting">Configuración a convertir.</param>
        /// <returns>Constructor de configuración.</returns>
        public static SettingBuilder ToSettingBuilder(ISetting setting)
        {
            var settingBuilder = new SettingBuilder
            {
                _setting = setting
            };
            return settingBuilder;
        }

        /// <summary>
        /// Permite añadir un atributo nuevo.
        /// </summary>
        /// <param name="name">El nombre del atributo.</param>
        /// <param name="value">
        /// Valor del atributo, este debe ser convertible a cadena o instancia <see cref="ISetting"/>.
        /// </param>
        /// <returns>La instancia actual del constructor.</returns>
        public SettingBuilder AppendAttribute(String name, Object value)
        {
            (_setting as Setting).GetAttributes().Add(name, value);
            return this;
        }

        /// <summary>
        /// Permite añadir una configuración secundaria.
        /// </summary>
        /// <param name="name">El nombre de la configuración.</param>
        /// <param name="setting">Instancia del constructor de la configuración.</param>
        /// <returns>La instancia actual del constructor.</returns>
        public SettingBuilder CreateAndAppendSetting(String name, out SettingBuilder setting)
        {
            setting = new SettingBuilder();
            (_setting as Setting).GetAttributes().Add(name, setting.ToSetting());
            return this;
        }

        /// <summary>
        /// Permite reemplazar el valor de un atributo de la configuración.
        /// </summary>
        /// <param name="name">Nombre de la configuración.</param>
        /// <param name="value">
        /// Valor de la configuración, este debe ser convertible a cadena o una instancia <see cref="ISetting"/>.
        /// </param>
        /// <returns>La instancia actual del constructor.</returns>
        public SettingBuilder ReplaceValue(String name, Object value)
        {
            (_setting as Setting).GetAttributes()[name] = value;
            return this;
        }

        /// <summary>
        /// Obtiene la configuración creada.
        /// </summary>
        /// <returns>La configuración creada en el constructor actual.</returns>
        public ISetting ToSetting()
            => _setting;
    }
}