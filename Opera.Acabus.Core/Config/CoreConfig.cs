using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Config.ViewModels;
using Opera.Acabus.Core.Config.Views;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Modules.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Opera.Acabus.Core.Config
{
    /// <summary>
    /// Define la configuración del nucleo del SGO.
    /// </summary>
    public class CoreConfig : IConfigurable
    {
        ///<summary>
        /// Campo que provee a la propiedad <see cref="Commands"/>.
        ///</summary>
        private List<Tuple<String, ICommand>> _commands;

        ///<summary>
        /// Campo que provee a la propiedad <see cref="PreviewData"/>.
        ///</summary>
        private List<Tuple<String, Func<Object>>> _previewData;

        /// <summary>
        /// Vista de la configuración del nucleo de SGO.
        /// </summary>
        private CoreConfigView _view;

        /// <summary>
        /// Modelo de la vista de la configuración del nucleo de SGO.
        /// </summary>
        private CoreConfigViewModel _viewModel;

        /// <summary>
        /// Crea una instancia de <see cref="CoreConfig"/>.
        /// </summary>
        public CoreConfig()
        {
            _previewData = new List<Tuple<string, Func<Object>>>() {
                new Tuple<string, Func<Object>>("Equipos", () => AcabusData.AllDevices.Count()),
                new Tuple<string, Func<Object>>("Estaciones", () => AcabusData.AllStations.Count()),
                new Tuple<string, Func<Object>>("Autobuses", () => AcabusData.AllBuses.Count()),
                new Tuple<string, Func<Object>>("Rutas", () => AcabusData.AllRoutes.Count())
            };

            _commands = new List<Tuple<string, ICommand>>()
            {
                new Tuple<string, ICommand>("DETALLES", new Command(parameter
                            => AcabusData.RequestShowContent(_view)))
            };

            _view = new CoreConfigView();
            _viewModel = _view.DataContext as CoreConfigViewModel;
        }

        /// <summary>
        /// Obtiene la lista de comandos disponibles desde el panel de configuración.
        /// </summary>
        public List<Tuple<string, ICommand>> Commands => _commands;

        /// <summary>
        /// Obtiene la lista de los datos previos visibles desde el panel de configuración.
        /// </summary>
        public List<Tuple<string, Func<object>>> PreviewData => _previewData;

        /// <summary>
        /// Obtiene el título de la sección en el panel de configuración.
        /// </summary>
        public string Title => "EQUIPOS, ESTACIONES, AUTOBUSES Y RUTAS";
    }
}