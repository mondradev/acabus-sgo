using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Opera.Acabus.Core.Gui.Converters
{
    /// <summary>
    /// Representa un convertidor que obtiene el valor booleano de la enumeración <see
    /// cref="Visibility" />.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BooleanToVisibilityConverter : IValueConverter, IMultiValueConverter
    {
        /// <summary>
        /// Es un parametro del convertir que indica que el compartamiento tratado con el operador NAND.
        /// </summary>
        public const int Nand = 8;

        /// <summary>
        /// Es un parametro del convertir que indica que el compartamiento tratado con el operador NOR.
        /// </summary>
        public const int Nor = 2;

        /// <summary>
        /// Es un parametro del convertir que indica que el compartamiento es inverso.
        /// </summary>
        public const int Not = 1;

        /// <summary>
        /// Es un parametro del convertir que indica que el compartamiento tratado con el operador OR.
        /// </summary>
        public const int Or = 4;

        /// <summary>
        /// Convierte el valor <see cref="bool" /> a un valor de <see cref="Visibility" />. Si se
        /// desea que los valores actuen al inverso, se requiere especificar como parametro del
        /// convertidor un valor <see cref="Not" />.
        /// </summary>
        /// <param name="value"> Un valor booleano. </param>
        /// <param name="targetType"> Tipo de dato del objetivo. </param>
        /// <param name="parameter"> Parametros del convertidor. </param>
        /// <param name="culture"> Referencia cultural utilizada para la conversión. </param>
        /// <returns>
        /// Un valor true si es <see cref="Visibility.Visible" /> o al inverso si el parametro del
        /// convertidor es <see cref="Not" />.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(bool)) return Visibility.Collapsed;

            var not = parameter is int ? (int)parameter : 0;
            var boolValue = (bool)value;

            if (GetBoolValue(boolValue, not)) return Visibility.Visible;

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Convierte un vector <see cref="bool" /> a un valor de <see cref="Visibility" />. Cuando
        /// se requiera que se valide una u otra opción, utilice el valor <see cref="Or" /> como
        /// parametro. Si se desea que los valores actuen al inverso, se requiere especificar como
        /// parametro del convertidor un valor <see cref="Nand" /> o <see cref="Nor" />. En caso que
        /// se desee que se valide de una forma unica cada elemento del vector, se puede utilizar un
        /// vector como parametro incluyendo en la misma posición el valor <see cref="Or" />, <see
        /// cref="Nor" />, <see cref="Nand" />.
        /// </summary>
        /// <param name="value"> Un vector booleano. </param>
        /// <param name="targetType"> Tipo de dato del objetivo. </param>
        /// <param name="parameter"> Parametros del convertidor. </param>
        /// <param name="culture"> Referencia cultural utilizada para la conversión. </param>
        /// <returns>
        /// Un valor true si es <see cref="Visibility.Visible" /> o al inverso si el parametro del
        /// convertidor es <see cref="Nand" /> o <see cref="Nor" />.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var intParameters = null as IEnumerable<int>;
            var intParameter = 0;
            var booleanValues = values.Cast<bool>();

            if (booleanValues == null || booleanValues.Count() == 0) return Visibility.Collapsed;

            if (parameter is IEnumerable<int>)
                intParameters = (IEnumerable<int>)parameter;

            if (parameter is int)
                intParameter = (int)parameter;

            int current = 0;
            bool boolResult = GetBoolValue(booleanValues.First(), intParameters != null ? intParameters.First() : intParameter);

            foreach (var boolValue in booleanValues.Skip(1))
            {
                var logicOp = intParameters != null ? intParameters.ElementAt(current) : intParameter;

                switch (logicOp)
                {
                    case Not:
                    case Nand:
                        boolResult &= GetBoolValue(boolValue, logicOp);
                        break;

                    case Or:
                    case Nor:
                        boolResult |= GetBoolValue(boolValue, logicOp);
                        break;
                }
            }

            return boolResult ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// El convertidor solo es unidireccional desde origen ( <see cref="BindingMode.OneWay" />.
        /// Función no implementada.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// El convertidor solo es unidireccional desde origen ( <see cref="BindingMode.OneWay" />.
        /// Función no implementada.
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Obtiene el valor booleano según la combinación de sus parametros. Por ejemplo para
        /// obtener un valor false, basta con la combinación de true y <see cref="Not" />.
        /// </summary>
        /// <param name="boolValue"> Valor booleano. </param>
        /// <param name="logicOp"> Operador lógico. </param>
        /// <returns> Un valor true según la combinación de los parametros. </returns>
        private bool GetBoolValue(bool boolValue, int logicOp)
        {
            if (logicOp == Not || logicOp == Nand || logicOp == Nor)
                return !boolValue;

            return boolValue;
        }
    }
}