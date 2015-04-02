namespace GitUI.CommandsDialogs.AboutBoxDialog
{
    partial class FormContributors
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
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.Coders = new System.Windows.Forms.ListBox();
            this.Translators = new System.Windows.Forms.ListBox();
            this.Designers = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 15);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 40, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Coders:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 295);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 0, 40, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Translators:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.Coders, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.Translators, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.Designers, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(15);
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(666, 546);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 400);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 0, 40, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "Logo design:";
            // 
            // Coders
            // 
            this.Coders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Coders.ItemHeight = 15;
            this.Coders.Location = new System.Drawing.Point(136, 18);
            this.Coders.Name = "Coders";
            this.Coders.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.Coders.Size = new System.Drawing.Size(512, 274);
            this.Coders.TabIndex = 6;
            // 
            // Translators
            // 
            this.Translators.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Translators.ItemHeight = 15;
            this.Translators.Location = new System.Drawing.Point(136, 298);
            this.Translators.Name = "Translators";
            this.Translators.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.Translators.Size = new System.Drawing.Size(512, 99);
            this.Translators.TabIndex = 7;
            // 
            // Designers
            // 
            this.Designers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Designers.ItemHeight = 15;
            this.Designers.Location = new System.Drawing.Point(136, 403);
            this.Designers.Name = "Designers";
            this.Designers.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.Designers.Size = new System.Drawing.Size(512, 125);
            this.Designers.TabIndex = 8;
            // 
            // FormContributors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(666, 546);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormContributors";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Contributors";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox Coders;
        private System.Windows.Forms.ListBox Translators;
        private System.Windows.Forms.ListBox Designers;

    }
}