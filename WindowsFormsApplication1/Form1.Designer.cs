using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusFiller = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnDelayed = new System.Windows.Forms.Button();
            this.btnAtOnce = new System.Windows.Forms.Button();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.Top;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusFiller,
            this.toolStripStatusLabel1});
            this.statusStrip.Location = new System.Drawing.Point(0, 0);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(706, 22);
            this.statusStrip.TabIndex = 5;
            // 
            // statusFiller
            // 
            this.statusFiller.Name = "statusFiller";
            this.statusFiller.Size = new System.Drawing.Size(677, 17);
            this.statusFiller.Spring = true;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(14, 17);
            this.toolStripStatusLabel1.Text = "X";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnDelayed
            // 
            this.btnDelayed.Location = new System.Drawing.Point(163, 38);
            this.btnDelayed.Name = "btnDelayed";
            this.btnDelayed.Size = new System.Drawing.Size(187, 42);
            this.btnDelayed.TabIndex = 6;
            this.btnDelayed.Text = "Test Delayed";
            this.btnDelayed.UseVisualStyleBackColor = true;
            // 
            // btnAtOnce
            // 
            this.btnAtOnce.Location = new System.Drawing.Point(425, 38);
            this.btnAtOnce.Name = "btnAtOnce";
            this.btnAtOnce.Size = new System.Drawing.Size(156, 42);
            this.btnAtOnce.TabIndex = 7;
            this.btnAtOnce.Text = "Test All at Once";
            this.btnAtOnce.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(706, 92);
            this.Controls.Add(this.btnAtOnce);
            this.Controls.Add(this.btnDelayed);
            this.Controls.Add(this.statusStrip);
            this.Name = "Form1";
            this.Text = "Form1";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusFiller;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private Button btnDelayed;
        private Button btnAtOnce;
    }
}

