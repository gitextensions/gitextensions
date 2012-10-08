namespace GitUI
{
    partial class FormCherryPick
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
            this.label1 = new System.Windows.Forms.Label();
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.checkAddReference = new System.Windows.Forms.CheckBox();
            this.autoParent = new System.Windows.Forms.CheckBox();
            this.AutoCommit = new System.Windows.Forms.CheckBox();
            this.CherryPick = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(544, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select a commit you want to cherry pick. The commit will be recommitted on top of" +
    " the current head.";
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Location = new System.Drawing.Point(3, 28);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.RevisionGraphDrawStyle = GitUI.DvcsGraph.RevisionGraphDrawStyleEnum.DrawNonRelativesGray;
            this.RevisionGrid.Size = new System.Drawing.Size(632, 266);
            this.RevisionGrid.TabIndex = 0;
            // 
            // checkAddReference
            // 
            this.checkAddReference.AutoSize = true;
            this.checkAddReference.Location = new System.Drawing.Point(3, 53);
            this.checkAddReference.Name = "checkAddReference";
            this.checkAddReference.Size = new System.Drawing.Size(145, 19);
            this.checkAddReference.TabIndex = 3;
            this.checkAddReference.Text = "Add commit reference";
            this.checkAddReference.UseVisualStyleBackColor = true;
            // 
            // autoParent
            // 
            this.autoParent.AutoSize = true;
            this.autoParent.Checked = true;
            this.autoParent.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoParent.Location = new System.Drawing.Point(3, 28);
            this.autoParent.Name = "autoParent";
            this.autoParent.Size = new System.Drawing.Size(312, 19);
            this.autoParent.TabIndex = 2;
            this.autoParent.Text = "Automatically set parent to 1 when commit is a merge";
            this.autoParent.UseVisualStyleBackColor = true;
            // 
            // AutoCommit
            // 
            this.AutoCommit.AutoSize = true;
            this.AutoCommit.Checked = true;
            this.AutoCommit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoCommit.Location = new System.Drawing.Point(3, 3);
            this.AutoCommit.Name = "AutoCommit";
            this.AutoCommit.Size = new System.Drawing.Size(375, 19);
            this.AutoCommit.TabIndex = 1;
            this.AutoCommit.Text = "Automatically create a commit  when there are no merge conflicts";
            this.AutoCommit.UseVisualStyleBackColor = true;
            // 
            // CherryPick
            // 
            this.CherryPick.Location = new System.Drawing.Point(519, 53);
            this.CherryPick.Name = "CherryPick";
            this.CherryPick.Size = new System.Drawing.Size(110, 25);
            this.CherryPick.TabIndex = 0;
            this.CherryPick.Text = "Cherry pick";
            this.CherryPick.UseVisualStyleBackColor = true;
            this.CherryPick.Click += new System.EventHandler(this.CherryPick_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.RevisionGrid, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(638, 387);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.CherryPick, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.autoParent, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.checkAddReference, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.AutoCommit, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 300);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(632, 84);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // FormCherryPick
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 387);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(400, 200);
            this.Name = "FormCherryPick";
            this.Text = "Cherry pick";
            this.Load += new System.EventHandler(this.FormCherryPick_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private RevisionGrid RevisionGrid;
        private System.Windows.Forms.Button CherryPick;
        private System.Windows.Forms.CheckBox AutoCommit;
        private System.Windows.Forms.CheckBox autoParent;
        private System.Windows.Forms.CheckBox checkAddReference;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}