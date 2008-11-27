namespace FileHashShell
{
    partial class ShellForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.fileSizeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.hashDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pathDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileInformationBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.bgWorker = new System.ComponentModel.BackgroundWorker();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblFileName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileInformationBindingSource)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fileSizeDataGridViewTextBoxColumn,
            this.nameDataGridViewTextBoxColumn,
            this.hashDataGridViewTextBoxColumn,
            this.fileDateDataGridViewTextBoxColumn,
            this.pathDataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.fileInformationBindingSource;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(679, 64);
            this.dataGridView1.TabIndex = 0;
            // 
            // fileSizeDataGridViewTextBoxColumn
            // 
            this.fileSizeDataGridViewTextBoxColumn.DataPropertyName = "FileSize";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle1.Format = "###,###,### \"KB\"";
            dataGridViewCellStyle1.NullValue = null;
            this.fileSizeDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.fileSizeDataGridViewTextBoxColumn.HeaderText = "File Size";
            this.fileSizeDataGridViewTextBoxColumn.Name = "fileSizeDataGridViewTextBoxColumn";
            this.fileSizeDataGridViewTextBoxColumn.Width = 71;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.Width = 189;
            // 
            // hashDataGridViewTextBoxColumn
            // 
            this.hashDataGridViewTextBoxColumn.DataPropertyName = "Hash";
            this.hashDataGridViewTextBoxColumn.HeaderText = "Hash";
            this.hashDataGridViewTextBoxColumn.Name = "hashDataGridViewTextBoxColumn";
            this.hashDataGridViewTextBoxColumn.Width = 235;
            // 
            // fileDateDataGridViewTextBoxColumn
            // 
            this.fileDateDataGridViewTextBoxColumn.DataPropertyName = "FileDate";
            dataGridViewCellStyle2.Format = "yyyy-MM-dd HH:mm:ss zzzz";
            this.fileDateDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.fileDateDataGridViewTextBoxColumn.HeaderText = "File Date";
            this.fileDateDataGridViewTextBoxColumn.Name = "fileDateDataGridViewTextBoxColumn";
            this.fileDateDataGridViewTextBoxColumn.Width = 120;
            // 
            // pathDataGridViewTextBoxColumn
            // 
            this.pathDataGridViewTextBoxColumn.DataPropertyName = "Path";
            this.pathDataGridViewTextBoxColumn.HeaderText = "Path";
            this.pathDataGridViewTextBoxColumn.Name = "pathDataGridViewTextBoxColumn";
            this.pathDataGridViewTextBoxColumn.Width = 102;
            // 
            // fileInformationBindingSource
            // 
            this.fileInformationBindingSource.DataSource = typeof(FileHashShell.FileInformation);
            // 
            // bgWorker
            // 
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.WorkerSupportsCancellation = true;
            this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
            this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblFileName);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(679, 64);
            this.panel1.TabIndex = 1;
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName.Location = new System.Drawing.Point(248, 21);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(0, 20);
            this.lblFileName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(74, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(174, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Calculating hash for ";
            // 
            // ShellForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(679, 64);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.dataGridView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.KeyPreview = true;
            this.Name = "ShellForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "SHA-1 File Hash";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShellForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ShellForm_KeyDown);
            this.Load += new System.EventHandler(this.ShellForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileInformationBindingSource)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.ComponentModel.BackgroundWorker bgWorker;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.BindingSource fileInformationBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn hashDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileSizeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileDateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathDataGridViewTextBoxColumn;
    }
}

