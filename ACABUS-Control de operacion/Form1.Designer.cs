namespace ACABUS_Control_de_operacion
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmifSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tsmifExit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.tlsTools = new System.Windows.Forms.ToolStrip();
            this.tsbStockCard = new System.Windows.Forms.ToolStripButton();
            this.dailyTaskButton = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            this.tlsTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiFile,
            this.tsmiEdit});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(786, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsmiFile
            // 
            this.tsmiFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmifSeparator,
            this.tsmifExit});
            this.tsmiFile.Name = "tsmiFile";
            this.tsmiFile.Size = new System.Drawing.Size(70, 20);
            this.tsmiFile.Text = "ARCHIVO";
            // 
            // tsmifSeparator
            // 
            this.tsmifSeparator.Name = "tsmifSeparator";
            this.tsmifSeparator.Size = new System.Drawing.Size(93, 6);
            // 
            // tsmifExit
            // 
            this.tsmifExit.Image = global::ACABUS_Control_de_operacion.Properties.Resources.Actions_close_icon;
            this.tsmifExit.Name = "tsmifExit";
            this.tsmifExit.Size = new System.Drawing.Size(96, 22);
            this.tsmifExit.Text = "Salir";
            this.tsmifExit.Click += new System.EventHandler(this.mbrArcExi_Click);
            // 
            // tsmiEdit
            // 
            this.tsmiEdit.Name = "tsmiEdit";
            this.tsmiEdit.Size = new System.Drawing.Size(57, 20);
            this.tsmiEdit.Text = "EDITAR";
            // 
            // tlsTools
            // 
            this.tlsTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbStockCard,
            this.dailyTaskButton});
            this.tlsTools.Location = new System.Drawing.Point(0, 24);
            this.tlsTools.Name = "tlsTools";
            this.tlsTools.Size = new System.Drawing.Size(786, 25);
            this.tlsTools.TabIndex = 2;
            this.tlsTools.Text = "toolStrip1";
            // 
            // tsbStockCard
            // 
            this.tsbStockCard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbStockCard.Image = ((System.Drawing.Image)(resources.GetObject("tsbStockCard.Image")));
            this.tsbStockCard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbStockCard.Name = "tsbStockCard";
            this.tsbStockCard.Size = new System.Drawing.Size(23, 22);
            this.tsbStockCard.Text = "Stock de tarjetas";
            this.tsbStockCard.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // dailyTaskButton
            // 
            this.dailyTaskButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.dailyTaskButton.Image = ((System.Drawing.Image)(resources.GetObject("dailyTaskButton.Image")));
            this.dailyTaskButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.dailyTaskButton.Name = "dailyTaskButton";
            this.dailyTaskButton.Size = new System.Drawing.Size(23, 22);
            this.dailyTaskButton.Text = "Tareas diarias";
            this.dailyTaskButton.Click += new System.EventHandler(this.dailyTaskButton_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(786, 461);
            this.Controls.Add(this.tlsTools);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.Text = "Control de operación - ACABUS";
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
        private System.Windows.Forms.ToolStripButton tsbStockCard;
        private System.Windows.Forms.ToolStripMenuItem tsmiEdit;
        private System.Windows.Forms.ToolStripSeparator tsmifSeparator;
        private System.Windows.Forms.ToolStripButton dailyTaskButton;
    }
}

