using InnSyTech.Standard.Configuration;
using InnSyTech.Standard.Mvvm;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Cctv.SubModules.ExportData.Models;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Gui.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Opera.Acabus.Cctv.SubModules.ExportData.ViewModels
{
    public sealed class ExportDataViewModel : ModuleViewerBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="AllReports" />.
        /// </summary>
        private ICollection<ReportQuery> _allReports;

        /// <summary>
        /// Campo que provee a la propiedad 'FinishDateTime'.
        /// </summary>
        private DateTime _finishDateTime;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedReport'.
        /// </summary>
        private ReportQuery _selectedReport;

        /// <summary>
        /// Campo que provee a la propiedad 'StartDateTime'.
        /// </summary>
        private DateTime _startDateTime;

        public ExportDataViewModel()
        {
            _startDateTime = DateTime.Now;
            _finishDateTime = DateTime.Now;

            var settings = AcabusDataContext.ConfigContext["Cctv"].GetSetting("reports").GetSettings("report");

            if (settings != null && settings.Count > 0)
                _allReports = new ObservableCollection<ReportQuery>(settings.Convert(ConvertToReport));

            GenerateExportCommand = new Command(Export, arg => SelectedReport != null);
        }

        /// <summary>
        /// Obtiene una lista de todos los reportes disponibles.
        /// </summary>
        public ICollection<ReportQuery> AllReports
            => _allReports ?? (_allReports = new ObservableCollection<ReportQuery>());

        /// <summary>
        /// Obtiene o establece la fecha final del exportado.
        /// </summary>
        public DateTime FinishDateTime {
            get => _finishDateTime;
            set {
                _finishDateTime = value;
                OnPropertyChanged("FinishDateTime");
            }
        }

        public ICommand GenerateExportCommand { get; }

        /// <summary>
        /// Obtiene o establece el reporte seleccionado.
        /// </summary>
        public ReportQuery SelectedReport {
            get => _selectedReport;
            set {
                _selectedReport = value;
                OnPropertyChanged("SelectedReport");
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha inicial del exportado.
        /// </summary>
        public DateTime StartDateTime {
            get => _startDateTime;
            set {
                _startDateTime = value;
                OnPropertyChanged("StartDateTime");
            }
        }

        private String FileName => String.Format("reporte_{{0}}_{0:yyyyMMdd}_{1:yyyyMMdd}.csv", StartDateTime, FinishDateTime);

        /// <summary>
        /// Convierte una configuración leida a <see cref="ReportQuery"/>.
        /// </summary>
        public ReportQuery ConvertToReport(ISetting setting)
            => new ReportQuery()
            {
                Description = setting["description"].ToString(),
                Query = setting["query"].ToString()
            };

        private void Export(object parameter)
        {
            if (SelectedReport is null) return;

            String query = String.Format(SelectedReport.Query,
                                     StartDateTime, FinishDateTime);

            var response = AcabusDataContext.DbContext.Batch(query).ToList();

            Task.Run(() =>
            {
                Thread.Sleep(2000);
                try
                {
                    if (response.Count == 0)
                    {
                        ShowMessage("No hay información a exportar\n\nEl periodo no obtuvo ningún resultado.");
                        return;
                    }

                    CsvDump.Export(response, String.Format(FileName, SelectedReport.Description));

                    Application.Current.Dispatcher.Invoke(()
                        => ShowMessage("Información fue exportada correctamente."));
                }
                catch
                {
                    Application.Current.Dispatcher.Invoke(()
                        => ShowMessage("Error al exportar\n\nOcurrió un error en la ejecución de la consulta."));
                }
            });

            Dispatcher.CloseDialog();

            ShowMessage("Exportando datos, te avisaremos cuando haya finalizado.");
        }
    }
}