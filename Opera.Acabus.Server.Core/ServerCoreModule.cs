using InnSyTech.Standard.Gui;
using Opera.Acabus.Core.Gui.Modules;
using System;
using System.Windows;

namespace Opera.Acabus.Server.Core
{
    /// <summary>
    /// Define el módulo del nucleo del servidor.
    /// </summary>
    public sealed class ServerCoreModule : ModuleInfoGui
    {
        /// <summary>
        /// Obtiene el nombre del autor del módulo.
        /// </summary>
        public override string Author => "Javier de J. Flores Mondragón";

        /// <summary>
        /// Obtiene el nombre código del módulo.
        /// </summary>
        public override string CodeName => "Server_Core";

        /// <summary>
        /// Obtiene el icono que representa al módulo.
        /// </summary>
        public override FrameworkElement Icon
            => GuiHelper.CreateIcon("M13,18H14A1,1 0 0,1 15,19H22V21H15A1,1 0 0,1 14,22H10A1,1 0 0,1 9,21H2V19H9A1,1 0 0,1 10,18H11V16H4A1,1 0 0,1 3,15V11A1,1 0 0,1 4,10H20A1,1 0 0,1 21,11V15A1,1 0 0,1 20,16H13V18M4,2H20A1,1 0 0,1 21,3V7A1,1 0 0,1 20,8H4A1,1 0 0,1 3,7V3A1,1 0 0,1 4,2M9,6H10V4H9V6M9,14H10V12H9V14M5,4V6H7V4H5M5,12V14H7V12H5Z");

        /// <summary>
        /// Obtiene el tipo de módulo.
        /// </summary>
        public override ModuleType ModuleType => ModuleType.VIEWER;

        /// <summary>
        /// Obtiene el nombre del módulo.
        /// </summary>
        public override string Name => "Monitor de servicios";

        /// <summary>
        /// Obtiene el lado de la barra en la cual aparece el botón que da acceso al módulo.
        /// </summary>
        public override Side Side => Side.LEFT;

        /// <summary>
        /// Obtiene el tipo de la bista principal del módulo.
        /// </summary>
        public override Type ViewType => typeof(Views.ServerCoreView);

        /// <summary>
        /// Permite la carga de los datos utilizados por el módulo <see cref="Server"/>.
        /// </summary>
        /// <returns>Un valor true cuando el módulo ha cargado correctamente.</returns>
        public override bool LoadModule()
        {
            ServerController.Start();
            return true;
        }
    }
}