namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Clase no generica que representa los parametros utilizados durante un evento de la clase <see cref="EntityLocalSyncBase{T}"/>
    /// </summary>
    public class LocalSyncArgs
    {
        /// <summary>
        /// Crea una nueva instancia.
        /// </summary>
        /// <param name="instance">Instancia que produce el evento.</param>
        /// <param name="operation">Operación que produce el evento.</param>
        public LocalSyncArgs(object instance, LocalSyncOperation operation)
        {
            Value = instance;
            Operation = operation;
        }

        /// <summary>
        /// Operación que desencadenó el evento.
        /// </summary>
        public LocalSyncOperation Operation { get; }

        /// <summary>
        /// Instancia involucrada en el evento.
        /// </summary>
        public object Value { get; }
    }
}