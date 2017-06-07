using Acabus.DataAccess;
using Acabus.Modules.CctvReports.Models;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using System;
using System.Linq;
using System.Windows.Input;

namespace Acabus.Modules.CctvReports
{
    public sealed class ExportDataViewModel : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'StartDateTime'.
        /// </summary>
        private DateTime _startDateTime;

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

        /// <summary>
        /// Campo que provee a la propiedad 'FinishDateTime'.
        /// </summary>
        private DateTime _finishDateTime;

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

        private String FileName => String.Format("acabus_{{0}}_{0:yyyyMMdd}_{1:yyyyMMdd}.csv", StartDateTime, FinishDateTime);

        public ICommand GenerateExportCommand { get; }

        public ExportDataViewModel()
        {
            _startDateTime = DateTime.Now;
            _finishDateTime = DateTime.Now;

            GenerateExportCommand = new CommandBase(Export);
        }

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedReport'.
        /// </summary>
        private ReportQuery _selectedReport;

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

        private void Export(object parameter)
        {
            //if (SelectedReport is null) return;

            //String query = String.Format(SelectedReport.Query,
            //                         StartDateTime, FinishDateTime);

            //var response = SQLiteAccess.ExecuteQuery(query, out String[] header);

            //if (response.Length <= 1 || response[0].Length < 1)
            //{
            //    AcabusControlCenterViewModel.ShowDialog("El periodo no obtuvo ningún resultado.");
            //    return;
            //}

            //Csv.Export(response.Select((item)
            //    => item.Select((subitem)
            //        => subitem.ToString()).ToArray()).ToArray(),
            //    header,
            //    String.Format(FileName, SelectedReport.Description));

            //AcabusControlCenterViewModel.ShowDialog("Información fue exportada correctamente.");
        }
    }
}
