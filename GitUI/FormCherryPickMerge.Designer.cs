namespace GitUI
{
    partial class FormCherryPickMerge
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCherryPickMerge));
            this.OK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ParentsList = new System.Windows.Forms.ListView();
            this.No = new System.Windows.Forms.ColumnHeader();
            this.Message = new System.Windows.Forms.ColumnHeader();
            this.Author = new System.Windows.Forms.ColumnHeader();
            this.CommitDate = new System.Windows.Forms.ColumnHeader();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SuspendLayout();
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(242, 104);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 25);
            this.OK.TabIndex = 1;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(233, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "This commit is a merge, please select the parent";
            // 
            // ParentsList
            // 
            this.ParentsList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ParentsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.No,
            this.Message,
            this.Author,
            this.CommitDate});
            this.ParentsList.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.ParentsList.FullRowSelect = true;
            this.ParentsList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ParentsList.HideSelection = false;
            this.ParentsList.Location = new System.Drawing.Point(4, 27);
            this.ParentsList.Margin = new System.Windows.Forms.Padding(5);
            this.ParentsList.MultiSelect = false;
            this.ParentsList.Name = "ParentsList";
            this.ParentsList.Size = new System.Drawing.Size(550, 69);
            this.ParentsList.TabIndex = 3;
            this.ParentsList.TileSize = new System.Drawing.Size(168, 50);
            this.ParentsList.UseCompatibleStateImageBehavior = false;
            this.ParentsList.View = System.Windows.Forms.View.Details;
            // 
            // No
            // 
            this.No.Text = "No.";
            this.No.Width = 36;
            // 
            // Message
            // 
            this.Message.Tag = "";
            this.Message.Text = "Message";
            this.Message.Width = 293;
            // 
            // Author
            // 
            this.Author.Text = "Author";
            this.Author.Width = 120;
            // 
            // CommitDate
            // 
            this.CommitDate.Text = "Date";
            this.CommitDate.Width = 80;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "No.";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 30;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Message";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 320;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Author";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 110;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Date";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 80;
            // 
            // FormCherryPickMerge
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 132);
            this.Controls.Add(this.ParentsList);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormCherryPickMerge";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose parent";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader Message;
        private System.Windows.Forms.ColumnHeader Author;
        private System.Windows.Forms.ColumnHeader CommitDate;
        private System.Windows.Forms.ColumnHeader No;
        public System.Windows.Forms.ListView ParentsList;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
    }
}