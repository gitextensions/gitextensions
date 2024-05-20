namespace GitUI.UserControls.Settings
{
    partial class SettingsCheckBox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel = new TableLayoutPanel();
            checkBox = new CheckBox();
            pictureBox = new PictureBox();
            tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.AutoSize = true;
            tableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(checkBox, 0, 0);
            tableLayoutPanel.Controls.Add(pictureBox, 1, 0);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(0, 0);
            tableLayoutPanel.Margin = new Padding(0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 1;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Size = new Size(104, 17);
            tableLayoutPanel.TabIndex = 0;
            // 
            // checkBox
            // 
            checkBox.AutoSize = true;
            checkBox.Location = new Point(0, 0);
            checkBox.Margin = new Padding(0, 0, 2, 0);
            checkBox.Name = "checkBox";
            checkBox.Size = new Size(84, 17);
            checkBox.TabIndex = 0;
            checkBox.Text = "Settings text";
            checkBox.UseVisualStyleBackColor = true;
            // 
            // pictureBox
            // 
            pictureBox.Cursor = Cursors.Hand;
            pictureBox.Image = Properties.Resources.information;
            pictureBox.Location = new Point(88, 0);
            pictureBox.Margin = new Padding(2, 0, 0, 0);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(16, 16);
            pictureBox.TabIndex = 1;
            pictureBox.TabStop = false;
            pictureBox.Visible = false;
            // 
            // SettingsCheckBox
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(tableLayoutPanel);
            Name = "SettingsCheckBox";
            Size = new Size(104, 17);
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel;
        private CheckBox checkBox;
        private PictureBox pictureBox;
    }
}
