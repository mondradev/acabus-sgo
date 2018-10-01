using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Server.Core.Utils;
using System;

namespace Opera.Acabus.Server.Core.ViewModels
{
    public sealed class ServerCoreViewModel : ViewModelBase
    {
        private ServiceStatus _serviceStatus;

        public ServiceStatus Status {
            get => _serviceStatus;
            private set {
                _serviceStatus = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public String ServiceName { get; } = "SGO Server v0.1";

        public ServerCoreViewModel()
        {
            ServerController.StatusChanged += (sender, status)
                => Status = status;

            Status = ServerController.Running ? ServiceStatus.ON : ServiceStatus.OFF;

        }
    }
}
