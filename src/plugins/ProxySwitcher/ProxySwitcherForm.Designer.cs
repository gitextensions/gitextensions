namespace GitExtensions.Plugins.ProxySwitcher
{
    partial class ProxySwitcherForm
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
            LocalHttpProxy_Label = new Label();
            LocalHttpProxy_TextBox = new TextBox();
            GlobalHttpProxy_Label = new Label();
            GlobalHttpProxy_TextBox = new TextBox();
            ApplyGlobally_CheckBox = new CheckBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            UnsetProxy_Button = new Button();
            SetProxy_Button = new Button();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // LocalHttpProxy_Label
            // 
            LocalHttpProxy_Label.Anchor = AnchorStyles.Right;
            LocalHttpProxy_Label.AutoSize = true;
            LocalHttpProxy_Label.Location = new Point(8, 6);
            LocalHttpProxy_Label.Margin = new Padding(3);
            LocalHttpProxy_Label.Name = "LocalHttpProxy_Label";
            LocalHttpProxy_Label.Size = new Size(90, 13);
            LocalHttpProxy_Label.TabIndex = 0;
            LocalHttpProxy_Label.Text = "Local http.proxy:";
            // 
            // LocalHttpProxy_TextBox
            // 
            LocalHttpProxy_TextBox.Dock = DockStyle.Fill;
            LocalHttpProxy_TextBox.Location = new Point(104, 3);
            LocalHttpProxy_TextBox.Name = "LocalHttpProxy_TextBox";
            LocalHttpProxy_TextBox.ReadOnly = true;
            LocalHttpProxy_TextBox.Size = new Size(234, 21);
            LocalHttpProxy_TextBox.TabIndex = 1;
            // 
            // GlobalHttpProxy_Label
            // 
            GlobalHttpProxy_Label.Anchor = AnchorStyles.Right;
            GlobalHttpProxy_Label.AutoSize = true;
            GlobalHttpProxy_Label.Location = new Point(3, 32);
            GlobalHttpProxy_Label.Margin = new Padding(3);
            GlobalHttpProxy_Label.Name = "GlobalHttpProxy_Label";
            GlobalHttpProxy_Label.Size = new Size(95, 13);
            GlobalHttpProxy_Label.TabIndex = 0;
            GlobalHttpProxy_Label.Text = "Global http.proxy:";
            // 
            // GlobalHttpProxy_TextBox
            // 
            GlobalHttpProxy_TextBox.Dock = DockStyle.Fill;
            GlobalHttpProxy_TextBox.Location = new Point(104, 29);
            GlobalHttpProxy_TextBox.Name = "GlobalHttpProxy_TextBox";
            GlobalHttpProxy_TextBox.ReadOnly = true;
            GlobalHttpProxy_TextBox.Size = new Size(234, 21);
            GlobalHttpProxy_TextBox.TabIndex = 1;
            // 
            // ApplyGlobally_CheckBox
            // 
            ApplyGlobally_CheckBox.AutoSize = true;
            ApplyGlobally_CheckBox.Checked = true;
            ApplyGlobally_CheckBox.CheckState = CheckState.Checked;
            ApplyGlobally_CheckBox.Location = new Point(104, 55);
            ApplyGlobally_CheckBox.Name = "ApplyGlobally_CheckBox";
            ApplyGlobally_CheckBox.Size = new Size(92, 17);
            ApplyGlobally_CheckBox.TabIndex = 2;
            ApplyGlobally_CheckBox.Text = "Apply globally";
            ApplyGlobally_CheckBox.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(GlobalHttpProxy_Label, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 3);
            tableLayoutPanel1.Controls.Add(ApplyGlobally_CheckBox, 1, 2);
            tableLayoutPanel1.Controls.Add(LocalHttpProxy_Label, 0, 0);
            tableLayoutPanel1.Controls.Add(LocalHttpProxy_TextBox, 1, 0);
            tableLayoutPanel1.Controls.Add(GlobalHttpProxy_TextBox, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(341, 106);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel1.SetColumnSpan(tableLayoutPanel2, 2);
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 123F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 123F));
            tableLayoutPanel2.Controls.Add(UnsetProxy_Button, 2, 0);
            tableLayoutPanel2.Controls.Add(SetProxy_Button, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 77);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(341, 29);
            tableLayoutPanel2.TabIndex = 3;
            // 
            // UnsetProxy_Button
            // 
            UnsetProxy_Button.Dock = DockStyle.Fill;
            UnsetProxy_Button.Location = new Point(221, 3);
            UnsetProxy_Button.Name = "UnsetProxy_Button";
            UnsetProxy_Button.Size = new Size(117, 23);
            UnsetProxy_Button.TabIndex = 6;
            UnsetProxy_Button.Text = "Unset proxy";
            UnsetProxy_Button.UseVisualStyleBackColor = true;
            UnsetProxy_Button.Click += UnsetProxy_Button_Click;
            // 
            // SetProxy_Button
            // 
            SetProxy_Button.Dock = DockStyle.Fill;
            SetProxy_Button.Location = new Point(98, 3);
            SetProxy_Button.Name = "SetProxy_Button";
            SetProxy_Button.Size = new Size(117, 23);
            SetProxy_Button.TabIndex = 7;
            SetProxy_Button.Text = "Set proxy";
            SetProxy_Button.UseVisualStyleBackColor = true;
            SetProxy_Button.Click += SetProxy_Button_Click;
            // 
            // ProxySwitcherForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(341, 106);
            Controls.Add(tableLayoutPanel1);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(357, 144);
            Name = "ProxySwitcherForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Proxy Switcher";
            Load += ProxySwitcherForm_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private Label LocalHttpProxy_Label;
        private TextBox LocalHttpProxy_TextBox;
        private Label GlobalHttpProxy_Label;
        private CheckBox ApplyGlobally_CheckBox;
        private TextBox GlobalHttpProxy_TextBox;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button UnsetProxy_Button;
        private Button SetProxy_Button;
    }
}
