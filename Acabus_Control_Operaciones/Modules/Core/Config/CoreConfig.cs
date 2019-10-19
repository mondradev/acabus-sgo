using Acabus.Modules.Configurations;
using Acabus.Modules.Core.Config.ViewModels;
using Acabus.Modules.Core.Config.Views;
using Acabus.Modules.Core.DataAccess;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Acabus.Modules.Core.Config
{
    public class CoreConfig : IConfigurable
    {
        private List<Tuple<string, ICommand>> _commands;

        private List<Tuple<string, Func<object>>> _previewData;

        private CoreConfigView _view;
        private CoreConfigViewModel _viewModel;

        public CoreConfig()
        {
            _previewData = new List<Tuple<string, Func<Object>>>() {
                new Tuple<string, Func<Object>>("Equipos", () => AcabusData.AllDevices.Count()),
                new Tuple<string, Func<Object>>("Estaciones", () => AcabusData.AllStations.Count()),
                new Tuple<string, Func<Object>>("Vehículos", () => AcabusData.AllVehicles.Count()),
                new Tuple<string, Func<Object>>("Rutas", () => AcabusData.AllRoutes.Count())
            };

            _commands = new List<Tuple<string, ICommand>>()
            {
                new Tuple<string, ICommand>("DETALLES", new CommandBase(parameter => AcabusControlCenterViewModel.ShowContent(_view))),
            };

            _view = new CoreConfigView();
            _viewModel = _view.DataContext as CoreConfigViewModel;
        }

        public List<Tuple<string, ICommand>> Commands => _commands;

        public List<Tuple<string, Func<object>>> PreviewData => _previewData;

        public string Title => "EQUIPOS, ESTACIONES, VEHÍCULOS Y RUTAS";
    }
}