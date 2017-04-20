namespace ACABUS_Control_de_operacion
{
    partial class UncirculatedVehicles
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
            this.saveButton = new System.Windows.Forms.Button();
            this.clearButton = new System.Windows.Forms.Button();
            this.uncirculatedTable = new System.Windows.Forms.DataGridView();
            this.noEconColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.refreshButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.uncirculatedTable)).BeginInit();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(418, 57);
            this.saveButton.Margin = new System.Windows.Forms.Padding(4);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(118, 37);
            this.saveButton.TabIndex = 18;
            this.saveButton.Text = "&Guardar";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(418, 12);
            this.clearButton.Margin = new System.Windows.Forms.Padding(4);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(118, 37);
            this.clearButton.TabIndex = 17;
            this.clearButton.Text = "&Vaciar";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // uncirculatedTable
            // 
            this.uncirculatedTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.uncirculatedTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.noEconColumn,
            this.statusColumn});
            this.uncirculatedTable.Location = new System.Drawing.Point(12, 13);
            this.uncirculatedTable.Name = "uncirculatedTable";
            this.uncirculatedTable.RowTemplate.Height = 24;
            this.uncirculatedTable.Size = new System.Drawing.Size(396, 470);
            this.uncirculatedTable.TabIndex = 19;
            this.uncirculatedTable.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.UncirculatedVehiOnCellValueChanged);
            // 
            // noEconColumn
            // 
            this.noEconColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.noEconColumn.HeaderText = "No. Económico";
            this.noEconColumn.Name = "noEconColumn";
            this.noEconColumn.Width = 164;
            // 
            // statusColumn
            // 
            this.statusColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.statusColumn.HeaderText = "Estado";
            this.statusColumn.Items.AddRange(new object[] {
            "TALLER",
            "SIN ENERGÍA"});
            this.statusColumn.Name = "statusColumn";
            this.statusColumn.Width = 73;
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(418, 102);
            this.refreshButton.Margin = new System.Windows.Forms.Padding(4);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(118, 37);
            this.refreshButton.TabIndex = 20;
            this.refreshButton.Text = "&Actualizar";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.RefreshButtonOnClick);
            // 
            // UncirculatedVehicles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 495);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.uncirculatedTable);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.clearButton);
            this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UncirculatedVehicles";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Vehículos sin circular";
            ((System.ComponentModel.ISupportInitialize)(this.uncirculatedTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.DataGridView uncirculatedTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn noEconColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn statusColumn;
        private System.Windows.Forms.Button refreshButton;
    }
}