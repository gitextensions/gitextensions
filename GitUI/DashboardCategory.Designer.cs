﻿namespace GitUI
{
    partial class DashboardCategory
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
            this._NO_TRANSLATE_Caption = new System.Windows.Forms.Label();
            this.CategoryContextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.CategoryContextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _NO_TRANSLATE_Caption
            // 
            this._NO_TRANSLATE_Caption.AutoSize = true;
            this._NO_TRANSLATE_Caption.ContextMenuStrip = this.CategoryContextMenuStrip1;
            this._NO_TRANSLATE_Caption.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this._NO_TRANSLATE_Caption.Location = new System.Drawing.Point(3, 2);
            this._NO_TRANSLATE_Caption.Name = "_NO_TRANSLATE_Caption";
            this._NO_TRANSLATE_Caption.Size = new System.Drawing.Size(44, 15);
            this._NO_TRANSLATE_Caption.TabIndex = 8;
            this._NO_TRANSLATE_Caption.Text = "##Title";
            // 
            // CategoryContextMenuStrip1
            // 
            this.CategoryContextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.editToolStripMenuItem});
            this.CategoryContextMenuStrip1.Name = "CategoryContextMenuStrip1";
            this.CategoryContextMenuStrip1.Size = new System.Drawing.Size(156, 92);
            // 
            // moveUpToolStripMenuItem
            // 
            this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            this.moveUpToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.moveUpToolStripMenuItem.Text = "Move Up";
            this.moveUpToolStripMenuItem.Click += new System.EventHandler(this.moveUpToolStripMenuItem_Click);
            // 
            // moveDownToolStripMenuItem
            // 
            this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            this.moveDownToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.moveDownToolStripMenuItem.Text = "Move Down";
            this.moveDownToolStripMenuItem.Click += new System.EventHandler(this.moveDownToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click_1);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.AutoSize = true;
            this.flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel.Location = new System.Drawing.Point(0, 23);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(256, 22);
            this.flowLayoutPanel.TabIndex = 9;
            this.flowLayoutPanel.WrapContents = false;
            // 
            // DashboardCategory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.flowLayoutPanel);
            this.Controls.Add(this._NO_TRANSLATE_Caption);
            this.Name = "DashboardCategory";
            this.Size = new System.Drawing.Size(259, 48);
            this.CategoryContextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _NO_TRANSLATE_Caption;
        private System.Windows.Forms.ContextMenuStrip CategoryContextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
    }
}
