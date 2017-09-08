namespace Opera.Acabus.Configurations
{
    /// <summary>
    /// Define la información que se almacena en el archivo de configuración de la aplicación.
    /// </summary>
    internal class ConfigurableInfo
    {
        /// <summary>
        /// Nombre del archivo del ensamblado.
        /// </summary>
        public string AssemblyFilename { get; internal set; }

        /// <summary>
        /// Nombre del componente de configuración.
        /// </summary>
        public object Name { get; internal set; }

        /// <summary>
        /// Nombre de la clase que gestiona la configuración.
        /// </summary>
        public string TypeClass { get; internal set; }
    }
}