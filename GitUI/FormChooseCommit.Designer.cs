namespace GitUI
{
    partial class FormChooseCommit
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
            this.revisionGrid = new GitUI.RevisionGrid();
            this.btnOK = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // revisionGrid
            // 
            this.revisionGrid.BranchFilter = "";
            this.revisionGrid.CurrentCheckout = null;
            this.revisionGrid.Filter = "";
            this.revisionGrid.FixedFilter = "";
            this.revisionGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.revisionGrid.InMemAuthorFilter = "";
            this.revisionGrid.InMemCommitterFilter = "";
            this.revisionGrid.InMemMessageFilter = "";
            this.revisionGrid.LastRow = 0;
            this.revisionGrid.Location = new System.Drawing.Point(0, 0);
            this.revisionGrid.Name = "revisionGrid";
            this.revisionGrid.NormalFont = new System.Drawing.Font("Tahoma", 8.75F);
            this.revisionGrid.Size = new System.Drawing.Size(842, 313);
            this.revisionGrid.SuperprojectCurrentCheckout = null;
            this.revisionGrid.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(718, 14);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(112, 29);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 323);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(842, 55);
            this.panel1.TabIndex = 2;
            // 
            // FormChooseCommit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(842, 378);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.revisionGrid);
            this.Name = "FormChooseCommit";
            this.Text = "Choose Commit";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private RevisionGrid revisionGrid;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel panel1;

    }
}