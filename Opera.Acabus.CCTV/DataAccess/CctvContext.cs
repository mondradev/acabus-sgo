using InnSyTech.Standard.Database;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Cctv.SubModules.AddIncidence.Views;
using Opera.Acabus.Cctv.SubModules.CloseIncidences.ViewModels;
using Opera.Acabus.Cctv.SubModules.CloseIncidences.Views;
using Opera.Acabus.Cctv.SubModules.ExportData.Views;
using Opera.Acabus.Cctv.SubModules.IncidenceHistorial.Views;
using Opera.Acabus.Cctv.SubModules.ModifyIncidence.ViewModels;
using Opera.Acabus.Cctv.SubModules.ModifyIncidence.Views;
using Opera.Acabus.Cctv.SubModules.OffDutyBus.Views;
using Opera.Acabus.Cctv.SubModules.RefundOfMoney.Views;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Opera.Acabus.Cctv.DataAccess
{
    /// <summary>
    /// Provee el acceso a los datos manipulados por el módulo de Cctv. En esta clase se podrá
    /// obtener la información de las incidencias, actividades, catergorias, devoluciones, destino de
    /// dinero, etc.
    /// </summary>
    public static class CctvContext
    {
        /// <summary>
        /// Contexto de acceso a la base de datos.
        /// </summary>
        private static readonly IDbSession _context
            = AcabusDataContext.DbContext;

        /// <summary>
        /// Evento que ocurre cuando existe una modificación en la información que administra esta clase.
        /// </summary>
        public static event EventHandler Refresh;

        /// <summary>
        /// Obtiene una lista de las actividades almacenada en la base de datos.
        /// </summary>
        public static IQueryable<Activity> Activities
            => _context.Read<Activity>();

        /// <summary>
        /// Obtiene una lista de los destinos de dinero almacenados en la base de datos.
        /// </summary>
        public static IQueryable<CashDestiny> CashDestinies
            => _context.Read<CashDestiny>();

        /// <summary>
        /// Obtiene una lista de las categorías de actividades almacenadas en la base de datos.
        /// </summary>
        public static IQueryable<ActivityCategory> Categories
            => _context.Read<ActivityCategory>();

        /// <summary>
        /// Obtiene una lista de las incidencias almacenas en la base de datos.
        /// </summary>
        public static IQueryable<Incidence> Incidences
            => _context.Read<Incidence>();

        /// <summary>
        /// Obtiene una lista de las prioridades válidas para una incidencia.
        /// </summary>
        public static IEnumerable<Priority> Priorities
            => Enum.GetValues(typeof(Priority)).Cast<Priority>().Except(new[] { Priority.NONE });

        /// <summary>
        /// Obtiene una lista de todas las devoluciones de dinero.
        /// </summary>
        public static IQueryable<RefundOfMoney> Refunds
            => _context.Read<RefundOfMoney>();

        /// <summary>
        /// Obtiene una lista  de todos los usuarios 
        /// </summary>
        public static IQueryable<AssignableStaff> Staff
                    => _context.Read<AssignableStaff>();

        /// <summary>
        ///
        /// </summary>
        public static IEnumerable<IncidenceStatus> Statuses
            => Enum.GetValues(typeof(IncidenceStatus)).Cast<IncidenceStatus>();

        /// <summary>
        /// Invoca el cuadro de dialogo para la apertura de uno o multiples folios.
        /// </summary>
        /// <param name="callback">
        /// Llamada de vuelte después de realizar la acción con una respuesta positiva.
        /// </param>
        public static void InvokeAddIncidenceDialog(Action callback = null)
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
        public static void InvokeCloseIncidence(IEnumerable<Incidence> selectedIncidences, Action callback = null)
        {
            if (selectedIncidences.Count() > 1)
                Dispatcher.RequestShowDialog(new MultiCloseIncidencesView
                {
                    DataContext = new MultiCloseIncidencesViewModel
                    {
                        SelectedIncidences = new ObservableCollection<Incidence>(selectedIncidences)
                    }
                }, delegate
                {
                    RefreshData();
                    callback?.Invoke();
                });
            else
                Dispatcher.RequestShowDialog(new CloseIncidenceView
                {
                    DataContext = new CloseIncidenceViewModel
                    {
                        SelectedIncidence = selectedIncidences.FirstOrDefault()
                    }
                }, delegate
                {
                    RefreshData();
                    callback?.Invoke();
                });
        }

        /// <summary>
        /// Invoca el cuadro de dialogo que permite exportar la información en formato CSV.
        /// </summary>
        public static void InvokeExportDialog()
            => Dispatcher.RequestShowDialog(new ExportDataView());

        /// <summary>
        /// Invoca el cuadro de dialogo que permite la busqueda de multiples folios en el historial.
        /// </summary>
        public static void InvokeHistoryIncidence()
            => Dispatcher.RequestShowDialog(new IncidencesHistorialView());

        /// <summary>
        /// Invoca el cuadro de dialogo que permite la modificación de los atributos del folio.
        /// </summary>
        /// <param name="selectedIncidence"> Incidencia a modificar. </param>
        /// <param name="callback">
        /// Llamada de vuelte después de realizar la acción con una respuesta positiva.
        /// </param>
        public static void InvokeModifyDialog(Incidence selectedIncidence, Action callback = null)
        {
            var viewModel = new ModifyIncidenceViewModel { SelectedIncidence = selectedIncidence };
            Dispatcher.RequestShowDialog(new ModifyIncidenceView() { DataContext = viewModel },
                delegate { callback?.Invoke(); });
        }

        /// <summary>
        /// Invoca el cuadro de dialogo que permite la gestion de los autobuses en fuera de servicio.
        /// </summary>
        public static void InvokeOffDutyBusDialog()
            => Dispatcher.RequestShowDialog(new OffDutyBusView());

        /// <summary>
        /// Invoca el cuadro de dialog que permite la devolución de dinero.
        /// </summary>
        /// <param name="callback">
        /// Llamada de vuelte después de realizar la acción con una respuesta positiva.
        /// </param>
        public static void InvokeRefundCashDialog(Action callback = null)
            => Dispatcher.RequestShowDialog(new RefundOfMoneyView(), delegate
            {
                RefreshData();
                callback?.Invoke();
            });

        /// <summary>
        /// Actualiza la información leída desde la base de datos.
        /// </summary>
        public static void RefreshData()
            => Refresh?.Invoke(null, new EventArgs());

        /// <summary>
        /// Obtiene un listado del personal que puede ser asignado.
        /// </summary>
        public static List<AssignableStaff> GetStaticAssignableStaff()
        {
            //TODO: Implementar la función que obtiene el personal asignable a la incidencia.
            throw new NotImplementedException();
        }
    }
}