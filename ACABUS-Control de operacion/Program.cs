using ACABUS_Control_de_operacion.Acabus;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AcabusData.LoadConfiguration();
            RuntimeHelpers.RunClassConstructor(typeof(EventViewer).TypeHandle);
            RuntimeHelpers.RunClassConstructor(typeof(DBKVRsExternos).TypeHandle);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
