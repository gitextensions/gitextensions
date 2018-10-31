using System.Windows.Forms;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    partial class FormDonate
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
            this.pbxDonate = new System.Windows.Forms.PictureBox();
            this.lblText = new System.Windows.Forms.Label();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.pbxDonate)).BeginInit();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbxDonate
            // 
            this.pbxDonate.BackgroundImage = global::GitUI.Properties.Images.DonateBadge;
            this.pbxDonate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pbxDonate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbxDonate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxDonate.Location = new System.Drawing.Point(12, 142);
            this.pbxDonate.Margin = new System.Windows.Forms.Padding(12);
            this.pbxDonate.MinimumSize = new System.Drawing.Size(90, 35);
            this.pbxDonate.Name = "pbxDonate";
            this.pbxDonate.Size = new System.Drawing.Size(452, 56);
            this.pbxDonate.TabIndex = 0;
            this.pbxDonate.TabStop = false;
            this.pbxDonate.Click += new System.EventHandler(this.PictureBox1Click);
            // 
            // lblText
            // 
            this.lblText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblText.Location = new System.Drawing.Point(10, 10);
            this.lblText.Margin = new System.Windows.Forms.Padding(10);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(456, 110);
            this.lblText.TabIndex = 0;
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.AutoSize = true;
            this.tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Controls.Add(this.lblText, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.pbxDonate, 0, 1);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(16, 16);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 3;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(476, 234);
            this.tableLayoutPanel.TabIndex = 2;
            // 
            // FormDonate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(508, 266);
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "FormDonate";
            this.Padding = new System.Windows.Forms.Padding(16);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Donate";
            ((System.ComponentModel.ISupportInitialize)(this.pbxDonate)).EndInit();
            this.tableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBox pbxDonate;
        private Label lblText;
        private TableLayoutPanel tableLayoutPanel;

    }
}