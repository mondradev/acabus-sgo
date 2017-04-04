namespace ACABUS_Control_de_operacion
{
    partial class MainForm
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmifSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.visorDeEventosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.conexiónBDKVRExternosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmifExit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.tlsTools = new System.Windows.Forms.ToolStrip();
            this.stockCardButton = new System.Windows.Forms.ToolStripButton();
            this.sqlButton = new System.Windows.Forms.ToolStripButton();
            this.monitorButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            this.tlsTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiFile,
            this.tsmiEdit});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1282, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsmiFile
            // 
            this.tsmiFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmifSeparator,
            this.visorDeEventosToolStripMenuItem,
            this.conexiónBDKVRExternosToolStripMenuItem,
            this.tsmifExit});
            this.tsmiFile.Name = "tsmiFile";
            this.tsmiFile.Size = new System.Drawing.Size(84, 24);
            this.tsmiFile.Text = "ARCHIVO";
            // 
            // tsmifSeparator
            // 
            this.tsmifSeparator.Name = "tsmifSeparator";
            this.tsmifSeparator.Size = new System.Drawing.Size(258, 6);
            // 
            // visorDeEventosToolStripMenuItem
            // 
            this.visorDeEventosToolStripMenuItem.Name = "visorDeEventosToolStripMenuItem";
            this.visorDeEventosToolStripMenuItem.Size = new System.Drawing.Size(261, 26);
            this.visorDeEventosToolStripMenuItem.Text = "Visor de eventos";
            this.visorDeEventosToolStripMenuItem.Click += new System.EventHandler(this.VisorDeEventosToolStripMenuItem_Click);
            // 
            // conexiónBDKVRExternosToolStripMenuItem
            // 
            this.conexiónBDKVRExternosToolStripMenuItem.Name = "conexiónBDKVRExternosToolStripMenuItem";
            this.conexiónBDKVRExternosToolStripMenuItem.Size = new System.Drawing.Size(261, 26);
            this.conexiónBDKVRExternosToolStripMenuItem.Text = "Conexión BD KVR externos";
            this.conexiónBDKVRExternosToolStripMenuItem.Click += new System.EventHandler(this.ConexiónBDKVRExternosToolStripMenuItem_Click);
            // 
            // tsmifExit
            // 
            this.tsmifExit.Image = global::ACABUS_Control_de_operacion.Properties.Resources.Actions_close_icon;
            this.tsmifExit.Name = "tsmifExit";
            this.tsmifExit.Size = new System.Drawing.Size(261, 26);
            this.tsmifExit.Text = "Salir";
            this.tsmifExit.Click += new System.EventHandler(this.mbrArcExi_Click);
            // 
            // tsmiEdit
            // 
            this.tsmiEdit.Name = "tsmiEdit";
            this.tsmiEdit.Size = new System.Drawing.Size(70, 24);
            this.tsmiEdit.Text = "EDITAR";
            // 
            // tlsTools
            // 
            this.tlsTools.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.tlsTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stockCardButton,
            this.sqlButton,
            this.monitorButton,
            this.toolStripButton1});
            this.tlsTools.Location = new System.Drawing.Point(0, 28);
            this.tlsTools.Name = "tlsTools";
            this.tlsTools.Size = new System.Drawing.Size(1282, 27);
            this.tlsTools.TabIndex = 2;
            this.tlsTools.Text = "toolStrip1";
            // 
            // stockCardButton
            // 
            this.stockCardButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stockCardButton.Image = ((System.Drawing.Image)(resources.GetObject("stockCardButton.Image")));
            this.stockCardButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stockCardButton.Name = "stockCardButton";
            this.stockCardButton.Size = new System.Drawing.Size(24, 24);
            this.stockCardButton.Text = "Inventario de tarjetas en KVR";
            this.stockCardButton.Click += new System.EventHandler(this.ToolStripButton1_Click);
            // 
            // sqlButton
            // 
            this.sqlButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.sqlButton.Image = ((System.Drawing.Image)(resources.GetObject("sqlButton.Image")));
            this.sqlButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.sqlButton.Name = "sqlButton";
            this.sqlButton.Size = new System.Drawing.Size(24, 24);
            this.sqlButton.Text = "SQL y Réplica";
            this.sqlButton.Click += new System.EventHandler(this.DailyTaskButton_Click);
            // 
            // monitorButton
            // 
            this.monitorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.monitorButton.Image = ((System.Drawing.Image)(resources.GetObject("monitorButton.Image")));
            this.monitorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.monitorButton.Name = "monitorButton";
            this.monitorButton.Size = new System.Drawing.Size(24, 24);
            this.monitorButton.Text = "Monitoreo";
            this.monitorButton.Click += new System.EventHandler(this.MonitorButton_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton1.Text = "Monitor de la red";
            this.toolStripButton1.Click += new System.EventHandler(this.MonitorLanButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1282, 653);
            this.Controls.Add(this.tlsTools);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(1300, 700);
            this.Name = "MainForm";
            this.Text = "Control de operación ACABUS";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tlsTools.ResumeLayout(false);
            this.tlsTools.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiFile;
        private System.Windows.Forms.ToolStripMenuItem tsmifExit;
        private System.Windows.Forms.ToolStrip tlsTools;
        private System.Windows.Forms.ToolStripButton stockCardButton;
        private System.Windows.Forms.ToolStripMenuItem tsmiEdit;
        private System.Windows.Forms.ToolStripSeparator tsmifSeparator;
        private System.Windows.Forms.ToolStripButton sqlButton;
        private System.Windows.Forms.ToolStripButton monitorButton;
        private System.Windows.Forms.ToolStripMenuItem visorDeEventosToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem conexiónBDKVRExternosToolStripMenuItem;
    }
}

