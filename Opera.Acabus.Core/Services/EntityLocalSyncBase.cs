using InnSyTech.Standard.Database;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Clase base que mantiene los datos actualizados desde el servidor.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad a sincronzar.</typeparam>
    public abstract class EntityLocalSyncBase<T> : IEntityLocalSync where T : AcabusEntityBase
    {
        /// <summary>
        /// Función crear para una entidad.
        /// </summary>
        private const string CREATE_FUNC_NAME = "Create{0}";

        /// <summary>
        /// Función eliminar para una entidad.
        /// </summary>
        private const string DELETE_FUNC_NAME = "Delete{0}";

        /// <summary>
        /// Función obtener para una entidad.
        /// </summary>
        private const string DOWNLOAD_FUNC_NAME = "Download{0}";

        /// <summary>
        /// Función obtener por ID para una entidad.
        /// </summary>
        private const string GET_BY_ID_FUNC_NAME = "Get{0}ByID";

        /// <summary>
        /// Función actualizar para una unidad.
        /// </summary>
        private const string UPDATE_FUNC_NAME = "Update{0}";

        /// <summary>
        /// Provee a la propieded <see cref="Dependencies"/>.
        /// </summary>
        private readonly List<String> _dependencies;

        /// <summary>
        /// Crea una nueva entidad de sincronía.
        /// </summary>
        protected EntityLocalSyncBase(params String[] dependencies)
        {
            _dependencies = new List<string>(dependencies);

            LocalContext = AcabusDataContext.DbContext;
        }

        /// <summary>
        /// Evento que ocurre cuando se crea una instancia en el servidor.
        /// </summary>
        public event LocalSyncHandler Created;

        /// <summary>
        /// Evento que ocurre cuando se elimina una instancia del servidor.
        /// </summary>
        public event LocalSyncHandler Deleted;

        /// <summary>
        /// Evento que ocurre cuando se actualiza una instancia en el servidor.
        /// </summary>
        public event LocalSyncHandler Updated;

        /// <summary>
        /// Obtiene una lista de las entidades a las cual depende esta para sincronizar.
        /// </summary>
        public IReadOnlyList<string> Dependencies => _dependencies;

        /// <summary>
        /// Obtiene el nombre de la entidad a sincronizar.
        /// </summary>
        /// <returns>Nombre de la entidad.</returns>
        public string EntityName => GetType().BaseType.GenericTypeArguments[0]?.Name;

        /// <summary>
        /// Obtiene el contexto local (base de datos).
        /// </summary>
        public IDbSession LocalContext { get; }

        /// <summary>
        /// Obtiene el identificador del campo utilizado para el ID de la entidad.
        /// </summary>
        protected abstract int IDField { get; }

        /// <summary>
        /// Obtiene el nombre del módulo al que se realizan las peticiones.
        /// </summary>
        protected virtual string ModuleName => "Server Core";

        /// <summary>
        /// Obtiene el identificador del campo utilizado para almacenar esta entidad en bytes.
        /// </summary>
        protected abstract int SourceField { get; }

        /// <summary>
        /// Solicita la creación de una nueva instancia de la entidad actual, si es satisfactoria, se
        /// guarda localmente.
        /// </summary>
        /// <param name="instance">Instancia a crear en el servidor.</param>
        /// <returns>Un valor true si la instancia fue creada.</returns>
        public bool Create(ref T instance)
        {
            var remoteContext = new AppClient();

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            IAdaptiveMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName))
                message.SetModuleName(ModuleName);

            message.SetFunctionName(String.Format(CREATE_FUNC_NAME, typeof(T).Name));

            ToMessage(instance, message);

            message = remoteContext.SendMessage(message).Result;

            if (message.GetResponseCode() == AdaptiveMessageResponseCode.CREATED)
                instance = Deserialize(message);
            else
                throw message.GetException();

            if (!ServerContext.IsServer() && !LocalExists(instance) && LocalContext.Create(instance))
                Created?.Invoke(this, new LocalSyncEventArgs(instance, LocalSyncOperation.CREATE));

            return true;
        }

        /// <summary>
        /// Método no genérico que solicita la creación de una nueva instancia de la entidad actual, si es satisfactoria, se
        /// guarda localmente.
        /// </summary>
        /// <param name="instance">Instancia a crear en el servidor.</param>
        /// <returns>Un valor true si la instancia fue creada.</returns>
        bool IEntityLocalSync.Create(ref object instance)
        {
            T instanceCasted = instance as T;
            return Create(ref instanceCasted);
        }

        /// <summary>
        /// Solicita la eliminación de la instancia espeficicada en el servidor, de completarse
        /// satisfactoriamente se realiza de manera local.
        /// </summary>
        /// <param name="instance">Instancia a eliminar.</param>
        /// <returns>Un valor true si la instancia fue eliminada.</returns>
        public bool Delete(T instance)
        {
            var remoteContext = new AppClient();

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            IAdaptiveMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName))
                message.SetModuleName(ModuleName);

            message.SetFunctionName(String.Format(DELETE_FUNC_NAME, typeof(T).Name));

            message[IDField] = instance.ID;

            message = remoteContext.SendMessage(message).Result;

            if (message.GetResponseCode() != AdaptiveMessageResponseCode.OK && message.GetResponseCode() != AdaptiveMessageResponseCode.BAD_REQUEST)
                throw message.GetException();

            (instance as AcabusEntityBase).SetAsDeleted();

            if (!ServerContext.IsServer())
                if (LocalContext.Update(instance))
                    Deleted?.Invoke(this, new LocalSyncEventArgs(instance, LocalSyncOperation.DELETE));
                else
                    return false;

            return true;
        }

        /// <summary>
        /// Método no genérico que solicita la eliminación de la instancia espeficicada en el servidor, de completarse
        /// satisfactoriamente se realiza de manera local.
        /// </summary>
        /// <param name="instance">Instancia a eliminar.</param>
        /// <returns>Un valor true si la instancia fue eliminada.</returns>
        bool IEntityLocalSync.Delete(object instance)
            => Delete(instance as T);

        /// <summary>
        /// Descarga una instancia por ID desde el servidor y almacenda de manera local.
        /// </summary>
        /// <param name="id">Valor del identificador a descargar.</param>
        /// <returns>Un valor true si fue descargada satisfactoriamente.</returns>
        public bool DownloadByID(UInt64 id)
        {
            var remoteContext = new AppClient();

            IAdaptiveMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName))
                message.SetModuleName(ModuleName);

            message.SetFunctionName(String.Format(GET_BY_ID_FUNC_NAME, typeof(T).Name));

            message[IDField] = id;
            message = remoteContext.SendMessage(message).Result;

            bool downloaded;

            if (message.GetResponseCode() == AdaptiveMessageResponseCode.OK)
            {
                if (ServerContext.IsServer())
                    return true;

                T instance = Deserialize(message);

                if (instance == null)
                    throw new FormatException("Información no válida para obtener la instancia");

                bool exists = LocalExists(instance);

                if (exists)
                {
                    if (LocalContext.Read<T>().Any(i => i.ID == instance.ID && i.ModifyTime < instance.ModifyTime))
                    {
                        downloaded = LocalContext.Update(instance);

                        if (downloaded)
                            if (instance.Active)
                                Updated?.Invoke(this, new LocalSyncEventArgs(instance, LocalSyncOperation.UPDATE));
                            else
                                Deleted?.Invoke(this, new LocalSyncEventArgs(instance, LocalSyncOperation.DELETE));
                        else
                            throw new InvalidOperationException("No se logró actualizar de forma local");
                    }
                    else
                        downloaded = true;
                }
                else
                {
                    downloaded = LocalContext.Create(instance);

                    if (downloaded)
                        Created?.Invoke(this, new LocalSyncEventArgs(instance, LocalSyncOperation.CREATE));
                    else
                        throw new InvalidOperationException("No se logró crear de forma local");
                }
            }
            else
                throw message.GetException();

            return downloaded;
        }

        /// <summary>
        /// Elimina la instancia de manera local.
        /// </summary>
        /// <typeparam name="T">Tipo del identificador de la instancia.</typeparam>
        /// <param name="id">Identificador de la instancia a eliminar.</param>
        /// <returns>Un valor true si se eliminó de forma correcta.</returns>
        public bool LocalDeleteByID(UInt64 id)
        {
            T instance = LocalContext.Read<T>().Where(x => x.ID == id).FirstOrDefault();
            instance.SetAsDeleted();

            bool deleted = LocalContext.Update(instance);

            if (deleted)
                Deleted?.Invoke(this, new LocalSyncEventArgs(instance, LocalSyncOperation.DELETE));
            else
                throw new InvalidOperationException("No se logró eliminar la instancia locamente");

            return deleted;
        }

        /// <summary>
        /// Realiza una sincronización unidireccional desde servidor a local. Este proceso descarga
        /// toda la tabla a sincronizar y almance los datos faltantes.
        /// </summary>
        /// <param name="guiProgress">Controlador de progreso, útil para interfaces gráficas.</param>
        public void Pull(IProgress<float> guiProgress = null)
        {
            if (ServerContext.IsServer())
                return;

            var remoteContext = new AppClient();

            float currentProgress = 0;
            IAdaptiveMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName)) message.SetModuleName(ModuleName);

            T lastSync = LocalContext.Read<T>().OrderByDescending(x => x.ModifyTime).ToList().Take(1).FirstOrDefault();

            message.SetFunctionName(String.Format(DOWNLOAD_FUNC_NAME, typeof(T).Name));
            message.SetDateTime(25, lastSync.ModifyTime);

            AdaptiveMessageCollection<T> collection = remoteContext.SendMessage(message, Deserialize).Result;

            if (collection.Message.GetResponseCode() != AdaptiveMessageResponseCode.PARTIAL_CONTENT)
                throw collection.Message.GetException();

            using (collection)
            {
                Trace.TraceInformation(String.Format("Sincronizando la colección {0}, pendientes por actualizar {1}", this.EntityName, collection.Count));

                if (collection.Count == 0)
                {
                    guiProgress?.Report(100);
                    return;
                }

                foreach (T item in collection)
                {
                    bool exists = LocalExists(item);
                    bool createdOrUpdated = false;

                    Trace.TraceInformation(item.ToString());

                    if (exists)
                    {
                        if (LocalContext.Read<T>().Any(i => i.ID == item.ID && i.ModifyTime < item.ModifyTime))
                        {
                            createdOrUpdated = LocalContext.Update(item);

                            if (createdOrUpdated)
                                if (item.Active)
                                    Updated?.Invoke(this, new LocalSyncEventArgs(item, LocalSyncOperation.UPDATE));
                                else
                                    Deleted?.Invoke(this, new LocalSyncEventArgs(item, LocalSyncOperation.DELETE));
                        }
                        else
                            createdOrUpdated = true;
                    }
                    else
                    {
                        createdOrUpdated = LocalContext.Create(item);

                        if (createdOrUpdated)
                            Created?.Invoke(this, new LocalSyncEventArgs(item, LocalSyncOperation.CREATE));
                    }

                    if (!createdOrUpdated)
                        throw new InvalidOperationException("No se logró actualizar el elemento");

                    currentProgress++;
                    guiProgress?.Report(currentProgress / collection.Count / 100f);
                }
            }
        }

        /// <summary>
        /// Solicita la actualización de la instancia espeficicada en el servidor, de completarse
        /// satisfactoriamente se realiza de manera local.
        /// </summary>
        /// <param name="instance">Instancia a actualizar.</param>
        /// <returns>Un valor true si la instancia fue actualizada.</returns>
        public bool Update(T instance)
        {
            var remoteContext = new AppClient();

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            IAdaptiveMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName))
                message.SetModuleName(ModuleName);

            message.SetFunctionName(String.Format(UPDATE_FUNC_NAME, typeof(T).Name));
            message[SourceField] = Serialize(instance);

            message = remoteContext.SendMessage(message).Result;

            if (message.GetResponseCode() != AdaptiveMessageResponseCode.OK)
                throw message.GetException();

            if (!ServerContext.IsServer() && LocalContext.Update(instance))
            {
                Updated?.Invoke(this, new LocalSyncEventArgs(instance, LocalSyncOperation.UPDATE));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Método no genérico que solicita la actualización de la instancia espeficicada en el
        /// servidor, de completarse satisfactoriamente se realiza de manera local.
        /// </summary>
        /// <param name="instance">Instancia a actualizar.</param>
        /// <returns>Un valor true si la instancia fue actualizada.</returns>
        bool IEntityLocalSync.Update(object instance)
        => Update(instance as T);

        /// <summary>
        /// Obtiene una instancia a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Mensaje que contiene los datos a deserializar.</param>
        /// <returns>Una instancia creada desde la secuencia.</returns>
        protected abstract T Deserialize(IAdaptiveMessage source);

        /// <summary>
        /// Asigna las propiedades de la instancia en los campos del mensaje requeridos para la
        /// petición de creación.
        /// </summary>
        /// <param name="instance">Instancia a crear.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected abstract void ToMessage(T instance, IAdaptiveMessage message);

        /// <summary>
        /// Obtiene una secuencia de bytes a partir de una instancia especificada.
        /// </summary>
        /// <param name="instance">Instancia a serializar.</param>
        /// <returns>Una secuencia de bytes que representan a la instancia.</returns>
        protected abstract Byte[] Serialize(T instance);

        /// <summary>
        /// Valida si la instancia existe.
        /// </summary>
        /// <param name="instance">Instancia a validar.</param>
        /// <returns></returns>
        private bool LocalExists(T instance)
            => LocalContext.Read<T>().Any(y => y.ID == instance.ID);
    }
}