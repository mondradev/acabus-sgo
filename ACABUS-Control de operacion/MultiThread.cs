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

        public int Capacity { get; set; }

        public int Count {
            get {
                return _threads.Count;
            }
        }

        public delegate void ChangedEventHandler(object sender, EventArgs e);

        public event ChangedEventHandler ThreadsChanged;

        protected virtual void OnChanged(EventArgs e)
        {
            ThreadsChanged?.Invoke(this, e);
        }

        public MultiThread()
        {
            this.Capacity = MAX_THREADS_DEFAULT;
            this._threads = new List<Thread>()
            {
                Capacity = Capacity
            };
        }

        public void RunTask(String taskName, Action toDo, Action<Exception> onError = null)
        {
            if (_inStoping)
            {
                Trace.WriteLine(String.Format("En proceso de destrucción de subprocesos: {0}", this.Count), "DEBUG");
                return;
            }

            Thread task = new Thread(() =>
            {

                Thread.CurrentThread.Name = taskName;
                try
                {
                    lock (_threads)
                    {
                        if (_threads.Count >= Capacity)
                        {
                            System.Threading.Monitor.Wait(_threads);
                        }
                        if (_inStoping)
                        {
                            Trace.WriteLine(String.Format("Proceso abortado: {0}", Thread.CurrentThread.Name), "DEBUG");
                            return;
                        }
                        _threads.Add(Thread.CurrentThread);
                    }
                    Trace.WriteLine(String.Format("Subproceso creado: {0}", Thread.CurrentThread.Name), "DEBUG");
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
            lock (_threads)
            {
                _threads.Remove(thread);
                Trace.WriteLine(String.Format("Subproceso removido: {0}", thread.Name), "DEBUG");
                System.Threading.Monitor.Pulse(_threads);
                OnChanged(new MultiThreadEventArgs(ActionThread.REMOVING));
            }
        }

        public void KillAllThreads(Action action = null)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.Name = "Killing Process";
                _inStoping = true;
                while (_threads.Count > 0)
                {
                    lock (_threads)
                        System.Threading.Monitor.PulseAll(_threads);

                    for (Int16 i = (Int16)(_threads.Count - 1); i >= 0; i--)
                    {
                        try
                        {
                            Trace.WriteLine(String.Format("Intentando matar subproceso: {0}", _threads[i].Name), "DEBUG");
                            if (!_threads[i].IsAlive)
                                RemoveProcess(_threads[i]);
                            else
                            {
                                _threads[i].Interrupt();
                                _threads[i].Join(600);
                                _threads[i].Abort();
                                _threads[i].Join(600);
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Trace.WriteLine(String.Format("Cambió la cantidad de subprocesos {0}", this.Count), "DEBUG");
                            OnChanged(new MultiThreadEventArgs(ActionThread.REMOVING));
                        }
                    }
                }
                _inStoping = false;
                if (action != null)
                    action.Invoke();
            }).Start();
        }

        public Boolean IsRunning()
        {
            return this.Count > 0;
        }
    }
}
