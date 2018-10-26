using System;
using System.Reflection;
using System.Text;

namespace InnSyTech.Standard.Net.Notifications.Push
{
    /// <summary>
    /// Provee la estructura base de las notificaciones que serán enviadas.
    /// </summary>
    public sealed class PushNotification<T> where T : class, IPushData
    {
        /// <summary>
        /// Crea una nueva instancia de notificación push
        /// </summary>
        /// <param name="data">Datos a trasmitir.</param>
        public PushNotification(T data)
        {
            Data = data;
        }

        /// <summary>
        /// Obtiene los datos que representa la notificación.
        /// </summary>
        public T Data { get; private set; }

        /// <summary>
        /// Obtiene una instancia <see cref="PushNotification{T}"/> a partir de una secuenciade Bytes.
        /// </summary>
        /// <param name="src">Secuencia de bytes origen.</param>
        /// <returns>Una instancia de notificación.</returns>
        public static PushNotification<DataType> FromBytes<DataType>(Byte[] src) where DataType : class, T
            => Parse<DataType>(Encoding.UTF8.GetString(src));

        /// <summary>
        /// Convierte la representación de texto en una instancia.
        /// </summary>
        /// <param name="src">Texto con formato valido.</param>
        /// <returns>Una instancia de notificación.</returns>
        public static PushNotification<DataType> Parse<DataType>(String src) where DataType : class, T
        {
            PushNotification<DataType> push = Activator.CreateInstance<PushNotification<DataType>>();

            Type instanceType = typeof(DataType);
            MethodInfo methodParse = instanceType.GetMethod("Parse", BindingFlags.Static);

            if (methodParse == null)
                throw new InvalidOperationException($"La clase '{typeof(DataType).FullName}' no tiene implementado un método Parse " +
                    "utilizado para convertir la representación en una instancia.");

            DataType dataInstance = methodParse?.Invoke(null, new object[] { src }) as DataType;

            push.Data = dataInstance;

            return push;
        }

        /// <summary>
        /// Obtiene los bytes de la notificación.
        /// </summary>
        /// <returns>Una secuencia de bytes.</returns>
        public Byte[] ToBytes()
            => Encoding.UTF8.GetBytes(ToString());

        /// <summary>
        /// Representa la notificación en texto, el cual deberá existir la posibilidad de convertilo a instancia a través de la función <see cref="Parse"/>.
        /// </summary>
        /// <returns></returns>
        public override String ToString()
            => Data.ToString();
    }
}