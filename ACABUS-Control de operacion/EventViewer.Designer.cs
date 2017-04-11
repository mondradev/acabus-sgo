namespace ACABUS_Control_de_operacion
{
    partial class EventViewer
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.debugCheck = new System.Windows.Forms.CheckBox();
            this.errorCheck = new System.Windows.Forms.CheckBox();
            this.infoCheck = new System.Windows.Forms.CheckBox();
            this.eventTable = new System.Windows.Forms.DataGridView();
            this.dateTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeEventColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.messageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventTable)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.eventTable, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.21127F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 89.78873F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1080, 568);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.debugCheck);
            this.groupBox1.Controls.Add(this.errorCheck);
            this.groupBox1.Controls.Add(this.infoCheck);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1074, 52);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Opciones de eventos";
            // 
            // debugCheck
            // 
            this.debugCheck.AutoSize = true;
            this.debugCheck.Location = new System.Drawing.Point(224, 21);
            this.debugCheck.Name = "debugCheck";
            this.debugCheck.Size = new System.Drawing.Size(103, 21);
            this.debugCheck.TabIndex = 2;
            this.debugCheck.Text = "Depuración";
            this.debugCheck.UseVisualStyleBackColor = true;
            this.debugCheck.CheckedChanged += new System.EventHandler(this.InfoCheck_CheckedChanged);
            // 
            // errorCheck
            // 
            this.errorCheck.AutoSize = true;
            this.errorCheck.Checked = true;
            this.errorCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.errorCheck.Location = new System.Drawing.Point(115, 21);
            this.errorCheck.Name = "errorCheck";
            this.errorCheck.Size = new System.Drawing.Size(77, 21);
            this.errorCheck.TabIndex = 1;
            this.errorCheck.Text = "Errores";
            this.errorCheck.UseVisualStyleBackColor = true;
            this.errorCheck.CheckedChanged += new System.EventHandler(this.InfoCheck_CheckedChanged);
            // 
            // infoCheck
            // 
            this.infoCheck.AutoSize = true;
            this.infoCheck.Checked = true;
            this.infoCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.infoCheck.Location = new System.Drawing.Point(6, 21);
            this.infoCheck.Name = "infoCheck";
            this.infoCheck.Size = new System.Drawing.Size(103, 21);
            this.infoCheck.TabIndex = 0;
            this.infoCheck.Text = "Información";
            this.infoCheck.UseVisualStyleBackColor = true;
            this.infoCheck.CheckedChanged += new System.EventHandler(this.InfoCheck_CheckedChanged);
            // 
            // dataGridView1
            // 
            this.eventTable.AllowUserToAddRows = false;
            this.eventTable.AllowUserToDeleteRows = false;
            this.eventTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.eventTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.eventTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dateTimeColumn,
            this.typeEventColumn,
            this.messageColumn});
            this.eventTable.Location = new System.Drawing.Point(3, 61);
            this.eventTable.Name = "dataGridView1";
            this.eventTable.ReadOnly = true;
            this.eventTable.RowHeadersVisible = false;
            this.eventTable.RowTemplate.Height = 24;
            this.eventTable.Size = new System.Drawing.Size(1074, 504);
            this.eventTable.TabIndex = 1;
            // 
            // dateTimeColumn
            // 
            this.dateTimeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dateTimeColumn.HeaderText = "Fecha/Hora";
            this.dateTimeColumn.Name = "dateTimeColumn";
            this.dateTimeColumn.ReadOnly = true;
            this.dateTimeColumn.Width = 111;
            // 
            // typeEventColumn
            // 
            this.typeEventColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.typeEventColumn.HeaderText = "Tipo";
            this.typeEventColumn.Name = "typeEventColumn";
            this.typeEventColumn.ReadOnly = true;
            this.typeEventColumn.Width = 65;
            // 
            // messageColumn
            // 
            this.messageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.messageColumn.HeaderText = "Mensaje";
            this.messageColumn.Name = "messageColumn";
            this.messageColumn.ReadOnly = true;
            this.messageColumn.Width = 90;
            // 
            // EventViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1104, 592);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "EventViewer";
            this.Text = "Visor de eventos";
            this.Load += new System.EventHandler(this.EventViewer_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox debugCheck;
        private System.Windows.Forms.CheckBox errorCheck;
        private System.Windows.Forms.CheckBox infoCheck;
        private System.Windows.Forms.DataGridView eventTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn dateTimeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn typeEventColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn messageColumn;
    }
}