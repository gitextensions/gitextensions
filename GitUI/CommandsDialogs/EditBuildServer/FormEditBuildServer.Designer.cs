namespace GitUI.CommandsDialogs.EditBuildServer
{
    partial class FormEditBuildServer
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
            this.buildServerSettingsPanel = new System.Windows.Forms.Panel();
            this.BuildServerType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buildServerSettingsPanel
            // 
            this.buildServerSettingsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buildServerSettingsPanel.AutoSize = true;
            this.buildServerSettingsPanel.Location = new System.Drawing.Point(13, 92);
            this.buildServerSettingsPanel.Name = "buildServerSettingsPanel";
            this.buildServerSettingsPanel.Size = new System.Drawing.Size(555, 219);
            this.buildServerSettingsPanel.TabIndex = 3;
            // 
            // BuildServerType
            // 
            this.BuildServerType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BuildServerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BuildServerType.FormattingEnabled = true;
            this.BuildServerType.Items.AddRange(new object[] {
            "(None)",
            "TeamCity"});
            this.BuildServerType.Location = new System.Drawing.Point(112, 54);
            this.BuildServerType.Name = "BuildServerType";
            this.BuildServerType.Size = new System.Drawing.Size(236, 23);
            this.BuildServerType.TabIndex = 2;
            this.BuildServerType.SelectedIndexChanged += new System.EventHandler(this.BuildServerType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Build server type";
            // 
            // label50
            // 
            this.label50.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label50.AutoSize = true;
            this.label50.Location = new System.Drawing.Point(10, 12);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(559, 30);
            this.label50.TabIndex = 0;
            this.label50.Text = "Git Extensions can integrate with build servers to supply Continuous Integration " +
    "information per commit.\r\nSet the build server information below:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 317);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(580, 44);
            this.panel1.TabIndex = 4;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(448, 7);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(110, 25);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(316, 7);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(110, 25);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.OK_Click);
            // 
            // FormEditBuildServer
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(580, 361);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.buildServerSettingsPanel);
            this.Controls.Add(this.BuildServerType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label50);
            this.Name = "FormEditBuildServer";
            this.Text = "Configure .buildserver";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel buildServerSettingsPanel;
        private System.Windows.Forms.ComboBox BuildServerType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;

    }
}