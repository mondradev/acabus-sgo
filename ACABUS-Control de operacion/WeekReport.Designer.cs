namespace ACABUS_Control_de_operacion
{
    partial class WeekReport
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
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.button1 = new System.Windows.Forms.Button();
            this.generateBtn = new System.Windows.Forms.Button();
            this.cantTxt = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(12, 12);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(360, 32);
            this.dateTimePicker1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(378, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(155, 32);
            this.button1.TabIndex = 1;
            this.button1.Text = "Generar reporte";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // generateBtn
            // 
            this.generateBtn.Location = new System.Drawing.Point(118, 50);
            this.generateBtn.Name = "generateBtn";
            this.generateBtn.Size = new System.Drawing.Size(254, 32);
            this.generateBtn.TabIndex = 3;
            this.generateBtn.Text = "Generar multi-reportes";
            this.generateBtn.UseVisualStyleBackColor = true;
            this.generateBtn.Click += new System.EventHandler(this.GenerateBtn_Click);
            // 
            // cantTxt
            // 
            this.cantTxt.Location = new System.Drawing.Point(12, 51);
            this.cantTxt.Name = "cantTxt";
            this.cantTxt.Size = new System.Drawing.Size(100, 32);
            this.cantTxt.TabIndex = 4;
            this.cantTxt.Text = "1";
            // 
            // WeekReport
            // 
            this.ClientSize = new System.Drawing.Size(956, 614);
            this.Controls.Add(this.cantTxt);
            this.Controls.Add(this.generateBtn);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dateTimePicker1);
            this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "WeekReport";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button generateBtn;
        private System.Windows.Forms.TextBox cantTxt;
    }
}
