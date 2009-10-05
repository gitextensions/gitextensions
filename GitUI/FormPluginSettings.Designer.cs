namespace GitUI
{
    partial class FormPluginSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPluginSettings));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.PluginList = new System.Windows.Forms.ListBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.PluginList);
            this.splitContainer1.Size = new System.Drawing.Size(737, 291);
            this.splitContainer1.SplitterDistance = 178;
            this.splitContainer1.TabIndex = 0;
            // 
            // PluginList
            // 
            this.PluginList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PluginList.FormattingEnabled = true;
            this.PluginList.Location = new System.Drawing.Point(0, 0);
            this.PluginList.Name = "PluginList";
            this.PluginList.Size = new System.Drawing.Size(178, 290);
            this.PluginList.TabIndex = 1;
            this.PluginList.SelectedIndexChanged += new System.EventHandler(this.PluginList_SelectedIndexChanged);
            // 
            // FormPluginSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 291);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPluginSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Plugin settings";
            this.Load += new System.EventHandler(this.FormPluginSettings_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormPluginSettings_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox PluginList;
    }
}