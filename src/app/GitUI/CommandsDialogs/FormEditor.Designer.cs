namespace GitUI.CommandsDialogs
{
    partial class FormEditor
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
            components = new System.ComponentModel.Container();
            tableLayoutPanel1 = new TableLayoutPanel();
            panelMessage = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label1 = new Label();
            labelWarning = new Label();
            toolStrip1 = new GitUI.ToolStripEx();
            toolStripSaveButton = new ToolStripButton();
            fileViewer = new GitUI.Editor.FileViewer();
            tableLayoutPanel1.SuspendLayout();
            panelMessage.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(toolStrip1, 0, 0);
            tableLayoutPanel1.Controls.Add(fileViewer, 0, 2);
            tableLayoutPanel1.Controls.Add(panelMessage, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(659, 543);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // panelMessage
            // 
            panelMessage.BackColor = Color.Salmon;
            panelMessage.Controls.Add(flowLayoutPanel1);
            panelMessage.Dock = DockStyle.Fill;
            panelMessage.Location = new Point(3, 28);
            panelMessage.Name = "panelMessage";
            panelMessage.Padding = new Padding(5);
            panelMessage.Size = new Size(653, 60);
            panelMessage.TabIndex = 2;
            panelMessage.Visible = false;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(label1);
            flowLayoutPanel1.Controls.Add(labelWarning);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(5, 5);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(643, 50);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.Image = Properties.Images.Warning;
            label1.ImageAlign = ContentAlignment.MiddleLeft;
            label1.Location = new Point(3, 19);
            label1.Name = "label1";
            label1.Size = new Size(22, 22);
            label1.TabIndex = 0;
            label1.Text = "   ";
            // 
            // labelWarning
            // 
            labelWarning.Anchor = AnchorStyles.Left;
            labelWarning.AutoSize = true;
            labelWarning.Location = new Point(31, 0);
            labelWarning.Name = "labelWarning";
            labelWarning.Size = new Size(382, 60);
            labelWarning.TabIndex = 1;
            labelWarning.Text = "Here be dragons!\r\nChanging this file by hand can be harmful and might break somet" +
    "hing.\r\nIf you are not sure just close this window.";
            // 
            // toolStrip1
            // 
            toolStrip1.ClickThrough = true;
            toolStrip1.DrawBorder = false;
            toolStrip1.Items.AddRange(new ToolStripItem[] {
            toolStripSaveButton});
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(659, 25);
            toolStrip1.TabIndex = 1;
            // 
            // toolStripSaveButton
            // 
            toolStripSaveButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripSaveButton.Image = Properties.Images.Save;
            toolStripSaveButton.ImageTransparentColor = Color.Magenta;
            toolStripSaveButton.Name = "toolStripSaveButton";
            toolStripSaveButton.Size = new Size(23, 22);
            toolStripSaveButton.ToolTipText = "Save";
            toolStripSaveButton.Click += toolStripSaveButton_Click;
            // 
            // fileViewer
            // 
            fileViewer.Dock = DockStyle.Fill;
            fileViewer.Location = new Point(0, 91);
            fileViewer.Margin = new Padding(0);
            fileViewer.Name = "fileViewer";
            fileViewer.Size = new Size(659, 452);
            fileViewer.TabIndex = 0;
            // 
            // FormEditor
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(659, 543);
            Controls.Add(tableLayoutPanel1);
            Name = "FormEditor";
            Text = "Editor";
            FormClosing += FormEditor_FormClosing;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panelMessage.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private GitUI.Editor.FileViewer fileViewer;
        private ToolStripEx toolStrip1;
        private ToolStripButton toolStripSaveButton;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panelMessage;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label label1;
        private Label labelWarning;
    }
}
