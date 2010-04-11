namespace GitUI
{
    partial class Gravatar
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
            this.components = new System.ComponentModel.Container();
            this.imgGravatar = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.registerAtGravatarcomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearImagecacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.imgGravatar)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // imgGravatar
            // 
            this.imgGravatar.ContextMenuStrip = this.contextMenuStrip;
            this.imgGravatar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgGravatar.Image = global::GitUI.Properties.Resources.User;
            this.imgGravatar.Location = new System.Drawing.Point(0, 0);
            this.imgGravatar.Name = "imgGravatar";
            this.imgGravatar.Size = new System.Drawing.Size(150, 150);
            this.imgGravatar.TabIndex = 0;
            this.imgGravatar.TabStop = false;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshToolStripMenuItem,
            this.clearImagecacheToolStripMenuItem,
            this.registerAtGravatarcomToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(207, 92);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.refreshToolStripMenuItem.Text = "Refresh image";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // registerAtGravatarcomToolStripMenuItem
            // 
            this.registerAtGravatarcomToolStripMenuItem.Name = "registerAtGravatarcomToolStripMenuItem";
            this.registerAtGravatarcomToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.registerAtGravatarcomToolStripMenuItem.Text = "Register at gravatar.com";
            this.registerAtGravatarcomToolStripMenuItem.Click += new System.EventHandler(this.registerAtGravatarcomToolStripMenuItem_Click);
            // 
            // clearImagecacheToolStripMenuItem
            // 
            this.clearImagecacheToolStripMenuItem.Name = "clearImagecacheToolStripMenuItem";
            this.clearImagecacheToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.clearImagecacheToolStripMenuItem.Text = "Clear image cache";
            this.clearImagecacheToolStripMenuItem.Click += new System.EventHandler(this.clearImagecacheToolStripMenuItem_Click);
            // 
            // Gravatar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.imgGravatar);
            this.Name = "Gravatar";
            ((System.ComponentModel.ISupportInitialize)(this.imgGravatar)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox imgGravatar;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem registerAtGravatarcomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearImagecacheToolStripMenuItem;
    }
}
