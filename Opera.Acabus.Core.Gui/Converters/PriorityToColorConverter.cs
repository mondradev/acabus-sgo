using Opera.Acabus.Core.Models;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media;

namespace Opera.Acabus.Core.Gui.Converters
{
    /// <summary>
    /// Representa un convertir de la enumeración <see cref="Priority"/> a colores según el valor.
    /// </summary>
    [ValueConversion(typeof(Priority), typeof(Brush))]
    public sealed class PriorityToColorConverter : EnumToColorConverter<Priority>
    {
        /// <summary>
        /// Crea una instancia nueva del convertidor.
        /// </summary>
        public PriorityToColorConverter() : base(new Dictionary<Priority, Brush>()
        {
            { Priority.LOW, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEB3B")) },
            {  Priority.MEDIUM, new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FF9800")) },
            {  Priority.HIGH, new SolidColorBrush((Color) ColorConverter.ConvertFromString("#F44336")) },
            {  Priority.NONE, new SolidColorBrush((Color) ColorConverter.ConvertFromString("#4CAF50")) }
        })
        {
        }
    }
}