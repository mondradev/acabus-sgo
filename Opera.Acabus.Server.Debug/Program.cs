using System;
using System.Threading;

namespace Opera.Acabus.Server.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            Core.Services.Server.Initialize();
            while (true)
            {
                Thread.Sleep(10);
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    Core.Services.Server.CloseAllSessions();
                    break;
                }
            }
        }
    }
}
