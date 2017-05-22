using System.Collections.Generic;

namespace Acabus.Utils.Mvvm
{
    public static class ViewModelService
    {
        private static List<ViewModelBase> _viewModelRegisted = new List<ViewModelBase>();

        public static void Register(ViewModelBase instance)
        {
            if (!Registered(instance))
                _viewModelRegisted.Add(instance);
        }

        private static bool Registered(ViewModelBase instance)
        {
            bool exists = false;
            _viewModelRegisted.ForEach(viewModel =>
            {
                if (!exists)
                    exists = viewModel.GetType() == instance.GetType();
            });
            return exists;
        }

        public static void UnRegister(ViewModelBase instance)
        {
            _viewModelRegisted.Remove(instance);
        }

        public static dynamic GetViewModel<T>()
        {
            dynamic instance = null;
            _viewModelRegisted.ForEach(viewModel =>
            {
                if (viewModel.GetType() == typeof(T) && instance is null)
                    instance = viewModel;
            });
            return instance;
        }
    }
}
