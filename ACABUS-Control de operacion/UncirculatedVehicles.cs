using System;
using System.IO;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class UncirculatedVehicles : Form
    {
        internal const String FILENAME_UNCIRCULATED_VEHICLES = "uncirculated_vehicles.list";

        public UncirculatedVehicles()
        {
            InitializeComponent();
            LoadUncirculatedVehicles();
        }

        private void LoadUncirculatedVehicles()
        {
            ClearTable();
            if (!File.Exists(FILENAME_UNCIRCULATED_VEHICLES)) return;
            String[] lines = File.ReadAllLines(FILENAME_UNCIRCULATED_VEHICLES);
            foreach (String line in lines)
            {
                this.uncirculatedTable.Rows.Add(line.Split(','));
            }
        }

        private void ClearTable()
        {
            this.uncirculatedTable.Rows.Clear();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            ClearTable();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveData();
                ClearTable();
                LoadUncirculatedVehicles();
                MessageBox.Show("Vehículos sin circular guardados correctamente.");
                Hide();
                Dispose();
            }
            catch (Exception)
            {
                File.Delete(FILENAME_UNCIRCULATED_VEHICLES);
                ClearTable();
                LoadUncirculatedVehicles();
            }
        }

        private void SaveData()
        {
            File.Delete(FILENAME_UNCIRCULATED_VEHICLES);
            foreach (DataGridViewRow item in this.uncirculatedTable.Rows)
            {
                if (item.Cells[0].Value == null) continue;
                String vehicle = String.Format("{0},{1}\n", item.Cells[0].Value, item.Cells[1].Value);
                File.AppendAllText(FILENAME_UNCIRCULATED_VEHICLES, vehicle);
            }
        }

        private void UncirculatedVehiOnCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && String.IsNullOrEmpty(this.uncirculatedTable.Rows[e.RowIndex].Cells[1].Value?.ToString()))
                this.uncirculatedTable.Rows[e.RowIndex].Cells[1].Value = "TALLER";
        }

        private void RefreshButtonOnClick(object sender, EventArgs e)
        {
            LoadUncirculatedVehicles();
        }
    }
}
