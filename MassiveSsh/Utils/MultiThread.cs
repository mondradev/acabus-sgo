using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Acabus.Utils
{
    public class MultiThread
    {
        public enum ActionThread
        {
            ADDING, REMOVING
        }

        private class ThreadInfo
        {
            public Thread Thread;

            public System.Threading.ThreadState State;

            public void StopThread(Action<ThreadInfo> callback)
            {
                var self = this;
                new Task(() =>
                {
                    self.State = System.Threading.ThreadState.StopRequested;
                    Trace.WriteLine(String.Format("Intentando matar subproceso: {0}", self.Thread.Name), "DEBUG");
                    while (self.Thread.IsAlive)
                    {
                        if (self.Thread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                            self.Thread.Interrupt();
                        self.Thread.Abort();
                    }
                    callback?.Invoke(self);
                }).Start();
            }
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

        private List<ThreadInfo> _threads;

        public int Capacity { get; set; }

        public int Count {
            get {
                return _threads.FindAll((threadInfo) => threadInfo.State == System.Threading.ThreadState.Running).Count;
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
            this._threads = new List<ThreadInfo>()
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
                ThreadInfo threadInfo = new ThreadInfo()
                {
                    State = System.Threading.ThreadState.Unstarted,
                    Thread = Thread.CurrentThread
                };
                threadInfo.Thread.Name = taskName;
                try
                {
                    lock (_threads)
                    {
                        _threads.Add(threadInfo);
                        if (_threads.FindAll((threadinfo) => threadInfo.State == System.Threading.ThreadState.Running).Count >= Capacity)
                        {
                            Monitor.Wait(_threads);
                        }
                    }
                    if (threadInfo.State == System.Threading.ThreadState.StopRequested)
                    {
                        Trace.WriteLine(String.Format("Proceso abortado: {0}", threadInfo.Thread.Name), "DEBUG");
                        return;
                    }
                    threadInfo.State = System.Threading.ThreadState.Running;

                    Trace.WriteLine(String.Format("Subproceso creado: {0}", threadInfo.Thread.Name), "DEBUG");
                    OnChanged(new MultiThreadEventArgs(ActionThread.ADDING));
                    toDo.Invoke();
                    RemoveProcess(threadInfo);
                }

                catch (Exception ex)
                {
                    RemoveProcess(threadInfo);
                    if (onError != null)
                        onError.Invoke(ex);
                }
            });
            task.Start();
        }

        private void RemoveProcess(ThreadInfo threadInfo)
        {
            if (threadInfo == null) return;
            threadInfo.StopThread((param) =>
            {
                _threads.Remove(param);
                GC.SuppressFinalize(param);
                Trace.WriteLine(String.Format("Subproceso removido: {0}", param.Thread.Name), "DEBUG");
                lock (_threads)
                {
                    Monitor.Pulse(_threads);
                }
                OnChanged(new MultiThreadEventArgs(ActionThread.REMOVING));
            });
        }

        public void KillAllThreads(Action action = null)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.Name = "Killing Process";
                _inStoping = true;
                while (IsRunning())
                {
                    lock (_threads)
                        Monitor.PulseAll(_threads);
                    _threads.FindAll((threadInfo) => threadInfo.State == System.Threading.ThreadState.Running)
                        .ForEach((threadInfo) => threadInfo.StopThread((param) =>
                        {
                            RemoveProcess(param);
                            OnChanged(new MultiThreadEventArgs(ActionThread.REMOVING));
                        }));
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

        public Boolean HasThreadUnStarted()
        {
            return this._threads.FindAll((threafInfo) => threafInfo.State == System.Threading.ThreadState.Unstarted).Count > 0;
        }
    }
}
