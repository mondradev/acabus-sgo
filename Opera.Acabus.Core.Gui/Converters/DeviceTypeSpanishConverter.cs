using InnSyTech.Standard.Mvvm.Converters;
using Opera.Acabus.Core.Models;
using System;
using System.Windows.Data;

namespace Opera.Acabus.Core.Gui.Converters
{
    /// <summary>
    /// Esta clase define un convertidor de valores para la enumeración <see cref="DeviceType"/>.
    /// </summary>
    [ValueConversion(typeof(DeviceType), typeof(String))]
    public sealed class DeviceTypeSpanishConverter : TranslateEnumConverter<DeviceType>
    {
        /// <summary>
        /// Crea una instancia nueva de <see cref="DeviceTypeSpanishConverter"/>.
        /// </summary>
        public DeviceTypeSpanishConverter() : base(new DeviceTypeSpanishTranslate()) { }
    }
}