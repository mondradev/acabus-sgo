using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Gui.Modules;
using System.Windows.Input;

namespace Opera.Acabus.Cctv.SubModules.AddIncidence.ViewModels
{
    /// <summary>
    /// Define el modelo de la vista <see cref="Views.CreateIncidenceView"/>
    /// </summary>
    public sealed class CreateIncidenceViewModel : ModuleViewerBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="IsMultiIncidence" />.
        /// </summary>
        private bool _isMultiIncidence;

        /// <summary>
        /// Crea una instancia nueva del modelo de la vista <see cref=" Views.CreateIncidenceView"/>.
        /// </summary>
        public CreateIncidenceViewModel()
        {
            DiscardCommand = new Command(p => Dispatcher.CloseDialog());
            ChangeModeCommand = new Command(p => IsMultiIncidence = !IsMultiIncidence);
        }

        /// <summary>
        /// Comando que cambia de modo de levantamiento de incidencias.
        /// </summary>
        public ICommand ChangeModeCommand { get; }

        /// <summary>
        /// Comando que descarta los cambios y finaliza el cuadro de dialogo.
        /// </summary>
        public ICommand DiscardCommand { get; }

        /// <summary>
        /// Obtiene o establece si es el levantamiento de multiples incidencias.
        /// </summary>
        public bool IsMultiIncidence {
            get => _isMultiIncidence;
            set {
                _isMultiIncidence = value;
                OnPropertyChanged(nameof(IsMultiIncidence));
                OnPropertyChanged(nameof(ModeButtonCaption));
            }
        }

        /// <summary>
        /// Obtiene el texto que muestra el botón de modo de incidencia.
        /// </summary>
        public string ModeButtonCaption => IsMultiIncidence ? "UNICA" : "MULTIPLE";
    }
}