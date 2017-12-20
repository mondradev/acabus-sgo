using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Cctv.Helpers;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opera.Acabus.Cctv.ViewModels
{
    /// <summary>
    /// Modelo de la vista <see cref="Views.CctvReportsView"/>.
    /// </summary>
    public sealed class CctvReportsViewModel : ModuleViewerBase
    {
        /// <summary>
        /// Proveedor de los servicios de Cctv.
        /// </summary>
        private CctvModule _cctvManager;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="KeywordToSearchIncidence"/>
        /// </summary>
        private String _keywordToSearchIncidence;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="OpenedFolioToSearch"/>
        /// </summary>
        private String _openedFolioToSearch;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedIncidences"/>.
        /// </summary>
        private ICollection<Incidence> _selectedIncidences;

        /// <summary>
        /// Indica si hay una actualización en curso.
        /// </summary>
        private bool _updatingIncidences;

        /// <summary>
        /// Crea una nueva instancia de modelo de la vista.
        /// </summary>
        public CctvReportsViewModel()
        {
            if (!AcabusDataContext.GetService("Cctv_Manager", out dynamic service))
                throw new InvalidOperationException("No se logró tener acceso al servicio Cctv_Manager");

            _cctvManager = service as CctvModule;

            ShowHistoryDialogCommand = new Command(p => _cctvManager.InvokeHistoryIncidence());

            ReassignTechnicianCommand = new Command(p =>
            {
                if (OpenedIncidences.Where(i => i.Status == IncidenceStatus.OPEN).Count() == 0) return;

                Task.Run(() =>
                {
                    foreach (var incidence in OpenedIncidences.Where(i => i.Status == IncidenceStatus.OPEN))
                        if (incidence.RequireReassign())
                            if (incidence.AssignStaff())
                                AcabusDataContext.DbContext.Update(incidence);
                    ShowMessage("Reasignación completada.");
                });
            });

            CopyingClipboardCommand = new Command(p => (p as Incidence).ToClipboard());

            UpdateSelectionCommand = new Command(p =>
            {
                if (p is null || !(p is IList))
                    return;

                SelectedIncidences.Clear();

                var selection = p as IList;

                foreach (Incidence item in selection)
                    SelectedIncidences.Add(item);

                SelectedIncidences.ToClipboard();
            });

            ShowCloseIncidenceDialogCommand = new Command(p => _cctvManager?.InvokeCloseIncidence(SelectedIncidences, ApplyChanges), CanInvokeCloseDialog);

            ShowAddIncidenceDialogCommand = new Command(p => _cctvManager?.InvokeAddIncidenceDialog(ApplyChanges));

            ShowRefundCashDialogCommand = new Command(p => _cctvManager?.InvokeRefundCashDialog(ApplyChanges));

            ShowModifyIncidenceDialogCommand = new Command(p => _cctvManager?.InvokeModifyDialog(SelectedIncidences.FirstOrDefault(), ApplyChanges), CanInvokeModifyDialog);

            ShowExportDialogCommand = new Command(p => _cctvManager?.InvokeExportDialog());

            ShowOffDutyVehiclesDialogCommand = new Command(p => _cctvManager?.InvokeOffDutyBusDialog());

            RefreshIncidencesCommand = new Command(p =>
            {
                Task.Run(() =>
                {
                    Trace.WriteLine("Actualizando lista de incidencias", "DEBUG");

                    SendNotify("ACTUALIZACIÓN DE LISTA DE INCIDENCIAS");

                    if (_updatingIncidences) return;

                    _updatingIncidences = true;

                    ReloadData();
                    ApplyChanges();

                    _updatingIncidences = false;

                    SendNotify("LISTA DE INCIDENCIAS ACTUALIZADA");
                });
            });

            ApplyChanges();
        }

        /// <summary>
        /// Obtiene una lista de la incidencias cerradas.
        /// </summary>
        public ObservableCollection<Incidence> ClosedIncidences
            => new ObservableCollection<Incidence>(Incidences?.Where(i =>
                {
                    Boolean isClosed = i.Status == IncidenceStatus.CLOSE;
                    Boolean isMatch = String.IsNullOrEmpty(KeywordToSearchIncidence)
                        || (i.Technician != null && i.Technician.Name.ToUpper().Contains(KeywordToSearchIncidence.ToUpper()))
                        || i.Activity.ToString().ToUpper().Contains(KeywordToSearchIncidence.ToUpper());

                    return isClosed && isMatch;
                }).OrderByDescending(i => i.FinishDate));

        /// <summary>
        /// Comando que invoca el copiado a portapapeles.
        /// </summary>
        public ICommand CopyingClipboardCommand { get; }

        /// <summary>
        /// Obtiene una lista de las incidencias actualmente abiertas.
        /// </summary>
        public ObservableCollection<Incidence> Incidences
            => _cctvManager?.Incidences;

        /// <summary>
        /// Obtiene o establece el criterio de busqueda la incidencia cerrada.
        /// </summary>
        public String KeywordToSearchIncidence {
            get => _keywordToSearchIncidence;
            set {
                _keywordToSearchIncidence = value;
                OnPropertyChanged(nameof(KeywordToSearchIncidence));
                OnPropertyChanged(nameof(ClosedIncidences));
            }
        }

        /// <summary>
        /// Obtiene o establece el folio a buscar en las incidencias abiertas.
        /// </summary>
        public String OpenedFolioToSearch {
            get => _openedFolioToSearch;
            set {
                _openedFolioToSearch = value;
                OnPropertyChanged(nameof(OpenedFolioToSearch));
                OnPropertyChanged(nameof(OpenedIncidences));
            }
        }

        /// <summary>
        /// Obtiene una lista de las incidencias abiertas, por confirmar o pendientes, ordenadas del
        /// más alto en prioridad de atención a falla, del más alto en prioridad de la incidencia
        /// (tiempo sin atender o definido previamente) y el primero en crearse.
        /// </summary>
        public ObservableCollection<Incidence> OpenedIncidences
            => new ObservableCollection<Incidence>(Incidences.Where(i =>
                {
                    Boolean isOpen = i.Status != IncidenceStatus.CLOSE;
                    Boolean isMatch = String.IsNullOrEmpty(OpenedFolioToSearch)
                        || String.Format("F-{0:D5", i.Folio).ToUpper().Contains(OpenedFolioToSearch.ToUpper());

                    return isOpen && isMatch;
                }).OrderByDescending(i => i.Activity.Priority)
                    .ThenByDescending(i => i.Priority)
                    .ThenBy(i => i.StartDate));

        /// <summary>
        /// Comando que invoca la reasignación de los técnicos a las indencias abiertas.
        /// </summary>
        public ICommand ReassignTechnicianCommand { get; }

        /// <summary>
        /// Comando que invoca la actualización de las incidencias leidas.
        /// </summary>
        public ICommand RefreshIncidencesCommand { get; }

        /// <summary>
        /// Obtiene una lista de las incidencias seleccionadas actualmente en la tabla de Incidencias abiertas.
        /// </summary>
        public ICollection<Incidence> SelectedIncidences
            => _selectedIncidences ?? (_selectedIncidences = new ObservableCollection<Incidence>());

        /// <summary>
        /// Comando que invoca el cuadro de dialogo para agregar incidencias.
        /// </summary>
        public ICommand ShowAddIncidenceDialogCommand { get; }

        /// <summary>
        /// Comando que invoca el cuadro de dialogo para cerrar incidencias.
        /// </summary>
        public ICommand ShowCloseIncidenceDialogCommand { get; }

        /// <summary>
        /// Comando que invoca el cuadro de dialogo para exportar los datos en formato CSV.
        /// </summary>
        public ICommand ShowExportDialogCommand { get; }

        /// <summary>
        /// Comando que invoca el cuadro de dialogo para buscar en el historial las incidencias.
        /// </summary>
        public ICommand ShowHistoryDialogCommand { get; }

        /// <summary>
        /// Comando que invoca el cuadro de dialogo que permite modificar los atributos de la incidencia.
        /// </summary>
        public ICommand ShowModifyIncidenceDialogCommand { get; }

        /// <summary>
        /// Comando que invoca el cuadro de dialogo que gestiona las unidades fuera de circulación.
        /// </summary>
        public ICommand ShowOffDutyVehiclesDialogCommand { get; }

        /// <summary>
        /// Comando que invoca el cuadro de dialogo que permite la devolución de dinero.
        /// </summary>
        public ICommand ShowRefundCashDialogCommand { get; }

        /// <summary>
        /// Comando que es invocado cuando ocurre una actualización en el número de elementos
        /// seleccionados en la tabla de incidencias abiertas.
        /// </summary>
        public ICommand UpdateSelectionCommand { get; }

        /// <summary>
        /// Hace visible los cambios en la vista.
        /// </summary>
        public void ApplyChanges()
        {
            OnPropertyChanged(nameof(OpenedIncidences));
            OnPropertyChanged(nameof(ClosedIncidences));
        }

        /// <summary>
        /// Invoca las funciones del módulo que permite actualizar la lista de incidencias.
        /// </summary>
        public void ReloadData()
            => _cctvManager?.RefreshData();

        /// <summary>
        /// Determina si es posible invocar el cuadro de dialogo para cerrar incidencias.
        /// </summary>
        /// <param name="arg">Argumento del comando.</param>
        /// <returns>Un valor true si es posible invocar.</returns>
        private bool CanInvokeCloseDialog(object arg)
            => SelectedIncidences?.Count > 0;

        /// <summary>
        /// Determina si es posible invocar el cuadro de dialogo para modificar incidencias.
        /// </summary>
        /// <param name="arg">Argumento del comando.</param>
        /// <returns>Un valor true si es posible invocar.</returns>
        private bool CanInvokeModifyDialog(object arg)
            => SelectedIncidences.Count == 1;
    }
}