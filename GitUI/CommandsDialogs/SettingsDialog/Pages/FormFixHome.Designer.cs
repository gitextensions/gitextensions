namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class FormFixHome
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFixHome));
            groupBox8 = new GroupBox();
            otherHomeBrowse = new GitUI.UserControls.FolderBrowserButton();
            otherHomeDir = new TextBox();
            otherHome = new RadioButton();
            userprofileHome = new RadioButton();
            defaultHome = new RadioButton();
            label51 = new Label();
            ok = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            groupBox8.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox8
            // 
            groupBox8.Controls.Add(otherHomeBrowse);
            groupBox8.Controls.Add(otherHomeDir);
            groupBox8.Controls.Add(otherHome);
            groupBox8.Controls.Add(userprofileHome);
            groupBox8.Controls.Add(defaultHome);
            groupBox8.Controls.Add(label51);
            groupBox8.Dock = DockStyle.Fill;
            groupBox8.Location = new Point(8, 8);
            groupBox8.Name = "groupBox8";
            groupBox8.Size = new Size(588, 154);
            groupBox8.TabIndex = 0;
            groupBox8.TabStop = false;
            groupBox8.Text = "Environment";
            // 
            // otherHomeBrowse
            // 
            otherHomeBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            otherHomeBrowse.Location = new Point(444, 114);
            otherHomeBrowse.Name = "otherHomeBrowse";
            otherHomeBrowse.PathShowingControl = otherHomeDir;
            otherHomeBrowse.Size = new Size(130, 25);
            otherHomeBrowse.TabIndex = 5;
            // 
            // otherHomeDir
            // 
            otherHomeDir.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            otherHomeDir.Location = new Point(141, 115);
            otherHomeDir.MaxLength = 250;
            otherHomeDir.Name = "otherHomeDir";
            otherHomeDir.Size = new Size(292, 21);
            otherHomeDir.TabIndex = 4;
            // 
            // otherHome
            // 
            otherHome.AutoSize = true;
            otherHome.Location = new Point(11, 118);
            otherHome.Name = "otherHome";
            otherHome.Size = new Size(53, 17);
            otherHome.TabIndex = 3;
            otherHome.TabStop = true;
            otherHome.Text = "Other";
            otherHome.UseVisualStyleBackColor = true;
            // 
            // userprofileHome
            // 
            userprofileHome.AutoSize = true;
            userprofileHome.Location = new Point(11, 95);
            userprofileHome.Name = "userprofileHome";
            userprofileHome.Size = new Size(157, 17);
            userprofileHome.TabIndex = 2;
            userprofileHome.TabStop = true;
            userprofileHome.Text = "Set HOME to USERPROFILE";
            userprofileHome.UseVisualStyleBackColor = true;
            // 
            // defaultHome
            // 
            defaultHome.AutoSize = true;
            defaultHome.Location = new Point(11, 72);
            defaultHome.Name = "defaultHome";
            defaultHome.Size = new Size(129, 17);
            defaultHome.TabIndex = 1;
            defaultHome.TabStop = true;
            defaultHome.Text = "Use default for HOME";
            defaultHome.UseVisualStyleBackColor = true;
            // 
            // label51
            // 
            label51.AutoSize = true;
            label51.Location = new Point(8, 19);
            label51.Name = "label51";
            label51.Size = new Size(412, 39);
            label51.TabIndex = 0;
            label51.Text = resources.GetString("label51.Text");
            // 
            // ok
            // 
            ok.Location = new Point(455, 3);
            ok.Name = "ok";
            ok.Size = new Size(130, 25);
            ok.TabIndex = 0;
            ok.Text = "OK";
            ok.UseVisualStyleBackColor = true;
            ok.Click += ok_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(ok);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(8, 162);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(588, 31);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // FormFixHome
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(604, 201);
            Controls.Add(groupBox8);
            Controls.Add(flowLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(620, 240);
            Name = "FormFixHome";
            Padding = new Padding(8);
            StartPosition = FormStartPosition.CenterParent;
            groupBox8.ResumeLayout(false);
            groupBox8.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private GroupBox groupBox8;
        private UserControls.FolderBrowserButton otherHomeBrowse;
        private TextBox otherHomeDir;
        private RadioButton otherHome;
        private RadioButton userprofileHome;
        private RadioButton defaultHome;
        private Label label51;
        private Button ok;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}
