using System.ServiceProcess;

namespace Opera.Acabus.Server
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ServerService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
