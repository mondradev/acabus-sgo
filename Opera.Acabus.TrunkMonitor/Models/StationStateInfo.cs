using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Opera.Acabus.TrunkMonitor.Models
{
    /// <summary>
    /// Administra la información de estado de una estación en especifico.
    /// </summary>
    public class StationStateInfo : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Links"/>.
        /// </summary>
        private ICollection<Link> _links;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="MaximunAcceptablePing"/>.
        /// </summary>
        private UInt16 _maximunAcceptablePing;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="MaximunPing"/>.
        /// </summary>
        private UInt16 _maximunPing;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Messages"/>.
        /// </summary>
        private ObservableCollection<StationMessage> _message;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Ping"/>.
        /// </summary>
        private Int16 _ping;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Station"/>.
        /// </summary>
        private Station _station;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Status"/>.
        /// </summary>
        private LinkState _status;

        /// <summary>
        /// Crea una instancia nueva de <see cref="StationStateInfo"/>.
        /// </summary>
        /// <param name="owner">Estación la que será administrada por esta instancia.</param>
        public StationStateInfo(Station owner)
        {
            _station = owner;
            _maximunAcceptablePing = 600;
            _maximunPing = 100;
            _status = LinkState.GOOD;

            _message = new ObservableCollection<StationMessage>();
            _message.CollectionChanged += (sender, args) =>
            {
                Status = LinkState.GOOD;

                var message = _message.ToList();

                if (message.Count == 0)
                    return;

                var priorities = message.GroupBy(m => m.Priority);

                var high = priorities.FirstOrDefault(g => g.Key == Priority.HIGH)?.Count() ?? 0;
                var medium = priorities.FirstOrDefault(g => g.Key == Priority.MEDIUM)?.Count() ?? 0;
                var low = priorities.FirstOrDefault(g => g.Key == Priority.LOW)?.Count() ?? 0;

                medium += low / 2;
                high += medium / 2;

                if (high > 0)
                    Status = LinkState.BAD;
                else if (medium > 0 || low > 0)
                    Status = LinkState.MEDIUM;
                else
                    Status = LinkState.GOOD;

            };
        }

        /// <summary>
        /// Obtiene una lista de los enlaces de esta estación.
        /// </summary>
        public ICollection<Link> Links
            => _links ?? (_links = new ObservableCollection<Link>());

        /// <summary>
        /// Obtiene o establece el valor máximo aceptable para la duración del eco.
        /// </summary>
        public UInt16 MaximunAcceptablePing {
            get => _maximunAcceptablePing;
            set {
                _maximunAcceptablePing = value;
                OnPropertyChanged(nameof(MaximunAcceptablePing));
            }
        }

        /// <summary>
        /// Obtiene o establece el valor máximo optimo para la duración del eco.
        /// </summary>
        public UInt16 MaximunPing {
            get => _maximunPing;
            set {
                _maximunPing = value;
                OnPropertyChanged(nameof(MaximunPing));
            }
        }

        /// <summary>
        /// Obtiene una lista de los mensajes arrojados por la revisión de estación.
        /// </summary>
        public ICollection<StationMessage> Messages => _message;

        /// <summary>
        /// Obtiene o establece el valor del tiempo del eco.
        /// </summary>
        public Int16 Ping {
            get => _ping;
            set {
                _ping = value;
                OnPropertyChanged(nameof(Ping));
            }
        }

        /// <summary>
        /// Obtiene la estación administrada por esta instancia.
        /// </summary>
        public Station Station
            => _station;

        /// <summary>
        /// Obtiene o establece el estado de la estación.
        /// </summary>
        public LinkState Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// Representa la estructura de un mensaje de estación.
        /// </summary>
        public sealed class StationMessage
        {
            /// <summary>
            /// Crea una instancia nueva de <see cref="StationMessage"/>.
            /// </summary>
            /// <param name="device">Dispositivo involucrado en el mensaje.</param>
            /// <param name="message">Mensaje a crear.</param>
            /// <param name="priority">
            /// Prioridad del mensaje, de manera predeterminada es baja <see cref="Priority.LOW"/>.
            /// </param>
            public StationMessage(Device device, String message, Priority priority = Priority.LOW)
            {
                Priority = priority;
                Device = device;
                Message = message;
            }

            /// <summary>
            /// Obtiene el dispositivo involucrado.
            /// </summary>
            public Device Device { get; }

            /// <summary>
            /// Obtiene el mensaje.
            /// </summary>
            public String Message { get; }

            /// <summary>
            /// Obtiene la prioridad del mensaje.
            /// </summary>
            public Priority Priority { get; }

            /// <summary>
            /// Compara dos instancias de <see cref="StationMessage"/> y determina si son diferentes.
            /// </summary>
            /// <param name="left">Operando izquierdo.</param>
            /// <param name="right">Operando derecho.</param>
            /// <returns>Un valor true si son diferentes.</returns>
            public static bool operator !=(StationMessage left, StationMessage right)
            {
                if (left is null && right is null)
                    return false;

                if (left is null || right is null)
                    return true;

                return !left.Equals(right);
            }

            /// <summary>
            /// Compara dos instancias de <see cref="StationMessage"/> y determina si son iguales.
            /// </summary>
            /// <param name="left">Operando izquierdo.</param>
            /// <param name="right">Operando derecho.</param>
            /// <returns>Un valor true si son iguales.</returns>
            public static bool operator ==(StationMessage left, StationMessage right)
            {
                if (left is null && right is null)
                    return true;

                if (left is null || right is null)
                    return false;

                return left.Equals(right);
            }

            /// <summary>
            /// Compara dos instancia y determina si son iguales.
            /// </summary>
            /// <param name="obj">Otra instancia.</param>
            /// <returns>Un valor true si son iguales las instancias.</returns>
            public override bool Equals(object obj)
            {
                if (obj is null)
                    return false;

                if (obj.GetType() != GetType())
                    return false;

                var anotherObj = obj as StationMessage;

                return Device == anotherObj.Device && Message == anotherObj.Message;
            }

            /// <summary>
            /// Devuelve el código Hash de la instancia actual.
            /// </summary>
            /// <returns>Código Hash de la instancia.</returns>
            public override int GetHashCode()
                => Tuple.Create(Device, Message, Priority).GetHashCode();

            /// <summary>
            /// Devuelve una cadena que representa a la instancia actual.
            /// </summary>
            /// <returns>Una cadena que representa a la instancia.</returns>
            public override string ToString()
                => Message;
        }
    }
}