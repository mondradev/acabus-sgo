using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opera.Acabus.Server.Core.ViewModels
{
    public sealed class ServerCoreViewModel : ViewModelBase
    {
        private Priority _serviceStatus;

        public Priority ServiceStatus {
            get => _serviceStatus;
            private set {
                _serviceStatus = value;
                OnPropertyChanged(nameof(ServiceStatus));
            }
        }

        public String ServiceName { get; }

        public ServerCoreViewModel()
        {
            ServiceStatus = Priority.NONE;
        }
    }
}
