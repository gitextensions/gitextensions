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
            this.translatorsLabel = new System.Windows.Forms.Label();
            this.codersLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.designersLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 19);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 50, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Coders:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 384);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 50, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 23);
            this.label2.TabIndex = 1;
            this.label2.Text = "Translators:";
            // 
            // translatorsLabel
            // 
            this.translatorsLabel.AutoSize = true;
            this.translatorsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.translatorsLabel.Location = new System.Drawing.Point(185, 384);
            this.translatorsLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 15);
            this.translatorsLabel.MinimumSize = new System.Drawing.Size(0, 100);
            this.translatorsLabel.Name = "translatorsLabel";
            this.translatorsLabel.Size = new System.Drawing.Size(623, 100);
            this.translatorsLabel.TabIndex = 2;
            this.translatorsLabel.Text = "Translators";
            // 
            // codersLabel
            // 
            this.codersLabel.AutoSize = true;
            this.codersLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codersLabel.Location = new System.Drawing.Point(185, 19);
            this.codersLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 15);
            this.codersLabel.MinimumSize = new System.Drawing.Size(0, 350);
            this.codersLabel.Name = "codersLabel";
            this.codersLabel.Size = new System.Drawing.Size(623, 350);
            this.codersLabel.TabIndex = 3;
            this.codersLabel.Text = "Coders";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.designersLabel, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.translatorsLabel, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.codersLabel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(19);
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(832, 683);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // designersLabel
            // 
            this.designersLabel.AutoSize = true;
            this.designersLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.designersLabel.Location = new System.Drawing.Point(185, 499);
            this.designersLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 15);
            this.designersLabel.MinimumSize = new System.Drawing.Size(0, 60);
            this.designersLabel.Name = "designersLabel";
            this.designersLabel.Size = new System.Drawing.Size(623, 150);
            this.designersLabel.TabIndex = 5;
            this.designersLabel.Text = "Designers";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 499);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 50, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 23);
            this.label4.TabIndex = 4;
            this.label4.Text = "Logo design:";
            // 
            // FormContributors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(832, 683);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
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
        private System.Windows.Forms.Label translatorsLabel;
        private System.Windows.Forms.Label codersLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label designersLabel;
        private System.Windows.Forms.Label label4;

    }
}