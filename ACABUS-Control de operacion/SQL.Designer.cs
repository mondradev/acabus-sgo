namespace ACABUS_Control_de_operacion
{
    partial class SQLModule
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
            this.resultTable = new System.Windows.Forms.DataGridView();
            this.statusBarra = new System.Windows.Forms.StatusStrip();
            this.currentTaskLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.taskProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.progressLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.execTimeLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.threadsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.queryBox = new System.Windows.Forms.TextBox();
            this.queryLabel = new System.Windows.Forms.Label();
            this.kvrCheck = new System.Windows.Forms.CheckBox();
            this.pmrCheck = new System.Windows.Forms.CheckBox();
            this.toresCheck = new System.Windows.Forms.CheckBox();
            this.stationsList = new System.Windows.Forms.ComboBox();
            this.deviceList = new System.Windows.Forms.ComboBox();
            this.taskTimeTimer = new System.Windows.Forms.Timer(this.components);
            this.runQueryButton = new System.Windows.Forms.Button();
            this.checkReplicaButton = new System.Windows.Forms.Button();
            this.stopTaskButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.resultTable)).BeginInit();
            this.statusBarra.SuspendLayout();
            this.SuspendLayout();
            // 
            // resultTable
            // 
            this.resultTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.resultTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.resultTable.Location = new System.Drawing.Point(554, 69);
            this.resultTable.Margin = new System.Windows.Forms.Padding(4);
            this.resultTable.Name = "resultTable";
            this.resultTable.Size = new System.Drawing.Size(615, 450);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(550, 41);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(212, 24);
            this.label1.TabIndex = 6;
            this.label1.Text = "Resultados de consultas";
            // 
            // queryBox
            // 
            this.queryBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.queryBox.Location = new System.Drawing.Point(12, 196);
            this.queryBox.Multiline = true;
            this.queryBox.Name = "queryBox";
            this.queryBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.queryBox.Size = new System.Drawing.Size(535, 323);
            this.queryBox.TabIndex = 9;
            // 
            // queryLabel
            // 
            this.queryLabel.AutoSize = true;
            this.queryLabel.Location = new System.Drawing.Point(11, 157);
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
            this.kvrCheck.Location = new System.Drawing.Point(15, 44);
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
            this.pmrCheck.Location = new System.Drawing.Point(15, 79);
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
            this.toresCheck.Location = new System.Drawing.Point(15, 114);
            this.toresCheck.Name = "toresCheck";
            this.toresCheck.Size = new System.Drawing.Size(90, 28);
            this.toresCheck.TabIndex = 13;
            this.toresCheck.Text = "Tor E/S";
            this.toresCheck.UseVisualStyleBackColor = true;
            // 
            // stationsList
            // 
            this.stationsList.FormattingEnabled = true;
            this.stationsList.Location = new System.Drawing.Point(105, 41);
            this.stationsList.Name = "stationsList";
            this.stationsList.Size = new System.Drawing.Size(168, 32);
            this.stationsList.TabIndex = 15;
            this.stationsList.SelectionChangeCommitted += new System.EventHandler(this.DeviceListOnSelectedChanged);
            // 
            // deviceList
            // 
            this.deviceList.FormattingEnabled = true;
            this.deviceList.Location = new System.Drawing.Point(105, 79);
            this.deviceList.Name = "deviceList";
            this.deviceList.Size = new System.Drawing.Size(168, 32);
            this.deviceList.TabIndex = 16;
            // 
            // taskTimeTimer
            // 
            this.taskTimeTimer.Interval = 1;
            // 
            // runQueryButton
            // 
            this.runQueryButton.Location = new System.Drawing.Point(376, 144);
            this.runQueryButton.Name = "runQueryButton";
            this.runQueryButton.Size = new System.Drawing.Size(171, 37);
            this.runQueryButton.TabIndex = 17;
            this.runQueryButton.Text = "Ejecutar con&sulta";
            this.runQueryButton.UseVisualStyleBackColor = true;
            this.runQueryButton.Click += new System.EventHandler(this.RunQueryInDeviceOnClick);
            // 
            // checkReplicaButton
            // 
            this.checkReplicaButton.Location = new System.Drawing.Point(376, 36);
            this.checkReplicaButton.Name = "checkReplicaButton";
            this.checkReplicaButton.Size = new System.Drawing.Size(171, 34);
            this.checkReplicaButton.TabIndex = 18;
            this.checkReplicaButton.Text = "Verificar &réplica";
            this.checkReplicaButton.UseVisualStyleBackColor = true;
            this.checkReplicaButton.Click += new System.EventHandler(this.CheckReplicaButtonOnClick);
            // 
            // stopTaskButton
            // 
            this.stopTaskButton.Location = new System.Drawing.Point(376, 79);
            this.stopTaskButton.Name = "stopTaskButton";
            this.stopTaskButton.Size = new System.Drawing.Size(171, 34);
            this.stopTaskButton.TabIndex = 19;
            this.stopTaskButton.Text = "&Detener tarea";
            this.stopTaskButton.UseVisualStyleBackColor = true;
            this.stopTaskButton.Click += new System.EventHandler(this.StopTaskButtonOnClick);
            // 
            // SQLModule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1182, 553);
            this.Controls.Add(this.stopTaskButton);
            this.Controls.Add(this.checkReplicaButton);
            this.Controls.Add(this.runQueryButton);
            this.Controls.Add(this.deviceList);
            this.Controls.Add(this.stationsList);
            this.Controls.Add(this.toresCheck);
            this.Controls.Add(this.pmrCheck);
            this.Controls.Add(this.kvrCheck);
            this.Controls.Add(this.queryLabel);
            this.Controls.Add(this.queryBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusBarra);
            this.Controls.Add(this.resultTable);
            this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SQLModule";
            this.Text = "SQL Masivo";
            ((System.ComponentModel.ISupportInitialize)(this.resultTable)).EndInit();
            this.statusBarra.ResumeLayout(false);
            this.statusBarra.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView resultTable;
        private System.Windows.Forms.StatusStrip statusBarra;
        private System.Windows.Forms.ToolStripStatusLabel currentTaskLabel;
        private System.Windows.Forms.ToolStripProgressBar taskProgressBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox queryBox;
        private System.Windows.Forms.Label queryLabel;
        private System.Windows.Forms.CheckBox kvrCheck;
        private System.Windows.Forms.CheckBox pmrCheck;
        private System.Windows.Forms.CheckBox toresCheck;
        private System.Windows.Forms.ComboBox stationsList;
        private System.Windows.Forms.ComboBox deviceList;
        private System.Windows.Forms.ToolStripStatusLabel progressLabel;
        private System.Windows.Forms.ToolStripStatusLabel execTimeLabel;
        private System.Windows.Forms.Timer taskTimeTimer;
        private System.Windows.Forms.ToolStripStatusLabel threadsStatusLabel;
        private System.Windows.Forms.Button runQueryButton;
        private System.Windows.Forms.Button checkReplicaButton;
        private System.Windows.Forms.Button stopTaskButton;
    }
}