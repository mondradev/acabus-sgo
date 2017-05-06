using System;
using System.ComponentModel;

namespace Acabus.Utils.MVVM
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(String name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
