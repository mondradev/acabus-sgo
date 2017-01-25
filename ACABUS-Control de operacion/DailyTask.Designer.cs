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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DailyTask));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.pendingButton = new System.Windows.Forms.ToolStripButton();
            this.checkReplicaButton = new System.Windows.Forms.ToolStripButton();
            this.restartReplicaButton = new System.Windows.Forms.ToolStripButton();
            this.resultTable = new System.Windows.Forms.DataGridView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.currentTaskLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.taskProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.valTable = new System.Windows.Forms.DataGridView();
            this.NoEcon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ValPending = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.badCountersTable = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.deathCounterButton = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resultTable)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.valTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.badCountersTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.deathCounterButton)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pendingButton,
            this.checkReplicaButton,
            this.restartReplicaButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(831, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // pendingButton
            // 
            this.pendingButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.pendingButton.Image = ((System.Drawing.Image)(resources.GetObject("pendingButton.Image")));
            this.pendingButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pendingButton.Name = "pendingButton";
            this.pendingButton.Size = new System.Drawing.Size(69, 22);
            this.pendingButton.Text = "Pendientes";
            // 
            // checkReplicaButton
            // 
            this.checkReplicaButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.checkReplicaButton.Image = ((System.Drawing.Image)(resources.GetObject("checkReplicaButton.Image")));
            this.checkReplicaButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.checkReplicaButton.Name = "checkReplicaButton";
            this.checkReplicaButton.Size = new System.Drawing.Size(94, 22);
            this.checkReplicaButton.Text = "Verificar &Réplica";
            this.checkReplicaButton.Click += new System.EventHandler(this.checkReplicaButton_Click);
            // 
            // restartReplicaButton
            // 
            this.restartReplicaButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.restartReplicaButton.Image = ((System.Drawing.Image)(resources.GetObject("restartReplicaButton.Image")));
            this.restartReplicaButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.restartReplicaButton.Name = "restartReplicaButton";
            this.restartReplicaButton.Size = new System.Drawing.Size(97, 22);
            this.restartReplicaButton.Text = "R&einiciar Réplica";
            this.restartReplicaButton.Click += new System.EventHandler(this.restartReplicaButton_Click);
            // 
            // resultTable
            // 
            this.resultTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.resultTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.resultTable.Location = new System.Drawing.Point(16, 368);
            this.resultTable.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.resultTable.Name = "resultTable";
            this.resultTable.Size = new System.Drawing.Size(799, 222);
            this.resultTable.TabIndex = 1;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.currentTaskLabel,
            this.taskProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 598);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(831, 29);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // currentTaskLabel
            // 
            this.currentTaskLabel.Name = "currentTaskLabel";
            this.currentTaskLabel.Size = new System.Drawing.Size(122, 24);
            this.currentTaskLabel.Text = "Tarea actual: Ninguna";
            // 
            // taskProgressBar
            // 
            this.taskProgressBar.Name = "taskProgressBar";
            this.taskProgressBar.Size = new System.Drawing.Size(400, 23);
            // 
            // valTable
            // 
            this.valTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.valTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NoEcon,
            this.ValPending});
            this.valTable.Location = new System.Drawing.Point(16, 76);
            this.valTable.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.valTable.Name = "valTable";
            this.valTable.Size = new System.Drawing.Size(343, 253);
            this.valTable.TabIndex = 3;
            // 
            // NoEcon
            // 
            this.NoEcon.HeaderText = "No. Económico";
            this.NoEcon.Name = "NoEcon";
            this.NoEcon.ReadOnly = true;
            // 
            // ValPending
            // 
            this.ValPending.HeaderText = "Días de validaciones pendientes";
            this.ValPending.Name = "ValPending";
            this.ValPending.ReadOnly = true;
            // 
            // badCountersTable
            // 
            this.badCountersTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.badCountersTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1});
            this.badCountersTable.Location = new System.Drawing.Point(367, 76);
            this.badCountersTable.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.badCountersTable.Name = "badCountersTable";
            this.badCountersTable.Size = new System.Drawing.Size(211, 253);
            this.badCountersTable.TabIndex = 4;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "No. Económico";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // deathCounterButton
            // 
            this.deathCounterButton.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.deathCounterButton.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn2});
            this.deathCounterButton.Location = new System.Drawing.Point(585, 76);
            this.deathCounterButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.deathCounterButton.Name = "deathCounterButton";
            this.deathCounterButton.Size = new System.Drawing.Size(229, 253);
            this.deathCounterButton.TabIndex = 5;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "No. Económico";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 339);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(167, 19);
            this.label1.TabIndex = 6;
            this.label1.Text = "Resultados de consultas";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 53);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(167, 19);
            this.label2.TabIndex = 7;
            this.label2.Text = "Validaciones pendientes";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(363, 53);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(142, 19);
            this.label3.TabIndex = 8;
            this.label3.Text = "Contadores en ceros";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(581, 53);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(241, 19);
            this.label4.TabIndex = 9;
            this.label4.Text = "Contadores menor que validaciones";
            // 
            // DailyTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(831, 627);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.deathCounterButton);
            this.Controls.Add(this.badCountersTable);
            this.Controls.Add(this.valTable);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.resultTable);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "DailyTask";
            this.Text = "DailyTask";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resultTable)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.valTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.badCountersTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.deathCounterButton)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton checkReplicaButton;
        private System.Windows.Forms.DataGridView resultTable;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel currentTaskLabel;
        private System.Windows.Forms.ToolStripProgressBar taskProgressBar;
        private System.Windows.Forms.ToolStripButton restartReplicaButton;
        private System.Windows.Forms.ToolStripButton pendingButton;
        private System.Windows.Forms.DataGridView valTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn NoEcon;
        private System.Windows.Forms.DataGridViewTextBoxColumn ValPending;
        private System.Windows.Forms.DataGridView badCountersTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridView deathCounterButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}