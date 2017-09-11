namespace ProxySwitcher
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
            this.LocalHttpProxy_Label = new System.Windows.Forms.Label();
            this.LocalHttpProxy_TextBox = new System.Windows.Forms.TextBox();
            this.GlobalHttpProxy_Label = new System.Windows.Forms.Label();
            this.GlobalHttpProxy_TextBox = new System.Windows.Forms.TextBox();
            this.ApplyGlobally_CheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.UnsetProxy_Button = new System.Windows.Forms.Button();
            this.SetProxy_Button = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // LocalHttpProxy_Label
            // 
            this.LocalHttpProxy_Label.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.LocalHttpProxy_Label.AutoSize = true;
            this.LocalHttpProxy_Label.Location = new System.Drawing.Point(8, 6);
            this.LocalHttpProxy_Label.Margin = new System.Windows.Forms.Padding(3);
            this.LocalHttpProxy_Label.Name = "LocalHttpProxy_Label";
            this.LocalHttpProxy_Label.Size = new System.Drawing.Size(90, 13);
            this.LocalHttpProxy_Label.TabIndex = 0;
            this.LocalHttpProxy_Label.Text = "Local http.proxy:";
            // 
            // LocalHttpProxy_TextBox
            // 
            this.LocalHttpProxy_TextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LocalHttpProxy_TextBox.Location = new System.Drawing.Point(104, 3);
            this.LocalHttpProxy_TextBox.Name = "LocalHttpProxy_TextBox";
            this.LocalHttpProxy_TextBox.ReadOnly = true;
            this.LocalHttpProxy_TextBox.Size = new System.Drawing.Size(234, 21);
            this.LocalHttpProxy_TextBox.TabIndex = 1;
            // 
            // GlobalHttpProxy_Label
            // 
            this.GlobalHttpProxy_Label.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.GlobalHttpProxy_Label.AutoSize = true;
            this.GlobalHttpProxy_Label.Location = new System.Drawing.Point(3, 32);
            this.GlobalHttpProxy_Label.Margin = new System.Windows.Forms.Padding(3);
            this.GlobalHttpProxy_Label.Name = "GlobalHttpProxy_Label";
            this.GlobalHttpProxy_Label.Size = new System.Drawing.Size(95, 13);
            this.GlobalHttpProxy_Label.TabIndex = 0;
            this.GlobalHttpProxy_Label.Text = "Global http.proxy:";
            // 
            // GlobalHttpProxy_TextBox
            // 
            this.GlobalHttpProxy_TextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GlobalHttpProxy_TextBox.Location = new System.Drawing.Point(104, 29);
            this.GlobalHttpProxy_TextBox.Name = "GlobalHttpProxy_TextBox";
            this.GlobalHttpProxy_TextBox.ReadOnly = true;
            this.GlobalHttpProxy_TextBox.Size = new System.Drawing.Size(234, 21);
            this.GlobalHttpProxy_TextBox.TabIndex = 1;
            // 
            // ApplyGlobally_CheckBox
            // 
            this.ApplyGlobally_CheckBox.AutoSize = true;
            this.ApplyGlobally_CheckBox.Checked = true;
            this.ApplyGlobally_CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ApplyGlobally_CheckBox.Location = new System.Drawing.Point(104, 55);
            this.ApplyGlobally_CheckBox.Name = "ApplyGlobally_CheckBox";
            this.ApplyGlobally_CheckBox.Size = new System.Drawing.Size(92, 17);
            this.ApplyGlobally_CheckBox.TabIndex = 2;
            this.ApplyGlobally_CheckBox.Text = "Apply globally";
            this.ApplyGlobally_CheckBox.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.GlobalHttpProxy_Label, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.ApplyGlobally_CheckBox, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.LocalHttpProxy_Label, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.LocalHttpProxy_TextBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.GlobalHttpProxy_TextBox, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(341, 106);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 2);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 123F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 123F));
            this.tableLayoutPanel2.Controls.Add(this.UnsetProxy_Button, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.SetProxy_Button, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 77);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(341, 29);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // UnsetProxy_Button
            // 
            this.UnsetProxy_Button.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UnsetProxy_Button.Location = new System.Drawing.Point(221, 3);
            this.UnsetProxy_Button.Name = "UnsetProxy_Button";
            this.UnsetProxy_Button.Size = new System.Drawing.Size(117, 23);
            this.UnsetProxy_Button.TabIndex = 6;
            this.UnsetProxy_Button.Text = "Unset proxy";
            this.UnsetProxy_Button.UseVisualStyleBackColor = true;
            this.UnsetProxy_Button.Click += new System.EventHandler(this.UnsetProxy_Button_Click);
            // 
            // SetProxy_Button
            // 
            this.SetProxy_Button.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SetProxy_Button.Location = new System.Drawing.Point(98, 3);
            this.SetProxy_Button.Name = "SetProxy_Button";
            this.SetProxy_Button.Size = new System.Drawing.Size(117, 23);
            this.SetProxy_Button.TabIndex = 7;
            this.SetProxy_Button.Text = "Set proxy";
            this.SetProxy_Button.UseVisualStyleBackColor = true;
            this.SetProxy_Button.Click += new System.EventHandler(this.SetProxy_Button_Click);
            // 
            // ProxySwitcherForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(341, 106);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(357, 144);
            this.Name = "ProxySwitcherForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Proxy Switcher";
            this.Load += new System.EventHandler(this.ProxySwitcherForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LocalHttpProxy_Label;
        private System.Windows.Forms.TextBox LocalHttpProxy_TextBox;
        private System.Windows.Forms.Label GlobalHttpProxy_Label;
        private System.Windows.Forms.CheckBox ApplyGlobally_CheckBox;
        private System.Windows.Forms.TextBox GlobalHttpProxy_TextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button UnsetProxy_Button;
        private System.Windows.Forms.Button SetProxy_Button;
    }
}