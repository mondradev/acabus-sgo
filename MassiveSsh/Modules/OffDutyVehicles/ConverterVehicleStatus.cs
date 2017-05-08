using Acabus.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Acabus.Modules.OffDutyVehicles
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ConverterVehicleStatus : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VehicleStatus)           
                return TranslateValue((VehicleStatus)value);            
            if (value is IEnumerable<VehicleStatus>)            
                return ((IEnumerable<VehicleStatus>)value).Select((item) => TranslateValue(item));
            
            return String.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private String TranslateValue(VehicleStatus value)
        {
            switch (value)
            {
                case VehicleStatus.IN_REPAIR:
                    return "EN TALLER";
                case VehicleStatus.WITHOUT_ENERGY:
                    return "SIN ENERGÍA";
                case VehicleStatus.OPERATING:
                    return "EN OPERACIÓN";
            }
            return String.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString())
            {
                case "EN TALLER": return VehicleStatus.IN_REPAIR;
                case "SIN ENERGÍA": return VehicleStatus.WITHOUT_ENERGY;
                case "EN OPERACIÓN":
                default:
                    return VehicleStatus.OPERATING;
            }
        }
    }
}
