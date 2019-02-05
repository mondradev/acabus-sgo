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
        }

        private void FillReport(String fileName = null)
        {
            Excel excel = Excel.Open(AcabusData.TEMPLATE_WEEK_REPORT);

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
            excel.SetValue(timeLapse, "D6");


            Trace.WriteLine(String.Format("Periodo {0}", timeLapse));                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               

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
                    if (String.IsNullOrEmpty(row[0])) break;
                    excel.SetWorksheetActive("Información_Troncal-Anexo\"A\"");
                    excel.SetValue(row[1], "E" + (12 + i));
                    excel.SetValue(row[2], "F" + (12 + i));
                    excel.SetWorksheetActive("Otros");
                    excel.SetValue(row[3], "S" + (9 + i));
                    excel.SetValue(row[4], "T" + (9 + i));
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
                    if (String.IsNullOrEmpty(row[0])) break;
                    excel.SetWorksheetActive("Información_Troncal-Anexo\"A\"");
                    excel.SetValue(row[1], "AG" + (12 + i));
                    excel.SetValue(row[2], "AH" + (12 + i));
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
                    if (String.IsNullOrEmpty(row[0])) break;
                    excel.SetWorksheetActive("Información_Troncal-Anexo\"A\"");
                    excel.SetValue(row[1], "AI" + (12 + i));
                    excel.SetValue(row[2], "AJ" + (12 + i));
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
                    if (String.IsNullOrEmpty(row[0])) break;
                    excel.SetWorksheetActive("DetalleValidacion");

                    while (!DateTime.FromOADate(Double.Parse(excel.GetValue("B" + (9 + i)))).Equals(DateTime.Parse(row[0])) & i < 7)
                        i++;

                    if (i > 6)
                        break;

                    excel.SetValue(row[1], "T" + (9 + i));
                    excel.SetValue(row[2], "U" + (9 + i));
                    excel.SetValue(row[3], "V" + (9 + i));
                    excel.SetValue(row[4], "W" + (9 + i));
                    excel.SetValue(row[5], "X" + (9 + i));
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
                    if (String.IsNullOrEmpty(row[0])) break;
                    excel.SetWorksheetActive("DetalleValidacion");

                    while (!DateTime.FromOADate(Double.Parse(excel.GetValue("B" + (9 + i)))).Equals(DateTime.Parse(row[0])) & i < 7)
                        i++;

                    if (i > 6)
                        break;

                    excel.SetValue(row[1], "J" + (9 + i));
                    excel.SetValue(row[2], "K" + (9 + i));
                    excel.SetValue(row[3], "L" + (9 + i));
                    excel.SetValue(row[4], "M" + (9 + i));
                    excel.SetValue(row[5], "Y" + (9 + i));
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
                    if (String.IsNullOrEmpty(row[0])) break;
                    excel.SetWorksheetActive("Otros");

                    while (i < 7 && !DateTime.FromOADate(Double.Parse(excel.GetValue("B" + (9 + i)))).Equals(DateTime.Parse(row[0])))
                        i++;

                    if (i > 6)
                        break;

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
                    if (String.IsNullOrEmpty(row[0])) break;
                    excel.SetWorksheetActive("Otros");

                    while (i < 7 && !DateTime.FromOADate(Double.Parse(excel.GetValue("B" + (9 + i)))).Equals(DateTime.Parse(row[0])))
                        i++;

                    if (i > 6)
                        break;

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
                    if (String.IsNullOrEmpty(row[0])) break;
                    excel.SetWorksheetActive("Otros");

                    while (i < 7 && !DateTime.FromOADate(Double.Parse(excel.GetValue("B" + (9 + i)))).Equals(DateTime.Parse(row[0])))
                        i++;

                    if (i > 6)
                        break;

                    excel.SetValue(row[1], "O" + (9 + i));
                    excel.SetValue(row[2], "P" + (9 + i));
                    i++;
                }
                Trace.WriteLine("Otros - Recargas a crédito ¡Listo!", "INFO");                

                // --- Nuevos campos

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_MANTTO_TRUNK)
                                                                               .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    if (String.IsNullOrEmpty(row[0])) break;
                    excel.SetWorksheetActive("DetalleValidacion");

                    while (i < 7 && !DateTime.FromOADate(Double.Parse(excel.GetValue("B" + (9 + i)))).Equals(DateTime.Parse(row[0])))
                        i++;

                    if (i > 6)
                        break;

                    excel.SetValue(row[1], "AA" + (9 + i));
                    i++;
                }
                Trace.WriteLine("Detalle Validación - Mantenimiento en estaciones ¡Listo!", "INFO");

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_MANTTO_VEHI)
                                                                             .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    if (String.IsNullOrEmpty(row[0])) break;
                    excel.SetWorksheetActive("DetalleValidacion");

                    while (i < 7 && !DateTime.FromOADate(Double.Parse(excel.GetValue("B" + (9 + i)))).Equals(DateTime.Parse(row[0])))
                        i++;

                    if (i > 6)
                        break;

                    excel.SetValue(row[1], "Z" + (9 + i));
                    i++;
                }
                Trace.WriteLine("Detalle Validación - Mantenimiento en vehiculo ¡Listo!", "INFO");

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_TPV)
                                                                               .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    if (String.IsNullOrEmpty(row[0])) break;

                    excel.SetWorksheetActive("Información_Troncal-Anexo\"A\"");

                    while (i < 7 && !DateTime.FromOADate(Double.Parse(excel.GetValue("I" + (12 + i)))).Equals(DateTime.Parse(row[0])))
                        i++;

                    if (i > 6)
                        break;

                    excel.SetValue(row[1], "AK" + (12 + i));
                    excel.SetValue(row[2], "AL" + (12 + i));
                    i++;
                }
                Trace.WriteLine("Recargas y abonos en TPV ¡Listo!", "INFO");

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_TPV_CRED)
                                                                              .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    if (String.IsNullOrEmpty(row[0])) break;

                    excel.SetWorksheetActive("Otros");

                    while (i < 7 && !DateTime.FromOADate(Double.Parse(excel.GetValue("B" + (9 + i)))).Equals(DateTime.Parse(row[0])))
                        i++;

                    if (i > 6)
                        break;

                    excel.SetValue(row[1], "Q" + (9 + i));
                    excel.SetValue(row[2], "R" + (9 + i));
                    i++;
                }
                Trace.WriteLine("Recargas y abonos a créditos en TPV ¡Listo!", "INFO");

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_DISC_TRUNK)
                                                                              .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    if (String.IsNullOrEmpty(row[0])) break;
                    excel.SetWorksheetActive("DetalleValidacion");

                    while (i < 7 && !DateTime.FromOADate(Double.Parse(excel.GetValue("B" + (9 + i)))).Equals(DateTime.Parse(row[0])))
                        i++;

                    if (i > 6)
                        break;

                    excel.SetValue(row[1], "O" + (9 + i));
                    i++;
                }

                Trace.WriteLine("Detalle Validación - Preferencial en estaciones ¡Listo!", "INFO");

                sqlResponse = psql.ExecuteQuery(File.ReadAllText(AcabusData.QUERY_FILE_DISC_VEHI)
                                                                             .Replace("{paramFchIni}", weekFirstDay));
                readHeader = false;
                i = 0;
                foreach (String[] row in sqlResponse)
                {
                    if (!readHeader) { readHeader = true; continue; }
                    if (String.IsNullOrEmpty(row[0])) break;
                    excel.SetWorksheetActive("DetalleValidacion");

                      while (i < 7 && !DateTime.FromOADate(Double.Parse(excel.GetValue("B" + (9 + i)))).Equals(DateTime.Parse(row[0])))
                        i++;

                    if (i > 6)
                        break;

                    excel.SetValue(row[1], "H" + (9 + i));
                    i++;
                }
                Trace.WriteLine("Detalle Validación - Preferencial en vehiculo ¡Listo!", "INFO");

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "ERROR");
                MessageBox.Show("Ocurrió un error al generar el reporte, verifique que la comunicación a la base de datos exista.",
                                                                        "Error en reporte", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                excel.SaveAs(fileName);

                excel.Close();
                Excel.Quit();

                return;
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

        private void Button1_Click(object sender, EventArgs e)
        {
            weekFirstDay = String.Format("{0:yyyy-MM-dd}", dateTimePicker1.Value);
            FillReport();
        }

        private void GenerateBtn_Click(object sender, EventArgs e)
        {
            var reports = Int32.Parse(cantTxt.Text);
            var cont = 0;

            if (reports <= 0)
                return;

            var ofd = new FolderBrowserDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            while (cont < reports)
            {
                weekFirstDay = String.Format("{0:yyyy-MM-dd}", dateTimePicker1.Value.AddDays(cont * 7));
                var name = String.Format("Reporte_Semana_{0}_Acabus.xlsx", weekFirstDay);
                name = Path.Combine(ofd.SelectedPath, name);

                FillReport(name);
                Trace.WriteLine("Reporte: " + name + " completado!", "INFO");
                cont++;
            }

            Trace.WriteLine("Fin del trabajo", "INFO");
        }
    }
}