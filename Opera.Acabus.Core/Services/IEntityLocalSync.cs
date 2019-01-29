using System;
using System.Collections.Generic;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Provee de la estructura básica para la creación de un monitor de sincronización de entidades.
    /// </summary>
    public interface IEntityLocalSync
    {
        /// <summary>
        /// Evento que ocurre cuando se crea una instancia en el servidor.
        /// </summary>
        event LocalSyncHandler Created;

        /// <summary>
        /// Evento que ocurre cuando se elimina una instancia del servidor.
        /// </summary>
        event LocalSyncHandler Deleted;

        /// <summary>
        /// Evento que ocurre cuando se actualiza una instancia en el servidor.
        /// </summary>
        event LocalSyncHandler Updated;

        /// <summary>
        /// Obtiene una lista de todos las entidades que deben estar sincronizadas antes de
        /// sincronizar esta.
        /// </summary>
        IReadOnlyList<String> Dependencies { get; }

        /// <summary>
        /// Obtiene el nombre de la entidad a sincronizar.
        /// </summary>
        /// <returns>Nombre de la entidad.</returns>
        String EntityName { get; }

        /// <summary>
        /// Solicita la creación de una nueva instancia de la entidad actual,
        /// si es satisfactoria, se guarda localmente.
        /// </summary>
        /// <param name="instance">Instancia a crear en el servidor.</param>
        /// <returns>Un valor true si la instancia fue creada.</returns>
        bool Create<T>(ref T instance) where T : class;

        /// <summary>
        /// Solicita la eliminación de la instancia espeficicada en el
        /// servidor, de completarse satisfactoriamente se realiza de manera local.
        /// </summary>
        /// <param name="instance">Instancia a eliminar.</param>
        /// <returns>Un valor true si la instancia fue eliminada.</returns>
        bool Delete<T>(T instance) where T : class;

        /// <summary>
        /// Descarga una instancia por ID desde el servidor y almacenda de manera local.
        /// </summary>
        /// <param name="id">Valor del identificador a descargar.</param>
        /// <returns>Un valor true si fue descargada satisfactoriamente.</returns>
        bool DownloadByID(UInt64 id);

        /// <summary>
        /// Elimina la instancia de manera local.
        /// </summary>
        /// <param name="id">Identificador de la instancia a eliminar.</param>
        /// <returns>Un valor true si se eliminó de forma correcta.</returns>
        bool LocalDeleteByID(UInt64 id);

        /// <summary>
        /// Realiza una sincronización unidireccional desde servidor a local. Este proceso descarga
        /// toda la tabla a sincronizar y almance los datos faltantes.
        /// </summary>
        /// <param name="guiProgress">Controlador de progreso, útil para interfaces gráficas.</param>
        void Pull(IProgress<float> guiProgress = null);

        /// <summary>
        /// Solicita la actualización de la instancia espeficicada en el
        /// servidor, de completarse satisfactoriamente se realiza de manera local.
        /// </summary>
        /// <param name="instance">Instancia a actualizar.</param>
        /// <returns>Un valor true si la instancia fue actualizada.</returns>
        bool Update<T>(T instance) where T : class;

    }
}