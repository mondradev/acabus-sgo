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
            Trunk.LoadConfiguration();
            RuntimeHelpers.RunClassConstructor(typeof(EventViewer).TypeHandle);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
