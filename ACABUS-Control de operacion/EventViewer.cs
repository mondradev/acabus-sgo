using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class EventViewer : Form
    {
        public static String FileNameErrorLogger = String.Format("error_{0}.log", DateTime.Now.ToShortDateString());
        public static String FileNameInfoLogger = String.Format("info_{0}.log", DateTime.Now.ToShortDateString());
        public static String FileNameDebugLogger = String.Format("debug_{0}.log", DateTime.Now.ToShortDateString());

        internal EventViewer()
        {
            InitializeComponent();
        }

        public enum EventType
        {
            ERROR, INFO, DEBUG
        }

        internal class LoggerRow
        {
            public DateTime DateTimeEvent { get; set; }
            public String ModuleName { get; set; }
            public String MessageEvent { get; set; }
            public EventType EventType { get; set; }

            private LoggerRow()
            { }

            public static LoggerRow Create(DateTime dateTime, String moduleName, String message)
            {
                return new LoggerRow()
                {
                    DateTimeEvent = dateTime,
                    ModuleName = moduleName,
                    MessageEvent = message
                };
            }
        }
    }
}
