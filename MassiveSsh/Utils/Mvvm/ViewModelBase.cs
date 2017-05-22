using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Acabus.Utils.Mvvm
{
    /// <summary>
    /// 
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<String, ICollection<String>> _errorsCollection
            = new Dictionary<string, ICollection<string>>();

        /// <summary>
        /// 
        /// </summary>
        public bool HasErrors => !(_errorsCollection.Values.FirstOrDefault(x => x.Count > 0) is null);

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public IEnumerable GetErrors(string propertyName)
        {
            if (!_errorsCollection.ContainsKey(propertyName)
                || _errorsCollection[propertyName].Count < 1)
                return null;

            return _errorsCollection[propertyName];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="error"></param>
        protected void AddError(String propertyName, String error)
        {
            if (!_errorsCollection.ContainsKey(propertyName))
                _errorsCollection.Add(propertyName, new List<String>());

            _errorsCollection[propertyName].Add(error);
            OnErrorChanged(propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ClearErrors(String propertyName)
        {
            if (!String.IsNullOrEmpty(propertyName)
                && _errorsCollection.ContainsKey(propertyName))
            {
                _errorsCollection[propertyName].Clear();
                OnErrorChanged(propertyName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ClearErrors()
        {
            foreach (var propertyName in _errorsCollection.Keys)
                ClearErrors(propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnErrorChanged(String propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            ValidateProperty(propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void ValidateProperty(string propertyName)
        {
            ClearErrors(propertyName);
            OnValidation(propertyName);
            OnErrorChanged(propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnValidation(string propertyName) { }

    }
}
