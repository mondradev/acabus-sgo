namespace ACABUS_Control_de_operacion
{
    partial class StockCard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StockCard));
            this.dgvResult = new System.Windows.Forms.DataGridView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbExecute = new System.Windows.Forms.ToolStripButton();
            this.stopTaskButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbConfig = new System.Windows.Forms.ToolStripButton();
            this.statusBarra = new System.Windows.Forms.StatusStrip();
            this.taskProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.progressLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.execTimeLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.threadsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.taskTimeTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResult)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.statusBarra.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvResult
            // 
            this.dgvResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResult.Location = new System.Drawing.Point(0, 29);
            this.dgvResult.Margin = new System.Windows.Forms.Padding(4);
            this.dgvResult.Name = "dgvResult";
            this.dgvResult.Size = new System.Drawing.Size(1089, 599);
            this.dgvResult.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbExecute,
            this.stopTaskButton,
            this.toolStripSeparator1,
            this.tsbConfig});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1089, 27);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbExecute
            // 
            this.tsbExecute.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbExecute.Image = ((System.Drawing.Image)(resources.GetObject("tsbExecute.Image")));
            this.tsbExecute.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbExecute.Name = "tsbExecute";
            this.tsbExecute.Size = new System.Drawing.Size(24, 24);
            this.tsbExecute.Text = "toolStripButton1";
            this.tsbExecute.Click += new System.EventHandler(this.TsbExecute_Click);
            // 
            // stopTaskButton
            // 
            this.stopTaskButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stopTaskButton.Enabled = false;
            this.stopTaskButton.Image = ((System.Drawing.Image)(resources.GetObject("stopTaskButton.Image")));
            this.stopTaskButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stopTaskButton.Name = "stopTaskButton";
            this.stopTaskButton.Size = new System.Drawing.Size(24, 24);
            this.stopTaskButton.Text = "Detener";
            this.stopTaskButton.Click += new System.EventHandler(this.StopTaskButtonOnClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbConfig
            // 
            this.tsbConfig.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbConfig.Image = ((System.Drawing.Image)(resources.GetObject("tsbConfig.Image")));
            this.tsbConfig.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbConfig.Name = "tsbConfig";
            this.tsbConfig.Size = new System.Drawing.Size(24, 24);
            this.tsbConfig.Text = "toolStripButton1";
            // 
            // statusBarra
            // 
            this.statusBarra.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusBarra.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.taskProgressBar,
            this.progressLabel,
            this.execTimeLabel,
            this.threadsStatusLabel});
            this.statusBarra.Location = new System.Drawing.Point(0, 632);
            this.statusBarra.Name = "statusBarra";
            this.statusBarra.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusBarra.Size = new System.Drawing.Size(1089, 25);
            this.statusBarra.TabIndex = 3;
            this.statusBarra.Text = "Barra de estado";
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
            // taskTimeTimer
            // 
            this.taskTimeTimer.Interval = 1;
            // 
            // StockCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1089, 657);
            this.Controls.Add(this.statusBarra);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.dgvResult);
            this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "StockCard";
            this.Text = "StockCard";
            ((System.ComponentModel.ISupportInitialize)(this.dgvResult)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusBarra.ResumeLayout(false);
            this.statusBarra.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvResult;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbExecute;
        private System.Windows.Forms.ToolStripButton stopTaskButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbConfig;
        private System.Windows.Forms.StatusStrip statusBarra;
        private System.Windows.Forms.ToolStripProgressBar taskProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel progressLabel;
        private System.Windows.Forms.ToolStripStatusLabel execTimeLabel;
        private System.Windows.Forms.ToolStripStatusLabel threadsStatusLabel;
        private System.Windows.Forms.Timer taskTimeTimer;
    }
}