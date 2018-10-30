using InnSyTech.Standard.Database;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Clase base que mantiene los datos actualizados desde el servidor.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad a sincronzar.</typeparam>
    public abstract class EntityLocalSyncBase<T> : IEntityLocalSync where T : class
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
        /// Función obtener por ID para una entidad.
        /// </summary>
        private const string GET_BY_ID_FUNC_NAME = "Get{0}ByID";

        /// <summary>
        /// Función obtener para una entidad.
        /// </summary>
        private const string GET_FUNC_NAME = "Get{0}";

        /// <summary>
        /// Función actualizar para una unidad.
        /// </summary>
        private const string UPDATE_FUNC_NAME = "Update{0}";

        /// <summary>
        /// Crea una nueva entidad de sincronía.
        /// </summary>
        public EntityLocalSyncBase()
        {
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
        protected virtual string ModuleName { get; }

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
                throw new LocalSyncException("Instancia no especificada.", new ArgumentNullException());

            IMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName)) message[AcabusAdaptiveMessageFieldID.ModuleName.ToInt32()] = ModuleName;
            message[AcabusAdaptiveMessageFieldID.FunctionName.ToInt32()] = String.Format(CREATE_FUNC_NAME, typeof(T).Name);
            InstanceToMessage(instance, message);

            Boolean created = false;

            T instanceR = instance;

            remoteContext.SendMessage(message, x =>
            {
                if (x.GetInt32(AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()) == 200)
                {
                    created = x.IsSet(22) ? x.GetBoolean(22) : false;
                    if (created)
                        instanceR = FromBytes(x.GetBytes(SourceField));
                    else throw new LocalSyncException("No se creo la instancia en el servidor.",
                        new InvalidOperationException(x.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
                }
                else throw new LocalSyncException("No se procesó correctamente la petición.",
                    new InvalidOperationException(x.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
            }).Wait();

            instance = instanceR;

            if (created)
                if (!Exists(LocalContext.Read<T>(), instance) && LocalContext.Create(instance))
                    Created?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.CREATE));

            return created;
        }

        /// <summary>
        /// Método no genérico que solicita la creación de una nueva instancia de la entidad actual, si es satisfactoria, se
        /// guarda localmente.
        /// </summary>
        /// <param name="instance">Instancia a crear en el servidor.</param>
        /// <returns>Un valor true si la instancia fue creada.</returns>
        bool IEntityLocalSync.Create<T2>(ref T2 instance)
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
                throw new LocalSyncException("La instancia a eliminar debe ser especificada.",
                    new ArgumentNullException());

            IMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName)) message[AcabusAdaptiveMessageFieldID.ModuleName.ToInt32()] = ModuleName;
            message[AcabusAdaptiveMessageFieldID.FunctionName.ToInt32()] = String.Format(DELETE_FUNC_NAME, typeof(T).Name);
            message[IDField] = GetID(instance);

            Boolean deleted = false;

            remoteContext.SendMessage(message, (IMessage x) =>
            {
                if (x.GetInt32(AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()) == 200)
                    deleted = x.IsSet(22) ? x.GetBoolean(22) : false;
                else throw new LocalSyncException("No se procesó correctamente la petición.",
                   new InvalidOperationException(x.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
            }).Wait();

            if (deleted)
                if (LocalContext.Delete(instance))
                    Deleted?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.DELETE));

            return deleted;
        }

        /// <summary>
        /// Método no genérico que solicita la eliminación de la instancia espeficicada en el servidor, de completarse
        /// satisfactoriamente se realiza de manera local.
        /// </summary>
        /// <param name="instance">Instancia a eliminar.</param>
        /// <returns>Un valor true si la instancia fue eliminada.</returns>
        bool IEntityLocalSync.Delete<T2>(T2 instance)
            => Delete(instance as T);

        /// <summary>
        /// Descarga una instancia por ID desde el servidor y almacenda de manera local.
        /// </summary>
        /// <typeparam name="TID">Tipo del identificador.</typeparam>
        /// <param name="id">Valor del identificador a descargar.</param>
        /// <returns>Un valor true si fue descargada satisfactoriamente.</returns>
        public bool DownloadByID<TID>(TID id)
        {
            var remoteContext = new AppClient();

            IMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName)) message[AcabusAdaptiveMessageFieldID.ModuleName.ToInt32()] = ModuleName;
            message[AcabusAdaptiveMessageFieldID.FunctionName.ToInt32()] = String.Format(GET_BY_ID_FUNC_NAME, typeof(T).Name);
            message[IDField] = id;

            bool downloaded = false;

            remoteContext.SendMessage(message, (IMessage x) =>
            {
                if (x.GetInt32(AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()) == 200)
                {
                    T instance = x.IsSet(SourceField) ? FromBytes(x.GetBytes(SourceField)) : null;

                    if (instance == null)
                        throw new LocalSyncException("No se logró descargar la instancia por ID.");

                    bool exists = Exists(LocalContext.Read<T>(), instance);

                    if (exists)
                    {
                        downloaded = LocalContext.Update(instance);

                        if (downloaded)
                            Updated?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.UPDATE));
                    }
                    else
                    {
                        downloaded = LocalContext.Create(instance);

                        if (downloaded)
                            Created?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.CREATE));
                    }
                }
                else throw new LocalSyncException("No se procesó correctamente la petición.",
                    new InvalidOperationException(x.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
            }).Wait();

            return downloaded;
        }

        /// <summary>
        /// Realiza una sincronización unidireccional desde servidor a local. Este proceso descarga
        /// toda la tabla a sincronizar y almance los datos faltantes.
        /// </summary>
        /// <param name="guiProgress">Controlador de progreso, útil para interfaces gráficas.</param>
        /// <returns>Un valor true si se sincronizó de manera satisfactoria.</returns>
        public bool FullUniSync(IProgress<float> guiProgress = null)
        {
            var remoteContext = new AppClient();

            float currentProgress = 0;
            List<T> list = new List<T>();
            IMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName)) message[AcabusAdaptiveMessageFieldID.ModuleName.ToInt32()] = ModuleName;
            message[AcabusAdaptiveMessageFieldID.FunctionName.ToInt32()] = String.Format(GET_FUNC_NAME, typeof(T).Name);

            remoteContext.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()) == 200)
                {
                    T instance = x.Current.IsSet(SourceField) ? FromBytes(x.Current.GetBytes(SourceField)) : null;

                    if (instance == null)
                        throw new LocalSyncException("No se logró obtener la instancia.");

                    list.Add(instance);
                    currentProgress++;
                    guiProgress?.Report(currentProgress / x.Current.GetInt32(AcabusAdaptiveMessageFieldID.EnumerableCount.ToInt32()) / 2f * 100f);
                }
                else throw new LocalSyncException("No se procesó correctamente la petición.",
                    new InvalidOperationException(x.Current.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
            }).Wait();

            currentProgress = 0;

            list.ForEach(x =>
            {
                bool exists = Exists(LocalContext.Read<T>(), x);
                bool createdOrUpdated = false;

                if (exists)
                {
                    createdOrUpdated = LocalContext.Update(x);

                    if (createdOrUpdated)
                        Updated?.Invoke(this, new LocalSyncArgs(x, LocalSyncOperation.UPDATE));
                }
                else
                {
                    createdOrUpdated = LocalContext.Create(x);

                    if (createdOrUpdated)
                        Created?.Invoke(this, new LocalSyncArgs(x, LocalSyncOperation.CREATE));
                }

                if (!createdOrUpdated)
                    throw new LocalSyncException("No se logró guardar o actualizar la instancia descargada: " + x);

                currentProgress++;
                guiProgress?.Report(50f + currentProgress / list.Count / 2f * 100f);
            });

            return true;
        }

        /// <summary>
        /// Elimina la instancia de manera local.
        /// </summary>
        /// <typeparam name="T">Tipo del identificador de la instancia.</typeparam>
        /// <param name="id">Identificador de la instancia a eliminar.</param>
        /// <returns>Un valor true si se eliminó de forma correcta.</returns>
        public bool LocalDeleteByID<TID>(TID id)
        {
            T instance = LocalReadByID(id);
            bool deleted = LocalContext.Delete(instance);

            if (deleted)
                Deleted?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.DELETE));

            return deleted;
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
                throw new LocalSyncException("La instancia a actualizar debe ser especificada.",
                    new ArgumentNullException());

            IMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName)) message[AcabusAdaptiveMessageFieldID.ModuleName.ToInt32()] = ModuleName;
            message[AcabusAdaptiveMessageFieldID.FunctionName.ToInt32()] = String.Format(UPDATE_FUNC_NAME, typeof(T).Name);
            message[SourceField] = ToBytes(instance);

            Boolean updated = false;

            remoteContext.SendMessage(message, (IMessage x) =>
             {
                 if (x.GetInt32(AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()) == 200)
                     updated = x.IsSet(22) ? x.GetBoolean(22) : false;
                 else throw new LocalSyncException("No se procesó correctamente la petición.",
                    new InvalidOperationException(x.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
             }).Wait();

            if (updated)
                if (LocalContext.Update(instance))
                    Updated?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.UPDATE));

            return updated;
        }

        /// <summary>
        /// Método no genérico que solicita la actualización de la instancia espeficicada en el servidor, de completarse
        /// satisfactoriamente se realiza de manera local.
        /// </summary>
        /// <param name="instance">Instancia a actualizar.</param>
        /// <returns>Un valor true si la instancia fue actualizada.</returns>
        bool IEntityLocalSync.Update<T2>(T2 instance)
            => Update(instance as T);

        /// <summary>
        /// Determina si la instancia especificada existe en la secuencia.
        /// </summary>
        /// <param name="source">Secuencia de datos a analizar.</param>
        /// <param name="instance">Instancia a buscar.</param>
        /// <returns>Un valor true si la instancia existe en la secuencia.</returns>
        protected abstract bool Exists(IQueryable<T> source, T instance);

        /// <summary>
        /// Obtiene una instancia a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Secuencia de bytes origen.</param>
        /// <returns>Una instancia creada desde la secuencia.</returns>
        protected abstract T FromBytes(Byte[] source);

        /// <summary>
        /// Obtiene el valor del identificador de la entidad.
        /// </summary>
        /// <param name="instance">Instancia a obtener el identificador.</param>
        /// <returns>Un valor que representa el identificador unico de la instancia.</returns>
        protected abstract object GetID(T instance);

        /// <summary>
        /// Asigna las propiedades de la instancia en los campos del mensaje requeridos para la
        /// petición de creación.
        /// </summary>
        /// <param name="instance">Instancia a crear.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected abstract void InstanceToMessage(T instance, IMessage message);

        /// <summary>
        /// Obtiene una instancia almacenada en el contexto local la cual coincide con el identificador especificado.
        /// </summary>
        /// <typeparam name="TID">Tipo del identificador.</typeparam>
        /// <param name="id">Identificador de la instancia.</param>
        /// <returns>Instancia leida del contexto local.</returns>
        protected abstract T LocalReadByID<TID>(TID id);

        /// <summary>
        /// Obtiene una secuencia de bytes a partir de una instancia especificada.
        /// </summary>
        /// <param name="instance">Instancia a serializar.</param>
        /// <returns>Una secuencia de bytes que representan a la instancia.</returns>
        protected abstract Byte[] ToBytes(T instance);
    }
}