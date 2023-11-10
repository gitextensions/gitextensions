namespace GitUI.CommandsDialogs
{
    partial class FormAddFiles
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
            force = new CheckBox();
            ShowFiles = new Button();
            AddFiles = new Button();
            Filter = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // force
            // 
            force.AutoSize = true;
            force.Location = new Point(89, 40);
            force.Margin = new Padding(4);
            force.Name = "force";
            force.Size = new Size(73, 27);
            force.TabIndex = 4;
            force.Text = "Force";
            force.UseVisualStyleBackColor = true;
            // 
            // ShowFiles
            // 
            ShowFiles.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ShowFiles.Location = new Point(340, 4);
            ShowFiles.Margin = new Padding(4);
            ShowFiles.Name = "ShowFiles";
            ShowFiles.Size = new Size(94, 31);
            ShowFiles.TabIndex = 3;
            ShowFiles.Text = "Show files";
            ShowFiles.UseVisualStyleBackColor = true;
            ShowFiles.Click += ShowFilesClick;
            // 
            // AddFiles
            // 
            AddFiles.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddFiles.Location = new Point(442, 4);
            AddFiles.Margin = new Padding(4);
            AddFiles.Name = "AddFiles";
            AddFiles.Size = new Size(94, 31);
            AddFiles.TabIndex = 2;
            AddFiles.Text = "Add files";
            AddFiles.UseVisualStyleBackColor = true;
            AddFiles.Click += AddFilesClick;
            // 
            // Filter
            // 
            Filter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Filter.Location = new Point(89, 6);
            Filter.Margin = new Padding(4);
            Filter.Name = "Filter";
            Filter.Size = new Size(217, 30);
            Filter.TabIndex = 1;
            Filter.Text = ".";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(15, 6);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(47, 23);
            label1.TabIndex = 0;
            label1.Text = "Filter";
            // 
            // FormAddFiles
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(552, 73);
            Controls.Add(force);
            Controls.Add(ShowFiles);
            Controls.Add(AddFiles);
            Controls.Add(Filter);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Margin = new Padding(4, 4, 4, 4);
            MaximizeBox = false;
            MaximumSize = new Size(10000, 120);
            MinimizeBox = false;
            MinimumSize = new Size(570, 120);
            Name = "FormAddFiles";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Add files";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox Filter;
        private Label label1;
        private Button AddFiles;
        private Button ShowFiles;
        private CheckBox force;
    }
}