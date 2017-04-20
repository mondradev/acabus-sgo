using ACABUS_Control_de_operacion.Acabus;
using ACABUS_Control_de_operacion.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class WeekReport : Form
    {

        private String weekFirstDay = "2017-04-08";

        public WeekReport()
        {
            InitializeComponent();
            FillReport();
        }

        private void FillReport()
        {
            Excel excel = Excel.Open(AcabusData.TEMPLATE_WEEK_REPORT);

            excel.SetWorksheetActive("Información_Troncal-Anexo\"A\"");
            excel.SetValue(weekFirstDay, "B12");

            SshPostgreSQL psql = SshPostgreSQL.CreateConnection(
                AcabusData.PG_PATH_SERVER,
                AcabusData.BD_SERVER_IP,
                AcabusData.PG_PORT,
                AcabusData.PG_USERNAME,
                AcabusData.PG_PASSWORD_SERVER,
                AcabusData.DATABASE_NAME,
                AcabusData.SSH_USERNAME_SERVER,
                AcabusData.SSH_PASSWORD_SERVER
            );
            try
            {
                String[][] sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_SALES)
                                                                                .Replace("{paramFchIni}", weekFirstDay));
                Boolean readHeader = false;
                int i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    excel.SetWorksheetActive("Información_Troncal-Anexo\"A\"");
                    excel.SetValue(row[1], "E" + (12 + i));
                    excel.SetValue(row[2], "F" + (12 + i));
                    excel.SetWorksheetActive("Otros");
                    excel.SetValue(row[3], "Q" + (9 + i));
                    excel.SetValue(row[4], "R" + (9 + i));
                    i++;
                }
                Trace.WriteLine("Ventas de tarjetas ¡Listo!", "INFO");

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILES_ACCESS)
                                                                                .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    excel.SetWorksheetActive("Información_Troncal-Anexo\"A\"");
                    excel.SetValue(row[1], "AE" + (12 + i));
                    excel.SetValue(row[2], "AF" + (12 + i));
                    i++;
                }
                Trace.WriteLine("Accesos con monedero electrónico ¡Listo!", "INFO");

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_RECHARGE)
                                                                                .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    excel.SetWorksheetActive("Información_Troncal-Anexo\"A\"");
                    excel.SetValue(row[1], "AG" + (12 + i));
                    excel.SetValue(row[2], "AH" + (12 + i));
                    i++;
                }
                Trace.WriteLine("Recargas de tarjetas ¡Listo!", "INFO");

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_ACCE_VEHI)
                                                                                .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    excel.SetWorksheetActive("DetalleValidacion");
                    excel.SetValue(row[1], "P" + (9 + i));
                    excel.SetValue(row[2], "Q" + (9 + i));
                    excel.SetValue(row[3], "R" + (9 + i));
                    excel.SetValue(row[4], "S" + (9 + i));
                    excel.SetValue(row[5], "T" + (9 + i));
                    i++;
                }
                Trace.WriteLine("Detalle Validación - Vehículos ¡Listo!", "INFO");

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_ACCE_TRUNK)
                                                                               .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    excel.SetWorksheetActive("DetalleValidacion");
                    excel.SetValue(row[1], "H" + (9 + i));
                    excel.SetValue(row[2], "I" + (9 + i));
                    excel.SetValue(row[3], "J" + (9 + i));
                    excel.SetValue(row[4], "K" + (9 + i));
                    excel.SetValue(row[5], "L" + (9 + i));
                    i++;
                }
                Trace.WriteLine("Detalle Validación - Estaciones ¡Listo!", "INFO");

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_ACC_CRED_VEHI)
                                                                               .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    excel.SetWorksheetActive("Otros");
                    excel.SetValue(row[1], "C" + (9 + i));
                    excel.SetValue(row[2], "D" + (9 + i));
                    i++;
                }
                Trace.WriteLine("Otros - Viajes parciales ¡Listo!", "INFO");

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_ACC_CRED_VEH_COST)
                                                                               .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    excel.SetWorksheetActive("Otros");
                    excel.SetValue(row[1], "E" + (9 + i));
                    excel.SetValue(row[2], "F" + (9 + i));
                    excel.SetValue(row[3], "G" + (9 + i));
                    excel.SetValue(row[4], "H" + (9 + i));
                    excel.SetValue(row[5], "I" + (9 + i));
                    excel.SetValue(row[6], "J" + (9 + i));
                    excel.SetValue(row[7], "K" + (9 + i));
                    excel.SetValue(row[8], "L" + (9 + i));
                    excel.SetValue(row[9], "M" + (9 + i));
                    excel.SetValue(row[10], "N" + (9 + i));
                    i++;
                }
                Trace.WriteLine("Otros - Detalles viajes parciales ¡Listo!", "INFO");

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_RECHARGE_CRED)
                                                                               .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    excel.SetWorksheetActive("Otros");
                    excel.SetValue(row[1], "O" + (9 + i));
                    excel.SetValue(row[2], "P" + (9 + i));
                    i++;
                }
                Trace.WriteLine("Otros - Recargas a crédito ¡Listo!", "INFO");

                DateTime iniDate = DateTime.Parse(weekFirstDay);
                DateTime endDate = iniDate.AddDays(6);
                String timeLapse = "";
                if (iniDate.Year.Equals(endDate.Year))
                    if (iniDate.Month.Equals(endDate.Month))
                        timeLapse = String.Format("{0:dd} al {1:dd} de {0:MMMM} del {0:yyyy}", iniDate, endDate);
                    else
                        timeLapse = String.Format("{0:dd} de {0:MMMM} al {1:dd} de {1:MMMM} del {0:yyyy}", iniDate, endDate);
                else
                    timeLapse = String.Format("{0:dd} de {0:MMMM} del {0:yyyy} al {1:dd} de {1:MMMM} del {1:yyyy}", iniDate, endDate);

                excel.SetWorksheetActive("Contraprestación Anexo\"C\"");
                excel.SetValue(timeLapse, "D5");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "ERROR");
                MessageBox.Show("Ocurrió un error al generar el reporte, verifique que la comunicación a la base de datos exista.",
                                                                        "Error en reporte", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            SaveFileDialog saveReport = new SaveFileDialog()
            {
                Filter = "Archivo Microsoft Excel 2016 (*.xlsx)|*.xlsx",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (saveReport.ShowDialog() == DialogResult.OK)
            {
                excel.SaveAs(saveReport.FileName);
            }

            excel.Close();
            Excel.Quit();
        }
    }
}
