using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class EventViewer : Form
    {
        public static String FileNameErrorLogger = String.Format("error_{0:yyyyMMdd}.log", DateTime.Now);
        public static String FileNameInfoLogger = String.Format("info_{0:yyyyMMdd}.log", DateTime.Now);
        public static String FileNameDebugLogger = String.Format("debug_{0:yyyyMMdd}.log", DateTime.Now);

        public static EventViewer Instance { get; set; }

        static EventViewer()
        {
            Trace.Listeners.Add(new TraceListenerImp());
            File.AppendAllText(FileNameInfoLogger, "");
            File.AppendAllText(FileNameErrorLogger, "");
            File.AppendAllText(FileNameDebugLogger, "");
        }

        internal EventViewer()
        {
            InitializeComponent();
            Instance = this;
        }

        public enum EventType
        {
            ERROR, INFO, DEBUG
        }

        internal class EventRow
        {
            public DateTime DateTimeEvent { get; set; }
            public String MessageEvent { get; set; }
            public EventType EventType { get; set; }

            private EventRow()
            { }

            public static EventRow Create(DateTime dateTime, EventType type, String message)
            {
                return new EventRow()
                {
                    DateTimeEvent = dateTime,
                    MessageEvent = message,
                    EventType = type
                };
            }

            public override string ToString()
            {
                return String.Format("{0:yyyy/MM/dd H:mm:ss}-{1}-{2}", DateTimeEvent, EventType.GetName(typeof(EventType), EventType), MessageEvent);
            }
        }

        internal class TraceListenerImp : TraceListener
        {
            public override void Write(string message)
            {
                WriteLine(message);
            }

            public override void WriteLine(string message)
            {
                String[] eventInfo = message.Split(new Char[] { ':' }, 2);
                if (eventInfo.Length > 0)
                    switch (eventInfo[0])
                    {
                        case "ERROR":
                            AddEvent(EventType.ERROR, eventInfo[1]);
                            break;
                        case "DEBUG":
                            AddEvent(EventType.DEBUG, eventInfo[1]);
                            break;
                        case "INFO":
                            AddEvent(EventType.INFO, eventInfo[1]);
                            break;
                        default:
                            AddEvent(EventType.INFO, message);
                            break;
                    }
            }
        }

        public static void AddEvent(EventType type, String message)
        {
            EventRow log = EventRow.Create(DateTime.Now, type, message.Trim().Replace("\n", " ").Replace("\r", ""));
            switch (type)
            {
                case EventType.ERROR:
                    File.AppendAllText(FileNameErrorLogger, log.ToString() + "\n");
                    break;
                case EventType.INFO:
                    File.AppendAllText(FileNameInfoLogger, log.ToString() + "\n");
                    break;
                case EventType.DEBUG:
                    File.AppendAllText(FileNameDebugLogger, log.ToString() + "\n");
                    break;
                default:
                    break;
            }
            AddRow(log);
        }

        private static void AddRow(EventRow item)
        {
            if (Instance != null && !Instance.IsDisposed)
                Instance.BeginInvoke(new Action(() =>
                {
                    if (Instance.infoCheck.Checked && item.EventType.Equals(EventType.INFO))
                        Instance.eventTable.Rows.Insert(0, item.ToString().Split(new Char[] { '-' }, 3));
                    if (Instance.errorCheck.Checked && item.EventType.Equals(EventType.ERROR))
                        Instance.eventTable.Rows.Insert(0, item.ToString().Split(new Char[] { '-' }, 3));
                    if (Instance.debugCheck.Checked && item.EventType.Equals(EventType.DEBUG))
                        Instance.eventTable.Rows.Insert(0, item.ToString().Split(new Char[] { '-' }, 3));
                }));
        }

        private static void LoadEventsFromFiles()
        {
            if (Instance == null) return;

            if (!File.Exists(FileNameInfoLogger))
                File.AppendAllText(FileNameInfoLogger, "");
            if (!File.Exists(FileNameErrorLogger))
                File.AppendAllText(FileNameErrorLogger, "");
            if (!File.Exists(FileNameDebugLogger))
                File.AppendAllText(FileNameDebugLogger, "");

            String[] info = File.ReadAllLines(FileNameInfoLogger);
            String[] error = File.ReadAllLines(FileNameErrorLogger);
            String[] debug = File.ReadAllLines(FileNameDebugLogger);
            List<String[]> events = new List<String[]>();
            if (Instance.infoCheck.Checked)
                foreach (String item in info)
                    events.Add(item.Split(new Char[] { '-' }, 3));
            if (Instance.errorCheck.Checked)
                foreach (String item in error)
                    events.Add(item.Split(new Char[] { '-' }, 3));
            if (Instance.debugCheck.Checked)
                foreach (String item in debug)
                    events.Add(item.Split(new Char[] { '-' }, 3));
            events.Sort((e1, e2) =>
            {
                return DateTime.Parse(e1[0]).CompareTo(DateTime.Parse(e2[0])) * -1;
            });
            if (!Instance.IsDisposed)
                Instance.BeginInvoke(new Action(() =>
                {
                    Instance.eventTable.Rows.Clear();
                    foreach (var item in events)
                        Instance.eventTable.Rows.Add(item);
                }));
        }

        private void EventViewer_Load(object sender, EventArgs e)
        {
            LoadEventsFromFiles();
        }

        private void InfoCheck_CheckedChanged(object sender, EventArgs e)
        {
            LoadEventsFromFiles();
        }
    }
}
