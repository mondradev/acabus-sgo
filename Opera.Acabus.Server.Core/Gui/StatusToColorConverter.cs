using Opera.Acabus.Core.Gui.Converters;
using Opera.Acabus.Server.Core.Models;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media;

namespace Opera.Acabus.Server.Core.Gui
{
    /// <summary>
    /// Representa un convertir de la enumeración <see cref="ServiceStatus"/> a colores según el valor.
    /// </summary>
    [ValueConversion(typeof(ServiceStatus), typeof(Brush))]
    public sealed class StatusToColorConverter : EnumToColorConverter<ServiceStatus>
    {
        /// <summary>
        /// Crea una instancia nueva del convertidor.
        /// </summary>
        public StatusToColorConverter() : base(new Dictionary<ServiceStatus, Brush>()
        {
            { ServiceStatus.OFF, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9E9E9E")) },
            {  ServiceStatus.ON, new SolidColorBrush((Color) ColorConverter.ConvertFromString("#4CAF50")) },
            {  ServiceStatus.ERROR, new SolidColorBrush((Color) ColorConverter.ConvertFromString("#F44336")) },
            {  ServiceStatus.WARN, new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FFEB3B")) }
        })
        {
        }
    }
}