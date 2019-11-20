namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Representa un método que controlará el evento producido en la clase <see cref="IEntityLocalSync"/>.
    /// </summary>
    /// <param name="sender">Instancia que ejecutó el evento.</param>
    /// <param name="args">Argumentos del evento.</param>
    public delegate void LocalSyncHandler(object sender, LocalSyncEventArgs args);
}