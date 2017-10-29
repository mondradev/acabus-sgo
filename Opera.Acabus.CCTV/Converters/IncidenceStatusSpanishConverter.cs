using InnSyTech.Standard.Mvvm.Converters;
using Opera.Acabus.Cctv.Models;
using System;
using System.Windows.Data;

namespace Opera.Acabus.Cctv.Converters
{
    /// <summary>
    /// Convertidor para la traducción de la enumeración <see cref="IncidenceStatus" />.
    /// </summary>
    [ValueConversion(typeof(IncidenceStatus), typeof(String))]
    public sealed class IncidenceStatusSpanishConverter : TranslateEnumConverter<IncidenceStatus>
    {
        /// <summary>
        /// Crea una instancia del traductor de la enumaración <see cref="IncidenceStatus"/>.
        /// </summary>
        public IncidenceStatusSpanishConverter() : base(new IncidenceStatusTranslator()) { }
    }
}