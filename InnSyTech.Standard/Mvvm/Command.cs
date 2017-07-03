using System;
using System.Windows.Input;

namespace InnSyTech.Standard.Mvvm
{
    /// <summary>
    /// Esta clase permite la creación de un comando.
    /// </summary>
    public class Command : ICommand
    {
        /// <summary>
        /// Función que determina si es posible ejecutar la acción.
        /// </summary>
        private Func<object, bool> canExecFunc;

        /// <summary>
        /// Acción a ejecutar en caso de ser posible.
        /// </summary>
        private Action<object> execAction;

        /// <summary>
        /// Crea una instancia de <see cref="Command"/>, que no requiere validar si se puede ejecutar.
        /// </summary>
        /// <param name="execAction">Acción a ejecutar del comando.</param>
        public Command(Action<object> execAction)
        {
            this.execAction = execAction;
            this.canExecFunc = null;
        }

        /// <summary>
        /// Crea una instancia de <see cref="Command"/>.
        /// </summary>
        /// <param name="execAction">Acción a ejecutar por el comando.</param>
        /// <param name="canExecFunc">Función que determina si es posible ejecutar la acción.</param>
        public Command(Action<object> execAction, Func<object, bool> canExecFunc)
        {
            this.execAction = execAction;
            this.canExecFunc = canExecFunc;
        }

        /// <summary>
        /// Evento que surge cuando la posibilidad de ejecución de la acción cambia.
        /// </summary>
        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Determina si la acción puede ser ejecutada.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        /// <returns><see cref="true"/> si es posible ejecutar la acción.</returns>
        public bool CanExecute(object parameter)
        {
            if (canExecFunc != null)
                return canExecFunc.Invoke(parameter);
            else
                return true;
        }

        /// <summary>
        /// Ejecuta la acción del comando.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        public void Execute(object parameter)
        {
            if (execAction != null)
                execAction.Invoke(parameter);
        }
    }
}