using InnSyTech.Standard.Database;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

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
        public bool Create(ref T instance, out Exception reason)
        {
            var remoteContext = new AppClient();

            if (instance == null)
            {
                reason = new LocalSyncException("La instancia especificada es un valor nulo.", new ArgumentNullException(nameof(instance)));
                return false;
            }

            IMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName)) message[AcabusAdaptiveMessageFieldID.ModuleName.ToInt32()] = ModuleName;
            message[AcabusAdaptiveMessageFieldID.FunctionName.ToInt32()] = String.Format(CREATE_FUNC_NAME, typeof(T).Name);
            ToMessage(instance, message);

            Boolean created = false;
            Exception exception = null;

            T instanceR = instance;

            remoteContext.SendMessage(message, x =>
            {
                if (x.GetInt32(AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()) == 200)
                {
                    created = x.IsSet(22) ? x.GetBoolean(22) : false;
                    if (created)
                        instanceR = Deserialize(x.GetBytes(SourceField));
                    else
                        exception = new LocalSyncException("No se logró crear la instancia en el servidor.",
                           new InvalidOperationException(x.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));

                }
                else
                    exception = new LocalSyncException("Operación de creación realizada de manera erronea.",
                    new InvalidOperationException(x.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
            }).Wait();

            if ((reason = exception) != null)
                return false;

            instance = instanceR;

            if (created)
                if (!LocalExists(instance) && LocalContext.Create(instance))
                    Created?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.CREATE));



            return created;
        }

        /// <summary>
        /// Método no genérico que solicita la creación de una nueva instancia de la entidad actual, si es satisfactoria, se
        /// guarda localmente.
        /// </summary>
        /// <param name="instance">Instancia a crear en el servidor.</param>
        /// <returns>Un valor true si la instancia fue creada.</returns>
        bool IEntityLocalSync.Create(ref object instance, out Exception reason)
        {
            T instanceCasted = instance as T;
            return Create(ref instanceCasted, out reason);
        }

        /// <summary>
        /// Solicita la eliminación de la instancia espeficicada en el servidor, de completarse
        /// satisfactoriamente se realiza de manera local.
        /// </summary>
        /// <param name="instance">Instancia a eliminar.</param>
        /// <returns>Un valor true si la instancia fue eliminada.</returns>
        public bool Delete(T instance, out Exception reason)
        {
            var remoteContext = new AppClient();

            if (instance == null)
            {
                reason = new LocalSyncException("No se puede borrar un valor nulo.",
                      new ArgumentNullException());
                return false;
            }

            IMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName)) message[AcabusAdaptiveMessageFieldID.ModuleName.ToInt32()] = ModuleName;
            message[AcabusAdaptiveMessageFieldID.FunctionName.ToInt32()] = String.Format(DELETE_FUNC_NAME, typeof(T).Name);
            message[IDField] = instance.ID;

            Boolean deleted = false;
            Exception exception = null;

            remoteContext.SendMessage(message, (IMessage x) =>
            {
                if (x.GetInt32(AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()) == 200)
                {
                    deleted = x.IsSet(22) ? x.GetBoolean(22) : false;
                    if (!deleted)
                        exception = new LocalSyncException("No se logró eliminar en el servidor.",
                                          new InvalidOperationException(x.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
                }
                else
                    exception = new LocalSyncException("Operación de borrado realizada de manera erronea.",
                   new InvalidOperationException(x.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
            }).Wait();

            if ((reason = exception) != null)
                return false;

            (instance as AcabusEntityBase).Delete();

            if (deleted)
                if (LocalContext.Update(instance))
                    Deleted?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.DELETE));

            return deleted;
        }

        /// <summary>
        /// Método no genérico que solicita la eliminación de la instancia espeficicada en el servidor, de completarse
        /// satisfactoriamente se realiza de manera local.
        /// </summary>
        /// <param name="instance">Instancia a eliminar.</param>
        /// <returns>Un valor true si la instancia fue eliminada.</returns>
        bool IEntityLocalSync.Delete(object instance, out Exception reason)
            => Delete(instance as T, out reason);

        /// <summary>
        /// Descarga una instancia por ID desde el servidor y almacenda de manera local.
        /// </summary>
        /// <param name="id">Valor del identificador a descargar.</param>
        /// <returns>Un valor true si fue descargada satisfactoriamente.</returns>
        public bool DownloadByID(UInt64 id, out Exception reason)
        {
            var remoteContext = new AppClient();

            IMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName)) message[AcabusAdaptiveMessageFieldID.ModuleName.ToInt32()] = ModuleName;
            message[AcabusAdaptiveMessageFieldID.FunctionName.ToInt32()] = String.Format(GET_BY_ID_FUNC_NAME, typeof(T).Name);
            message[IDField] = id;

            bool downloaded = false;
            Exception exception = null;

            remoteContext.SendMessage(message, (IMessage x) =>
            {
                if (x.GetInt32(AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()) == 200)
                {
                    T instance = x.IsSet(SourceField) ? Deserialize(x.GetBytes(SourceField)) : null;

                    if (instance == null)
                    {
                        exception = new LocalSyncException(
                            x.IsSet(SourceField) ?
                            "Información no válida para obtener la instancia" :
                            "No se logró obtener alguna instancia desde el servidor: {ID=" + id + "}");
                        return;
                    }

                    bool exists = LocalExists(instance);

                    if (exists)
                    {
                        downloaded = LocalContext.Update(instance);

                        if (downloaded)
                            if (instance.Active)
                                Updated?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.UPDATE));
                            else
                                Deleted?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.DELETE));
                        else
                            exception = new LocalSyncException("No se logró actualizar de forma local {" + instance + "}");
                    }
                    else
                    {
                        downloaded = LocalContext.Create(instance);

                        if (downloaded)
                            Created?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.CREATE));
                        else
                            exception = new LocalSyncException("No se logró crear de forma local {" + instance + "}");
                    }
                }
                else
                    exception = new LocalSyncException("Operación de descarga de instancia por ID realizada de manera erronea.",
                    new InvalidOperationException(x.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
            }).Wait();

            reason = exception;

            return downloaded;
        }

        /// <summary>
        /// Elimina la instancia de manera local.
        /// </summary>
        /// <typeparam name="T">Tipo del identificador de la instancia.</typeparam>
        /// <param name="id">Identificador de la instancia a eliminar.</param>
        /// <returns>Un valor true si se eliminó de forma correcta.</returns>
        public bool LocalDeleteByID(UInt64 id, out Exception reason)
        {
            T instance = LocalContext.Read<T>().Where(x => x.ID == id).FirstOrDefault();
            instance.Delete();

            bool deleted = LocalContext.Update(instance);

            reason = null;

            if (deleted)
                Deleted?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.DELETE));
            else
                reason = new LocalSyncException("No se logró eliminar la instancia locamente: {" + instance + "}");

            return deleted;
        }

        /// <summary>
        /// Realiza una sincronización unidireccional desde servidor a local. Este proceso descarga
        /// toda la tabla a sincronizar y almance los datos faltantes.
        /// </summary>
        /// <param name="guiProgress">Controlador de progreso, útil para interfaces gráficas.</param>
        public void Pull(out Exception reason, IProgress<float> guiProgress = null)
        {
            var remoteContext = new AppClient();

            var hostName = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(hostName);
            var listIP = ipHost.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork);

            reason = null;

            if (listIP.Any(x => x.Equals(remoteContext.ServerIP)))
                return;

            float currentProgress = 0;
            List<T> list = new List<T>();
            IMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName)) message[AcabusAdaptiveMessageFieldID.ModuleName.ToInt32()] = ModuleName;

            T lastSync = LocalContext.Read<T>().OrderBy(x => x.ModifyTime).ToList().Take(1).FirstOrDefault();

            message[AcabusAdaptiveMessageFieldID.FunctionName.ToInt32()] = String.Format(DOWNLOAD_FUNC_NAME, typeof(T).Name);
            message.SetDateTime(25, lastSync.ModifyTime);

            Exception exception = null;

            remoteContext.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()) == 200)
                {
                    T instance = x.Current.IsSet(SourceField) ? Deserialize(x.Current.GetBytes(SourceField)) : null;

                    if (instance == null)
                    {
                        exception = new LocalSyncException(
                            x.Current.IsSet(SourceField) ?
                            "Información no valida para obtener la instancia" :
                            "No se logró obtener el elemento desde servidor");
                        return;
                    }

                    list.Add(instance);
                    currentProgress++;
                    guiProgress?.Report(currentProgress / x.Current.GetInt32(AcabusAdaptiveMessageFieldID.EnumerableCount.ToInt32()) / 2f * 100f);
                }
                else
                    exception = new LocalSyncException("No se procesó correctamente la petición.",
                    new InvalidOperationException(x.Current.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
            }).Wait();

            if ((reason = exception) != null)
                return;

            currentProgress = 0;

            foreach (T x in list)
            {
                bool exists = LocalExists(x);
                bool createdOrUpdated = false;

                if (exists)
                {
                    createdOrUpdated = LocalContext.Update(x);

                    if (createdOrUpdated)
                        if (x.Active)
                            Updated?.Invoke(this, new LocalSyncArgs(x, LocalSyncOperation.UPDATE));
                        else
                            Deleted?.Invoke(this, new LocalSyncArgs(x, LocalSyncOperation.DELETE));
                }
                else
                {
                    createdOrUpdated = LocalContext.Create(x);

                    if (createdOrUpdated)
                        Created?.Invoke(this, new LocalSyncArgs(x, LocalSyncOperation.CREATE));
                }

                if (!createdOrUpdated)
                {
                    reason = new LocalSyncException("No se logró guardar o actualizar la instancia descargada: " + x);
                    return;
                }

                currentProgress++;
                guiProgress?.Report(50f + currentProgress / list.Count / 2f * 100f);
            }
        }

        /// <summary>
        /// Solicita la actualización de la instancia espeficicada en el servidor, de completarse
        /// satisfactoriamente se realiza de manera local.
        /// </summary>
        /// <param name="instance">Instancia a actualizar.</param>
        /// <returns>Un valor true si la instancia fue actualizada.</returns>
        public bool Update(T instance, out Exception reason)
        {
            var remoteContext = new AppClient();

            if (instance == null)
            {
                reason = new LocalSyncException("La instancia a actualizar debe ser especificada.",
                      new ArgumentNullException());
                return false;
            }

            IMessage message = remoteContext.CreateMessage();

            if (!String.IsNullOrEmpty(ModuleName)) message[AcabusAdaptiveMessageFieldID.ModuleName.ToInt32()] = ModuleName;
            message[AcabusAdaptiveMessageFieldID.FunctionName.ToInt32()] = String.Format(UPDATE_FUNC_NAME, typeof(T).Name);
            message[SourceField] = Serialize(instance);

            Boolean updated = false;
            Exception exception = null;

            remoteContext.SendMessage(message, (IMessage x) =>
             {
                 if (x.GetInt32(AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()) == 200)
                 {
                     updated = x.IsSet(22) ? x.GetBoolean(22) : false;

                     if (!updated)
                         exception = new LocalSyncException("No se logró actualizar los atributos de la instancia.",
                     new InvalidOperationException(x.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
                 }
                 else
                     exception = new LocalSyncException("Operación de actualización realizada de manera erronea.",
                 new InvalidOperationException(x.GetString(AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32())));
             }).Wait();

            if ((reason = exception) != null)
                return false;

            if (updated)
                if (LocalContext.Update(instance))
                    Updated?.Invoke(this, new LocalSyncArgs(instance, LocalSyncOperation.UPDATE));

            return updated;
        }

        /// <summary>
        /// Método no genérico que solicita la actualización de la instancia espeficicada en el
        /// servidor, de completarse satisfactoriamente se realiza de manera local.
        /// </summary>
        /// <param name="instance">Instancia a actualizar.</param>
        /// <returns>Un valor true si la instancia fue actualizada.</returns>
        bool IEntityLocalSync.Update(object instance, out Exception reason)
        => Update(instance as T, out reason);

        /// <summary>
        /// Obtiene una instancia a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Secuencia de bytes origen.</param>
        /// <returns>Una instancia creada desde la secuencia.</returns>
        protected abstract T Deserialize(Byte[] source);

        /// <summary>
        /// Asigna las propiedades de la instancia en los campos del mensaje requeridos para la
        /// petición de creación.
        /// </summary>
        /// <param name="instance">Instancia a crear.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected abstract void ToMessage(T instance, IMessage message);

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
            => LocalContext.Read<T>().Where(y => y.ID == instance.ID).ToList().Any();
    }
}