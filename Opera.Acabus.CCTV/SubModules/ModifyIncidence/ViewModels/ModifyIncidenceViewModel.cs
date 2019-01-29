using InnSyTech.Standard.Mvvm;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Cctv.DataAccess;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Gui.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Opera.Acabus.Cctv.SubModules.ModifyIncidence.ViewModels
{
    /// <summary>
    /// Representa el modelo de la vista <see cref="Views.ModifyIncidenceView"/>.
    /// </summary>
    public sealed class ModifyIncidenceViewModel : ModuleViewerBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Business" />.
        /// </summary>
        private IEnumerable<String> _business;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="NewWhoReporting" />.
        /// </summary>
        private String _newWhoReporting;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Observations" />.
        /// </summary>
        private String _observations;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedIncidence" />.
        /// </summary>
        private Incidence _selectedIncidence;

        /// <summary>
        /// Crea una nueva instancia de <see cref="ModifyIncidenceViewModel"/>.
        /// </summary>
        public ModifyIncidenceViewModel()
        {
            _business = AcabusDataContext.ConfigContext["Cctv"]?
                .GetSetting("business")?
                .GetSettings("business")?
                .Select(s => s.ToString("value"))
                .OrderBy(s => s);

            UpdateIncidenceCommand = new Command(UpdateIncidence, CanUpdate);
            DiscardCommand = new Command(Dispatcher.CloseDialog);
        }

        /// <summary>
        /// Obtiene un listado de las empresas que reportan las incidencias.
        /// </summary>
        public IEnumerable<String> Business => _business;

        /// <summary>
        /// Comando que descarta los cambios realizados en el cuadro de dialogo.
        /// </summary>
        public ICommand DiscardCommand { get; }

        /// <summary>
        /// Obtiene o establece el nuevo valor de la propiedad <see cref="Incidence.WhoReporting"/>.
        /// </summary>
        public String NewWhoReporting {
            get => _newWhoReporting;
            set {
                _newWhoReporting = value;
                OnPropertyChanged(nameof(NewWhoReporting));
            }
        }

        /// <summary>
        /// Obtiene o establece el nuevo valor de la propiedad <see cref="Incidence.FaultObservations"/>.
        /// </summary>
        public String Observations {
            get => _observations;
            set {
                _observations = value;
                OnPropertyChanged(nameof(Observations));
            }
        }

        /// <summary>
        /// Obtiene o establece la incidencia a modificar sus propiedades.
        /// </summary>
        public Incidence SelectedIncidence {
            get => _selectedIncidence;
            set {
                _selectedIncidence = value;
                _observations = value?.FaultObservations;
                _newWhoReporting = value?.WhoReporting;
                OnPropertyChanged(nameof(SelectedIncidence));
                OnPropertyChanged(nameof(Observations));
                OnPropertyChanged(nameof(NewWhoReporting));
            }
        }

        /// <summary>
        /// Comando que aplica los cambios realizados en el cuadro de dialogo.
        /// </summary>
        public ICommand UpdateIncidenceCommand { get; }

        /// <summary>
        /// Ocurre cuando se asigna un valor a alguna propiedad del modelo, permitiendo ser validadas y generando mensajes de error de ser necesario.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad a evaluar.</param>
        protected override void OnValidation(string propertyName)
        {
            if (nameof(NewWhoReporting) == propertyName)
                if (String.IsNullOrEmpty(NewWhoReporting))
                    AddError(nameof(NewWhoReporting), "Seleccione la entidad que reporta la incidencia.");
        }

        /// <summary>
        /// Determina si es posible actualizar la incidencia.
        /// </summary>
        /// <param name="arg"> Parametro del comando. </param>
        /// <returns> Un valor true para habilitar el comando de actualización. </returns>
        private bool CanUpdate(object arg)
        {
            ValidateProperty(nameof(NewWhoReporting));

            if (NewWhoReporting == SelectedIncidence?.WhoReporting
                && Observations == SelectedIncidence?.FaultObservations)
                return false;

            return !HasErrors;
        }

        /// <summary>
        /// Actualiza los valores de la incidencia.
        /// </summary>
        /// <param name="obj">Parametro del comando.</param>
        private void UpdateIncidence(object obj)
        {
            var oldObservations = SelectedIncidence.FaultObservations;
            var oldWhoReporting = SelectedIncidence.WhoReporting;

            try
            {
                SelectedIncidence.FaultObservations = Observations;
                SelectedIncidence.WhoReporting = NewWhoReporting;

                AcabusDataContext.DbContext.Update(SelectedIncidence);

                CctvContext.RefreshData();

                Dispatcher.CloseDialog();
            }
            catch (Exception ex)
            {
                SelectedIncidence.FaultObservations = oldObservations;
                SelectedIncidence.WhoReporting = oldWhoReporting;

                Trace.WriteLine(ex.PrintMessage().JoinLines(), "ERROR");
                Dispatcher.CloseDialog();
                ShowMessage($"No se logró modificar la incidencia {SelectedIncidence.ID}");
            }
        }
    }
}