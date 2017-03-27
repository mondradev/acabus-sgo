using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ACABUS_Control_de_operacion
{
    public class MultiThread
    {
        public enum ActionThread
        {
            ADDING, REMOVING
        }

        public class MultiThreadEventArgs : EventArgs
        {
            public ActionThread Action { get; private set; }

            public MultiThreadEventArgs(ActionThread action)
            {
                Action = action;
            }
        }

        private const int MAX_THREADS_DEFAULT = 10;

        private Boolean _inStoping = false;

        private List<Thread> _threads;

        public List<Thread> Threads {
            get {
                if (_threads == null) _threads = new List<Thread>();
                return _threads;
            }
            private set {
                _threads = value;
            }
        }

        public int Capacity { get; set; }

        public delegate void ChangedEventHandler(object sender, EventArgs e);

        public event ChangedEventHandler ThreadsChanged;

        protected virtual void OnChanged(EventArgs e)
        {
            ThreadsChanged?.Invoke(this, e);
        }

        public MultiThread()
        {
            Capacity = MAX_THREADS_DEFAULT;
            Threads.Capacity = Capacity;
        }

        public void RunTask(Action toDo, Action<Exception> onError = null)
        {

            if (_inStoping)
            {
                Trace.WriteLine(String.Format("En proceso de destrucción de subprocesos: {0}", Threads.Count), "DEBUG");
                return;
            }

            Thread task = new Thread(() =>
            {
                try
                {
                    if (Threads.Count >= Capacity)
                    {
                        while (Threads.Count + 2 > Capacity)
                        {
                            Thread.Sleep(50);
                        }
                    }
                    if (_inStoping)
                    {
                        Trace.WriteLine(String.Format("En proceso de destrucción de subprocesos: {0}", Threads.Count), "DEBUG");
                        return;
                    }
                    Threads.Add(Thread.CurrentThread);
                    Trace.WriteLine(String.Format("Subproceso creado: {0}", Threads.Count), "DEBUG");
                    OnChanged(new MultiThreadEventArgs(ActionThread.ADDING));
                    toDo.Invoke();
                    RemoveProcess(Thread.CurrentThread);
                }

                catch (Exception ex)
                {
                    RemoveProcess(Thread.CurrentThread);
                    if (onError != null)
                        onError.Invoke(ex);
                }
            });
            task.Start();
        }

        private void RemoveProcess(Thread thread)
        {
            Trace.WriteLine(String.Format("Removiendo subproceso: {0}", Threads.Count), "DEBUG");
            Threads.Remove(thread);
            Trace.WriteLine(String.Format("Subproceso removido: {0}", Threads.Count), "DEBUG");
            OnChanged(new MultiThreadEventArgs(ActionThread.REMOVING));
        }

        public void KillAllThreads(Action action = null)
        {
            new Thread(() =>
            {
                _inStoping = true;
                while (Threads.Count > 0)
                {
                    Trace.WriteLine(String.Format("Intentando matar subproceso: {0}", Threads.Count), "DEBUG");
                    for (Int16 i = (Int16)(Threads.Count - 1); i >= 0; i--)
                    {
                        try
                        {
                            if (!Threads[i].IsAlive)
                                RemoveProcess(Threads[i]);
                            else
                            {
                                Threads[i].Interrupt();
                                Threads[i].Abort();
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Trace.WriteLine(String.Format("Cambió la cantidad de subprocesos {0}", Threads.Count), "DEBUG");
                        }
                    }
                    Thread.Sleep(600);
                }
                _inStoping = false;
                if (action != null)
                    action.Invoke();
            }).Start();
        }

        public Boolean IsRunning()
        {
            return Threads.Count > 0;
        }
    }
}
