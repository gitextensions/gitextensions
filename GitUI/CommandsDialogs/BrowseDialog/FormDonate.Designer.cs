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
            if (disposing && (components is not null))
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
            pbxDonate = new PictureBox();
            lblText = new Label();
            tableLayoutPanel = new TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(pbxDonate)).BeginInit();
            tableLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // pbxDonate
            // 
            pbxDonate.BackgroundImage = Properties.Images.DonateBadge;
            pbxDonate.BackgroundImageLayout = ImageLayout.Zoom;
            pbxDonate.Cursor = Cursors.Hand;
            pbxDonate.Dock = DockStyle.Fill;
            pbxDonate.Location = new Point(12, 142);
            pbxDonate.Margin = new Padding(12);
            pbxDonate.MinimumSize = new Size(90, 35);
            pbxDonate.Name = "pbxDonate";
            pbxDonate.Size = new Size(452, 56);
            pbxDonate.TabIndex = 0;
            pbxDonate.TabStop = false;
            pbxDonate.Click += PictureBox1Click;
            // 
            // lblText
            // 
            lblText.Dock = DockStyle.Fill;
            lblText.Location = new Point(10, 10);
            lblText.Margin = new Padding(10);
            lblText.Name = "lblText";
            lblText.Size = new Size(456, 110);
            lblText.TabIndex = 0;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.AutoSize = true;
            tableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel.ColumnCount = 1;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel.Controls.Add(lblText, 0, 0);
            tableLayoutPanel.Controls.Add(pbxDonate, 0, 1);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(16, 16);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 3;
            tableLayoutPanel.RowStyles.Add(new RowStyle());
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Size = new Size(476, 234);
            tableLayoutPanel.TabIndex = 2;
            // 
            // FormDonate
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(508, 266);
            Controls.Add(tableLayoutPanel);
            Name = "FormDonate";
            Padding = new Padding(16);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Donate";
            ((System.ComponentModel.ISupportInitialize)(pbxDonate)).EndInit();
            tableLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private PictureBox pbxDonate;
        private Label lblText;
        private TableLayoutPanel tableLayoutPanel;

    }
}