using InnSyTech.Standard.Net.Communication.Iso8583;
using System.Windows;

namespace Acabus
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var data = new byte[] { 0xAF, 0xFC, 0xEB, 0xDE, 0xFF, 0xFF };

            var field = new Field(20, "3B21DF");

            var dataR = field.Encode(8, FieldType.Alpha, FieldLength.Fixed, FieldFormat.BinaryCodedDecimal, 'F');
        }
    }
}