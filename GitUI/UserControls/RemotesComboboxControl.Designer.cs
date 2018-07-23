namespace GitUI.UserControls
{
    partial class RemotesComboboxControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBoxRemotes = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonSelectMultipleRemotes = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxRemotes
            // 
            this.comboBoxRemotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxRemotes.FormattingEnabled = true;
            this.comboBoxRemotes.Location = new System.Drawing.Point(0, 0);
            this.comboBoxRemotes.Margin = new System.Windows.Forms.Padding(0);
            this.comboBoxRemotes.Name = "comboBoxRemotes";
            this.comboBoxRemotes.Size = new System.Drawing.Size(158, 23);
            this.comboBoxRemotes.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.buttonSelectMultipleRemotes, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxRemotes, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(181, 25);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // buttonSelectMultipleRemotes
            // 
            this.buttonSelectMultipleRemotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectMultipleRemotes.Image = global::GitUI.Properties.Images.Select;
            this.buttonSelectMultipleRemotes.Location = new System.Drawing.Point(158, 0);
            this.buttonSelectMultipleRemotes.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSelectMultipleRemotes.Name = "buttonSelectMultipleRemotes";
            this.buttonSelectMultipleRemotes.Size = new System.Drawing.Size(23, 25);
            this.buttonSelectMultipleRemotes.TabIndex = 2;
            this.buttonSelectMultipleRemotes.UseVisualStyleBackColor = true;
            // 
            // RemotesComboboxControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "RemotesComboboxControl";
            this.Size = new System.Drawing.Size(181, 25);
            this.Load += new System.EventHandler(this.RemotesComboboxControl_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxRemotes;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button buttonSelectMultipleRemotes;
    }
}
