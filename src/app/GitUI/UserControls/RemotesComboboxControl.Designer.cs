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
            comboBoxRemotes = new ComboBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            buttonSelectMultipleRemotes = new Button();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // comboBoxRemotes
            // 
            comboBoxRemotes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxRemotes.FormattingEnabled = true;
            comboBoxRemotes.Location = new Point(0, 0);
            comboBoxRemotes.Margin = new Padding(0);
            comboBoxRemotes.Name = "comboBoxRemotes";
            comboBoxRemotes.Size = new Size(158, 23);
            comboBoxRemotes.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(buttonSelectMultipleRemotes, 1, 0);
            tableLayoutPanel1.Controls.Add(comboBoxRemotes, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(181, 25);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // buttonSelectMultipleRemotes
            // 
            buttonSelectMultipleRemotes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            buttonSelectMultipleRemotes.Image = Properties.Images.Select;
            buttonSelectMultipleRemotes.Location = new Point(158, 0);
            buttonSelectMultipleRemotes.Margin = new Padding(0);
            buttonSelectMultipleRemotes.Name = "buttonSelectMultipleRemotes";
            buttonSelectMultipleRemotes.Size = new Size(23, 25);
            buttonSelectMultipleRemotes.TabIndex = 2;
            buttonSelectMultipleRemotes.UseVisualStyleBackColor = true;
            // 
            // RemotesComboboxControl
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(tableLayoutPanel1);
            Name = "RemotesComboboxControl";
            Size = new Size(181, 25);
            Load += RemotesComboboxControl_Load;
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private ComboBox comboBoxRemotes;
        private TableLayoutPanel tableLayoutPanel1;
        private Button buttonSelectMultipleRemotes;
    }
}
