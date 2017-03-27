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
            this.disconnectVehicleTable = new System.Windows.Forms.DataGridView();
            this.alarmTrunkTable = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.uncirculatedVehiclesButton = new System.Windows.Forms.Button();
            this.NoEcon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ValPending = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusCounterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastAccessColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.badCountersTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.disconnectVehicleTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.alarmTrunkTable)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // counterLabel
            // 
            this.counterLabel.AutoSize = true;
            this.counterLabel.Location = new System.Drawing.Point(1, 0);
            this.counterLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.counterLabel.Name = "counterLabel";
            this.counterLabel.Size = new System.Drawing.Size(173, 24);
            this.counterLabel.TabIndex = 12;
            this.counterLabel.Text = "Falla en contadores";
            // 
            // disconnectVehicleLabel
            // 
            this.disconnectVehicleLabel.AutoSize = true;
            this.disconnectVehicleLabel.Location = new System.Drawing.Point(5, 0);
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
            this.badCountersTable.Location = new System.Drawing.Point(5, 30);
            this.badCountersTable.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.badCountersTable.Name = "badCountersTable";
            this.badCountersTable.RowHeadersVisible = false;
            this.badCountersTable.Size = new System.Drawing.Size(410, 599);
            this.badCountersTable.TabIndex = 10;
            this.badCountersTable.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.ResultTableOnRowsAdded);
            // 
            // disconnectVehicleTable
            // 
            this.disconnectVehicleTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.disconnectVehicleTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.disconnectVehicleTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NoEcon,
            this.ValPending});
            this.disconnectVehicleTable.Location = new System.Drawing.Point(9, 30);
            this.disconnectVehicleTable.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.disconnectVehicleTable.Name = "disconnectVehicleTable";
            this.disconnectVehicleTable.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.disconnectVehicleTable.RowHeadersVisible = false;
            this.disconnectVehicleTable.Size = new System.Drawing.Size(264, 599);
            this.disconnectVehicleTable.TabIndex = 9;
            this.disconnectVehicleTable.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.ResultTableOnRowsAdded);
            // 
            // alarmTrunkTable
            // 
            this.alarmTrunkTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.alarmTrunkTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.alarmTrunkTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.Column1});
            this.alarmTrunkTable.Location = new System.Drawing.Point(0, 30);
            this.alarmTrunkTable.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.alarmTrunkTable.Name = "alarmTrunkTable";
            this.alarmTrunkTable.RowHeadersVisible = false;
            this.alarmTrunkTable.Size = new System.Drawing.Size(570, 599);
            this.alarmTrunkTable.TabIndex = 13;
            this.alarmTrunkTable.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.ResultTableOnRowsAdded);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(195, 24);
            this.label1.TabIndex = 14;
            this.label1.Text = "Alarmas de vía de hoy";
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(14, 12);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(94, 37);
            this.startButton.TabIndex = 15;
            this.startButton.Text = "&Iniciar";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.StartOnClick);
            // 
            // StopButton
            // 
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(115, 12);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(94, 37);
            this.StopButton.TabIndex = 16;
            this.StopButton.Text = "&Detener";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopOnClick);
            // 
            // timer1
            // 
            this.timer1.Interval = 10000;
            this.timer1.Tick += new System.EventHandler(this.RefreshOnTick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel1.Controls.Add(this.panel3, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 54);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1291, 641);
            this.tableLayoutPanel1.TabIndex = 17;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.alarmTrunkTable);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Location = new System.Drawing.Point(713, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(575, 635);
            this.panel3.TabIndex = 19;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.badCountersTable);
            this.panel2.Controls.Add(this.counterLabel);
            this.panel2.Location = new System.Drawing.Point(287, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(420, 635);
            this.panel2.TabIndex = 18;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.disconnectVehicleTable);
            this.panel1.Controls.Add(this.disconnectVehicleLabel);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(278, 635);
            this.panel1.TabIndex = 12;
            // 
            // uncirculatedVehiclesButton
            // 
            this.uncirculatedVehiclesButton.Location = new System.Drawing.Point(215, 11);
            this.uncirculatedVehiclesButton.Name = "uncirculatedVehiclesButton";
            this.uncirculatedVehiclesButton.Size = new System.Drawing.Size(199, 37);
            this.uncirculatedVehiclesButton.TabIndex = 18;
            this.uncirculatedVehiclesButton.Text = "&Vehículos s/circular";
            this.uncirculatedVehiclesButton.UseVisualStyleBackColor = true;
            this.uncirculatedVehiclesButton.Click += new System.EventHandler(this.UncirculatedVehiclesButton_Click);
            // 
            // NoEcon
            // 
            this.NoEcon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.NoEcon.HeaderText = "No. Económico";
            this.NoEcon.Name = "NoEcon";
            this.NoEcon.ReadOnly = true;
            // 
            // ValPending
            // 
            this.ValPending.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ValPending.HeaderText = "Última conexión";
            this.ValPending.Name = "ValPending";
            this.ValPending.ReadOnly = true;
            this.ValPending.Width = 160;
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
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn3.HeaderText = "Descripción";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column1.HeaderText = "Hora";
            this.Column1.Name = "Column1";
            this.Column1.Width = 79;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.HeaderText = "No. Económico";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // statusCounterColumn
            // 
            this.statusCounterColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.statusCounterColumn.HeaderText = "Estado del contador";
            this.statusCounterColumn.Name = "statusCounterColumn";
            this.statusCounterColumn.ReadOnly = true;
            // 
            // lastAccessColumn
            // 
            this.lastAccessColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.lastAccessColumn.HeaderText = "Ultima validación";
            this.lastAccessColumn.Name = "lastAccessColumn";
            this.lastAccessColumn.ReadOnly = true;
            this.lastAccessColumn.Width = 168;
            // 
            // Monitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1315, 703);
            this.Controls.Add(this.uncirculatedVehiclesButton);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.startButton);
            this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1160, 750);
            this.Name = "Monitor";
            this.Text = "Monitor";
            ((System.ComponentModel.ISupportInitialize)(this.badCountersTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.disconnectVehicleTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.alarmTrunkTable)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label counterLabel;
        private System.Windows.Forms.Label disconnectVehicleLabel;
        private System.Windows.Forms.DataGridView badCountersTable;
        private System.Windows.Forms.DataGridView disconnectVehicleTable;
        private System.Windows.Forms.DataGridView alarmTrunkTable;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button uncirculatedVehiclesButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn NoEcon;
        private System.Windows.Forms.DataGridViewTextBoxColumn ValPending;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusCounterColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastAccessColumn;
    }
}