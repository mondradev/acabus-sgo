namespace ACABUS_Control_de_operacion
{
    partial class Monitor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.counterLabel = new System.Windows.Forms.Label();
            this.disconnectVehicleLabel = new System.Windows.Forms.Label();
            this.badCountersTable = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusCounterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastAccessColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.disconnectVehicleTable = new System.Windows.Forms.DataGridView();
            this.NoEcon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ValPending = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.alarmTrunkTable = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.badCountersTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.disconnectVehicleTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.alarmTrunkTable)).BeginInit();
            this.SuspendLayout();
            // 
            // counterLabel
            // 
            this.counterLabel.AutoSize = true;
            this.counterLabel.Location = new System.Drawing.Point(375, 56);
            this.counterLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.counterLabel.Name = "counterLabel";
            this.counterLabel.Size = new System.Drawing.Size(173, 24);
            this.counterLabel.TabIndex = 12;
            this.counterLabel.Text = "Falla en contadores";
            // 
            // disconnectVehicleLabel
            // 
            this.disconnectVehicleLabel.AutoSize = true;
            this.disconnectVehicleLabel.Location = new System.Drawing.Point(10, 54);
            this.disconnectVehicleLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.disconnectVehicleLabel.Name = "disconnectVehicleLabel";
            this.disconnectVehicleLabel.Size = new System.Drawing.Size(199, 24);
            this.disconnectVehicleLabel.TabIndex = 11;
            this.disconnectVehicleLabel.Text = "Vehículos sin conexión";
            // 
            // badCountersTable
            // 
            this.badCountersTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.badCountersTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.badCountersTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.statusCounterColumn,
            this.lastAccessColumn});
            this.badCountersTable.Location = new System.Drawing.Point(379, 86);
            this.badCountersTable.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.badCountersTable.Name = "badCountersTable";
            this.badCountersTable.RowHeadersVisible = false;
            this.badCountersTable.Size = new System.Drawing.Size(304, 602);
            this.badCountersTable.TabIndex = 10;
            this.badCountersTable.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.ResultTableOnRowsAdded);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn1.HeaderText = "No. Económico";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 150;
            // 
            // statusCounterColumn
            // 
            this.statusCounterColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.statusCounterColumn.HeaderText = "Estado del contador";
            this.statusCounterColumn.Name = "statusCounterColumn";
            this.statusCounterColumn.ReadOnly = true;
            this.statusCounterColumn.Width = 190;
            // 
            // lastAccessColumn
            // 
            this.lastAccessColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.lastAccessColumn.HeaderText = "Ultima validación";
            this.lastAccessColumn.Name = "lastAccessColumn";
            this.lastAccessColumn.ReadOnly = true;
            this.lastAccessColumn.Width = 168;
            // 
            // disconnectVehicleTable
            // 
            this.disconnectVehicleTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.disconnectVehicleTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.disconnectVehicleTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NoEcon,
            this.ValPending});
            this.disconnectVehicleTable.Location = new System.Drawing.Point(14, 86);
            this.disconnectVehicleTable.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.disconnectVehicleTable.Name = "disconnectVehicleTable";
            this.disconnectVehicleTable.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.disconnectVehicleTable.RowHeadersVisible = false;
            this.disconnectVehicleTable.Size = new System.Drawing.Size(360, 602);
            this.disconnectVehicleTable.TabIndex = 9;
            this.disconnectVehicleTable.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.ResultTableOnRowsAdded);
            // 
            // NoEcon
            // 
            this.NoEcon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.NoEcon.HeaderText = "No. Económico";
            this.NoEcon.Name = "NoEcon";
            this.NoEcon.ReadOnly = true;
            this.NoEcon.Width = 150;
            // 
            // ValPending
            // 
            this.ValPending.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ValPending.HeaderText = "Última conexión";
            this.ValPending.Name = "ValPending";
            this.ValPending.ReadOnly = true;
            this.ValPending.Width = 160;
            // 
            // alarmTrunkTable
            // 
            this.alarmTrunkTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.alarmTrunkTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.alarmTrunkTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.Column1});
            this.alarmTrunkTable.Location = new System.Drawing.Point(693, 86);
            this.alarmTrunkTable.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.alarmTrunkTable.Name = "alarmTrunkTable";
            this.alarmTrunkTable.RowHeadersVisible = false;
            this.alarmTrunkTable.Size = new System.Drawing.Size(606, 602);
            this.alarmTrunkTable.TabIndex = 13;
            this.alarmTrunkTable.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.ResultTableOnRowsAdded);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(802, 54);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 24);
            this.label1.TabIndex = 14;
            this.label1.Text = "Alarmas de vía";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(14, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(94, 39);
            this.button1.TabIndex = 15;
            this.button1.Text = "&Iniciar";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.StartOnClick);
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(115, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(94, 39);
            this.button2.TabIndex = 16;
            this.button2.Text = "&Detener";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.StopOnClick);
            // 
            // timer1
            // 
            this.timer1.Interval = 10000;
            this.timer1.Tick += new System.EventHandler(this.RefreshOnTick);
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn2.HeaderText = "Equipo";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Width = 98;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn3.HeaderText = "Descripción";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Width = 135;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column1.HeaderText = "Fecha";
            this.Column1.Name = "Column1";
            this.Column1.Width = 87;
            // 
            // Monitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1315, 703);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.alarmTrunkTable);
            this.Controls.Add(this.counterLabel);
            this.Controls.Add(this.disconnectVehicleLabel);
            this.Controls.Add(this.badCountersTable);
            this.Controls.Add(this.disconnectVehicleTable);
            this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1160, 750);
            this.Name = "Monitor";
            this.Text = "Monitor";
            ((System.ComponentModel.ISupportInitialize)(this.badCountersTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.disconnectVehicleTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.alarmTrunkTable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label counterLabel;
        private System.Windows.Forms.Label disconnectVehicleLabel;
        private System.Windows.Forms.DataGridView badCountersTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusCounterColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastAccessColumn;
        private System.Windows.Forms.DataGridView disconnectVehicleTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn NoEcon;
        private System.Windows.Forms.DataGridViewTextBoxColumn ValPending;
        private System.Windows.Forms.DataGridView alarmTrunkTable;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
    }
}