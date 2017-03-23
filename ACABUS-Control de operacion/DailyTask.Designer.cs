namespace ACABUS_Control_de_operacion
{
    partial class DailyTask
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DailyTask));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.runQueryButton = new System.Windows.Forms.ToolStripButton();
            this.checkReplicaButton = new System.Windows.Forms.ToolStripButton();
            this.stopTaskButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.resultTable = new System.Windows.Forms.DataGridView();
            this.statusBarra = new System.Windows.Forms.StatusStrip();
            this.currentTaskLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.taskProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.progressLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.execTimeLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.threadsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.disconnectVehicleTable = new System.Windows.Forms.DataGridView();
            this.badCountersTable = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.disconnectVehicleLabel = new System.Windows.Forms.Label();
            this.counterLabel = new System.Windows.Forms.Label();
            this.queryBox = new System.Windows.Forms.TextBox();
            this.queryLabel = new System.Windows.Forms.Label();
            this.kvrCheck = new System.Windows.Forms.CheckBox();
            this.pmrCheck = new System.Windows.Forms.CheckBox();
            this.toresCheck = new System.Windows.Forms.CheckBox();
            this.stationsList = new System.Windows.Forms.ComboBox();
            this.deviceList = new System.Windows.Forms.ComboBox();
            this.taskTimeTimer = new System.Windows.Forms.Timer(this.components);
            this.NoEcon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ValPending = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusCounterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastAccessColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resultTable)).BeginInit();
            this.statusBarra.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.disconnectVehicleTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.badCountersTable)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runQueryButton,
            this.checkReplicaButton,
            this.stopTaskButton,
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1182, 27);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // runQueryButton
            // 
            this.runQueryButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.runQueryButton.Image = ((System.Drawing.Image)(resources.GetObject("runQueryButton.Image")));
            this.runQueryButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runQueryButton.Name = "runQueryButton";
            this.runQueryButton.Size = new System.Drawing.Size(127, 24);
            this.runQueryButton.Text = "Ejecutar Consulta";
            this.runQueryButton.Click += new System.EventHandler(this.RunQueryInDeviceOnClick);
            // 
            // checkReplicaButton
            // 
            this.checkReplicaButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.checkReplicaButton.Image = ((System.Drawing.Image)(resources.GetObject("checkReplicaButton.Image")));
            this.checkReplicaButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.checkReplicaButton.Name = "checkReplicaButton";
            this.checkReplicaButton.Size = new System.Drawing.Size(120, 24);
            this.checkReplicaButton.Text = "Verificar &Réplica";
            this.checkReplicaButton.Click += new System.EventHandler(this.CheckReplicaButtonOnClick);
            // 
            // stopTaskButton
            // 
            this.stopTaskButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.stopTaskButton.Enabled = false;
            this.stopTaskButton.Image = ((System.Drawing.Image)(resources.GetObject("stopTaskButton.Image")));
            this.stopTaskButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stopTaskButton.Name = "stopTaskButton";
            this.stopTaskButton.Size = new System.Drawing.Size(105, 24);
            this.stopTaskButton.Text = "&Detener Tarea";
            this.stopTaskButton.Click += new System.EventHandler(this.StopTaskButtonOnClick);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(79, 24);
            this.toolStripButton1.Text = "Actualizar";
            this.toolStripButton1.Click += new System.EventHandler(this.RefreshOnClick);
            // 
            // resultTable
            // 
            this.resultTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.resultTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.resultTable.Location = new System.Drawing.Point(313, 197);
            this.resultTable.Margin = new System.Windows.Forms.Padding(4);
            this.resultTable.Name = "resultTable";
            this.resultTable.Size = new System.Drawing.Size(615, 323);
            this.resultTable.TabIndex = 1;
            this.resultTable.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.ResultTableOnRowsAdded);
            // 
            // statusBarra
            // 
            this.statusBarra.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusBarra.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.currentTaskLabel,
            this.taskProgressBar,
            this.progressLabel,
            this.execTimeLabel,
            this.threadsStatusLabel});
            this.statusBarra.Location = new System.Drawing.Point(0, 528);
            this.statusBarra.Name = "statusBarra";
            this.statusBarra.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusBarra.Size = new System.Drawing.Size(1182, 25);
            this.statusBarra.TabIndex = 2;
            this.statusBarra.Text = "Barra de estado";
            // 
            // currentTaskLabel
            // 
            this.currentTaskLabel.Name = "currentTaskLabel";
            this.currentTaskLabel.Size = new System.Drawing.Size(151, 20);
            this.currentTaskLabel.Text = "Tarea actual: Ninguna";
            // 
            // taskProgressBar
            // 
            this.taskProgressBar.Margin = new System.Windows.Forms.Padding(0);
            this.taskProgressBar.Name = "taskProgressBar";
            this.taskProgressBar.Size = new System.Drawing.Size(800, 25);
            // 
            // progressLabel
            // 
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(33, 20);
            this.progressLabel.Text = "0 %";
            // 
            // execTimeLabel
            // 
            this.execTimeLabel.Name = "execTimeLabel";
            this.execTimeLabel.Size = new System.Drawing.Size(40, 20);
            this.execTimeLabel.Text = "0 ms";
            // 
            // threadsStatusLabel
            // 
            this.threadsStatusLabel.Name = "threadsStatusLabel";
            this.threadsStatusLabel.Size = new System.Drawing.Size(105, 20);
            this.threadsStatusLabel.Text = "0 Subprocesos";
            // 
            // disconnectVehicleTable
            // 
            this.disconnectVehicleTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.disconnectVehicleTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.disconnectVehicleTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NoEcon,
            this.ValPending});
            this.disconnectVehicleTable.Location = new System.Drawing.Point(16, 69);
            this.disconnectVehicleTable.Margin = new System.Windows.Forms.Padding(4);
            this.disconnectVehicleTable.Name = "disconnectVehicleTable";
            this.disconnectVehicleTable.Size = new System.Drawing.Size(289, 451);
            this.disconnectVehicleTable.TabIndex = 3;
            // 
            // badCountersTable
            // 
            this.badCountersTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.badCountersTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.badCountersTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.statusCounterColumn,
            this.lastAccessColumn});
            this.badCountersTable.Location = new System.Drawing.Point(940, 69);
            this.badCountersTable.Margin = new System.Windows.Forms.Padding(4);
            this.badCountersTable.Name = "badCountersTable";
            this.badCountersTable.Size = new System.Drawing.Size(229, 451);
            this.badCountersTable.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(313, 169);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(212, 24);
            this.label1.TabIndex = 6;
            this.label1.Text = "Resultados de consultas";
            // 
            // disconnectVehicleLabel
            // 
            this.disconnectVehicleLabel.AutoSize = true;
            this.disconnectVehicleLabel.Location = new System.Drawing.Point(13, 41);
            this.disconnectVehicleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.disconnectVehicleLabel.Name = "disconnectVehicleLabel";
            this.disconnectVehicleLabel.Size = new System.Drawing.Size(199, 24);
            this.disconnectVehicleLabel.TabIndex = 7;
            this.disconnectVehicleLabel.Text = "Vehículos sin conexión";
            // 
            // counterLabel
            // 
            this.counterLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.counterLabel.AutoSize = true;
            this.counterLabel.Location = new System.Drawing.Point(936, 41);
            this.counterLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.counterLabel.Name = "counterLabel";
            this.counterLabel.Size = new System.Drawing.Size(173, 24);
            this.counterLabel.TabIndex = 8;
            this.counterLabel.Text = "Falla en contadores";
            // 
            // queryBox
            // 
            this.queryBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.queryBox.Location = new System.Drawing.Point(581, 69);
            this.queryBox.Multiline = true;
            this.queryBox.Name = "queryBox";
            this.queryBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.queryBox.Size = new System.Drawing.Size(347, 97);
            this.queryBox.TabIndex = 9;
            // 
            // queryLabel
            // 
            this.queryLabel.AutoSize = true;
            this.queryLabel.Location = new System.Drawing.Point(577, 41);
            this.queryLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.queryLabel.Name = "queryLabel";
            this.queryLabel.Size = new System.Drawing.Size(119, 24);
            this.queryLabel.TabIndex = 10;
            this.queryLabel.Text = "Consulta SQL";
            // 
            // kvrCheck
            // 
            this.kvrCheck.AutoSize = true;
            this.kvrCheck.Checked = true;
            this.kvrCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.kvrCheck.Location = new System.Drawing.Point(317, 67);
            this.kvrCheck.Name = "kvrCheck";
            this.kvrCheck.Size = new System.Drawing.Size(64, 28);
            this.kvrCheck.TabIndex = 11;
            this.kvrCheck.Text = "KVR";
            this.kvrCheck.UseVisualStyleBackColor = true;
            // 
            // pmrCheck
            // 
            this.pmrCheck.AutoSize = true;
            this.pmrCheck.Checked = true;
            this.pmrCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.pmrCheck.Location = new System.Drawing.Point(317, 102);
            this.pmrCheck.Name = "pmrCheck";
            this.pmrCheck.Size = new System.Drawing.Size(70, 28);
            this.pmrCheck.TabIndex = 12;
            this.pmrCheck.Text = "PMR";
            this.pmrCheck.UseVisualStyleBackColor = true;
            // 
            // toresCheck
            // 
            this.toresCheck.AutoSize = true;
            this.toresCheck.Checked = true;
            this.toresCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toresCheck.Location = new System.Drawing.Point(317, 137);
            this.toresCheck.Name = "toresCheck";
            this.toresCheck.Size = new System.Drawing.Size(90, 28);
            this.toresCheck.TabIndex = 13;
            this.toresCheck.Text = "Tor E/S";
            this.toresCheck.UseVisualStyleBackColor = true;
            // 
            // stationsList
            // 
            this.stationsList.FormattingEnabled = true;
            this.stationsList.Location = new System.Drawing.Point(407, 64);
            this.stationsList.Name = "stationsList";
            this.stationsList.Size = new System.Drawing.Size(168, 32);
            this.stationsList.TabIndex = 15;
            this.stationsList.SelectionChangeCommitted += new System.EventHandler(this.DeviceListOnSelectedChanged);
            // 
            // deviceList
            // 
            this.deviceList.FormattingEnabled = true;
            this.deviceList.Location = new System.Drawing.Point(407, 102);
            this.deviceList.Name = "deviceList";
            this.deviceList.Size = new System.Drawing.Size(168, 32);
            this.deviceList.TabIndex = 16;
            // 
            // taskTimeTimer
            // 
            this.taskTimeTimer.Interval = 1;
            // 
            // NoEcon
            // 
            this.NoEcon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.NoEcon.HeaderText = "No. Económico";
            this.NoEcon.Name = "NoEcon";
            this.NoEcon.ReadOnly = true;
            this.NoEcon.Width = 164;
            // 
            // ValPending
            // 
            this.ValPending.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ValPending.HeaderText = "Última conexión";
            this.ValPending.Name = "ValPending";
            this.ValPending.ReadOnly = true;
            this.ValPending.Width = 160;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn1.HeaderText = "No. Económico";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 164;
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
            // DailyTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1182, 553);
            this.Controls.Add(this.deviceList);
            this.Controls.Add(this.stationsList);
            this.Controls.Add(this.toresCheck);
            this.Controls.Add(this.pmrCheck);
            this.Controls.Add(this.kvrCheck);
            this.Controls.Add(this.queryLabel);
            this.Controls.Add(this.queryBox);
            this.Controls.Add(this.counterLabel);
            this.Controls.Add(this.disconnectVehicleLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.badCountersTable);
            this.Controls.Add(this.disconnectVehicleTable);
            this.Controls.Add(this.statusBarra);
            this.Controls.Add(this.resultTable);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DailyTask";
            this.Text = "DailyTask";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resultTable)).EndInit();
            this.statusBarra.ResumeLayout(false);
            this.statusBarra.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.disconnectVehicleTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.badCountersTable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton checkReplicaButton;
        private System.Windows.Forms.DataGridView resultTable;
        private System.Windows.Forms.StatusStrip statusBarra;
        private System.Windows.Forms.ToolStripStatusLabel currentTaskLabel;
        private System.Windows.Forms.ToolStripProgressBar taskProgressBar;
        private System.Windows.Forms.ToolStripButton runQueryButton;
        private System.Windows.Forms.DataGridView disconnectVehicleTable;
        private System.Windows.Forms.DataGridView badCountersTable;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label disconnectVehicleLabel;
        private System.Windows.Forms.Label counterLabel;
        private System.Windows.Forms.TextBox queryBox;
        private System.Windows.Forms.Label queryLabel;
        private System.Windows.Forms.CheckBox kvrCheck;
        private System.Windows.Forms.CheckBox pmrCheck;
        private System.Windows.Forms.CheckBox toresCheck;
        private System.Windows.Forms.ComboBox stationsList;
        private System.Windows.Forms.ComboBox deviceList;
        private System.Windows.Forms.ToolStripButton stopTaskButton;
        private System.Windows.Forms.ToolStripStatusLabel progressLabel;
        private System.Windows.Forms.ToolStripStatusLabel execTimeLabel;
        private System.Windows.Forms.Timer taskTimeTimer;
        private System.Windows.Forms.ToolStripStatusLabel threadsStatusLabel;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.DataGridViewTextBoxColumn NoEcon;
        private System.Windows.Forms.DataGridViewTextBoxColumn ValPending;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusCounterColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastAccessColumn;
    }
}