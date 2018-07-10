using InnSyTech.Standard.Gui;
using Opera.Acabus.Core.Gui.Modules;
using System;
using System.Windows;

namespace Opera.Acabus.Server.Core
{
    public sealed class ServerCoreModule : ModuleInfoGui
    {
        public override string Author => "Javier de J. Flores Mondragón";

        public override string CodeName => "Server_Core";

        public override FrameworkElement Icon => GuiHelper.CreateIcon("M13,18H14A1,1 0 0,1 15,19H22V21H15A1,1 0 0,1 14,22H10A1,1 0 0,1 9,21H2V19H9A1,1 0 0,1 10,18H11V16H4A1,1 0 0,1 3,15V11A1,1 0 0,1 4,10H20A1,1 0 0,1 21,11V15A1,1 0 0,1 20,16H13V18M4,2H20A1,1 0 0,1 21,3V7A1,1 0 0,1 20,8H4A1,1 0 0,1 3,7V3A1,1 0 0,1 4,2M9,6H10V4H9V6M9,14H10V12H9V14M5,4V6H7V4H5M5,12V14H7V12H5Z");

        public override ModuleType ModuleType => ModuleType.VIEWER;

        public override string Name => "Monitor de servicios";

        public override Side Side => Side.LEFT;

        public override Type ViewType => typeof(Views.ServerCoreView);
        
        public override bool LoadModule()
        {
            ServerController.Start();
            return true;
        }
    }
}
