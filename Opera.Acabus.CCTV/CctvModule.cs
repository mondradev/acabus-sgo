using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Gui;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Cctv.SubModules.AddIncidence.Views;
using Opera.Acabus.Cctv.SubModules.ModifyIncidence.ViewModels;
using Opera.Acabus.Cctv.SubModules.ModifyIncidence.Views;
using Opera.Acabus.Cctv.SubModules.OffDutyBus.Views;
using Opera.Acabus.Cctv.SubModules.RefundOfMoney.Views;
using Opera.Acabus.Cctv.Views;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Gui.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace Opera.Acabus.Cctv
{
    /// <summary>
    /// Representa la información del módulo de <see cref="Cctv" />, el cual provee de funciones para
    /// la apertura y cierre de folios manual, asignación de actividades, devolución de dinero, entre otros.
    /// </summary>
    public sealed class CctvModule : ModuleInfoGui
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Incidences" />.
        /// </summary>
        private ObservableCollection<Incidence> _incidences;

        /// <summary>
        /// Obtiene el autor del módulo.
        /// </summary>
        public override string Author => "Javier de J. Flores Mondragón";

        /// <summary>
        /// Obtiene el nombre código del módulo.
        /// </summary>
        public override string CodeName => "Cctv_Manager";

        /// <summary>
        /// Obtiene el icono con el cual es representado para acceder a la interfaz.
        /// </summary>
        public override FrameworkElement Icon => GuiHelper.CreateIcon("M18.15,4.94C17.77,4.91 17.37,5 17,5.2L8.35,10.2C7.39,10.76 7.07,12 7.62,12.94L9.12,15.53C9.67,16.5 10.89,16.82 11.85,16.27L13.65,15.23C13.92,15.69 14.32,16.06 14.81,16.27V18.04C14.81,19.13 15.7,20 16.81,20H22V18.04H16.81V16.27C17.72,15.87 18.31,14.97 18.31,14C18.31,13.54 18.19,13.11 17.97,12.73L20.5,11.27C21.47,10.71 21.8,9.5 21.24,8.53L19.74,5.94C19.4,5.34 18.79,5 18.15,4.94M6.22,13.17L2,13.87L2.75,15.17L4.75,18.63L5.5,19.93L8.22,16.63L6.22,13.17Z");

        /// <summary>
        /// Obtiene una lista de todas las incidencias abiertas, pendientes y sin confirmar así como
        /// las cerradas hasta hace 5 días.
        /// </summary>
        public ObservableCollection<Incidence> Incidences
            => _incidences ?? (_incidences = new ObservableCollection<Incidence>());

        /// <summary>
        /// Obtiene el tipo de módulo.
        /// </summary>
        public override ModuleType ModuleType => ModuleType.VIEWER;

        /// <summary>
        /// Obtiene el nombre del módulo.
        /// </summary>
        public override string Name => "Gestor de incidencias";

        /// <summary>
        /// Obtiene la posición en la cual se muestra en la barra de menú.
        /// </summary>
        public override Side Side => Side.LEFT;

        /// <summary>
        /// Obtiene el tipo de la vista principal del módulo.
        /// </summary>
        public override Type ViewType => typeof(CctvReportsView);

        /// <summary>
        /// Invoca el cuadro de dialogo para la apertura de uno o multiples folios.
        /// </summary>
        /// <param name="callback">
        /// Llamada de vuelte después de realizar la acción con una respuesta positiva.
        /// </param>
        public void InvokeAddIncidenceDialog(Action callback = null)
            => Dispatcher.RequestShowDialog(new CreateIncidenceView(), delegate
            {
                RefreshData();
                callback?.Invoke();
            });

        /// <summary>
        /// Invoca el cuadro de dialogo del cierre de uno o multiples folios.
        /// </summary>
        /// <param name="selectedIncidences"> Secuencia que contiene los folios a cerrar. </param>
        /// <param name="callback">
        /// Llamada de vuelte después de realizar la acción con una respuesta positiva.
        /// </param>
        public void InvokeCloseIncidence(IEnumerable<Incidence> selectedIncidences, Action callback = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoca el cuadro de dialogo que permite exportar la información en formato CSV.
        /// </summary>
        public void InvokeExportDialog()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoca el cuadro de dialogo que permite la busqueda de multiples folios en el historial.
        /// </summary>
        public void InvokeHistoryIncidence()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoca el cuadro de dialogo que permite la modificación de los atributos del folio.
        /// </summary>
        /// <param name="selectedIncidence"> Incidencia a modificar. </param>
        /// <param name="callback">
        /// Llamada de vuelte después de realizar la acción con una respuesta positiva.
        /// </param>
        public void InvokeModifyDialog(Incidence selectedIncidence, Action callback = null)
        {
            var viewModel = new ModifyIncidenceViewModel { SelectedIncidence = selectedIncidence };
            Dispatcher.RequestShowDialog(new ModifyIncidenceView() { DataContext = viewModel },
                delegate { callback?.Invoke(); });
        }

        /// <summary>
        /// Invoca el cuadro de dialogo que permite la gestion de los autobuses en fuera de servicio.
        /// </summary>
        public void InvokeOffDutyBusDialog()
            => Dispatcher.RequestShowDialog(new OffDutyBusView());

        /// <summary>
        /// Invoca el cuadro de dialog que permite la devolución de dinero.
        /// </summary>
        /// <param name="callback">
        /// Llamada de vuelte después de realizar la acción con una respuesta positiva.
        /// </param>
        public void InvokeRefundCashDialog(Action callback = null)
            => Dispatcher.RequestShowDialog(new RefundOfMoneyView(), delegate
            {
                RefreshData();
                callback?.Invoke();
            });

        /// <summary>
        /// Inicializa el módulo de Cctv_Manager, cargando la lista de incidencias utilizadas en el módulo.
        /// </summary>
        /// <returns> Un valor true si se cargó correctamente. </returns>
        public override bool LoadModule()
        {
            try
            {
                _incidences = new ObservableCollection<Incidence>(AcabusDataContext.DbContext
                    .Read<Incidence>()
                    .Where(i => i.Status != IncidenceStatus.CLOSE
                            || (i.StartDate > DateTime.Now.AddDays(-5)))
                    .LoadReference(3));

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.PrintMessage().JoinLines(), "ERROR");
                return false;
            }
        }

        /// <summary>
        /// Actualiza la información leída desde la base de datos.
        /// </summary>
        public void RefreshData()
        {
            try
            {
                _incidences = new ObservableCollection<Incidence>(AcabusDataContext.DbContext
                     .Read<Incidence>()
                     .Where(i => i.Status != IncidenceStatus.CLOSE
                             || (i.StartDate > DateTime.Now.AddDays(-5)))
                     .LoadReference(3));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.PrintMessage().JoinLines(), "ERROR");
            }
        }
    }
}