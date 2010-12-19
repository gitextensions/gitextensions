namespace GitUI
{
    partial class FormChooseTranslation
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
            this.label1 = new System.Windows.Forms.Label();
            this.brittish = new System.Windows.Forms.PictureBox();
            this.dutch = new System.Windows.Forms.PictureBox();
            this.italian = new System.Windows.Forms.PictureBox();
            this.japanese = new System.Windows.Forms.PictureBox();
            this.spanish = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.brittish)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dutch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.italian)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.japanese)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spanish)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Choose your language";
            // 
            // brittish
            // 
            this.brittish.BackgroundImage = global::GitUI.Properties.Resources.Brittish1;
            this.brittish.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.brittish.Cursor = System.Windows.Forms.Cursors.Hand;
            this.brittish.Location = new System.Drawing.Point(103, 3);
            this.brittish.Name = "brittish";
            this.brittish.Size = new System.Drawing.Size(94, 44);
            this.brittish.TabIndex = 1;
            this.brittish.TabStop = false;
            this.brittish.Tag = "Brittish";
            this.brittish.Click += new System.EventHandler(this.brittish_Click);
            // 
            // dutch
            // 
            this.dutch.BackgroundImage = global::GitUI.Properties.Resources.Dutch1;
            this.dutch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.dutch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.dutch.Location = new System.Drawing.Point(3, 3);
            this.dutch.Name = "dutch";
            this.dutch.Size = new System.Drawing.Size(94, 44);
            this.dutch.TabIndex = 2;
            this.dutch.TabStop = false;
            this.dutch.Tag = "Dutch";
            this.dutch.Click += new System.EventHandler(this.dutch_Click);
            // 
            // italian
            // 
            this.italian.BackgroundImage = global::GitUI.Properties.Resources.Italiano;
            this.italian.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.italian.Cursor = System.Windows.Forms.Cursors.Hand;
            this.italian.Location = new System.Drawing.Point(203, 3);
            this.italian.Name = "italian";
            this.italian.Size = new System.Drawing.Size(94, 44);
            this.italian.TabIndex = 3;
            this.italian.TabStop = false;
            this.italian.Tag = "Italian";
            this.italian.Click += new System.EventHandler(this.italian_Click);
            // 
            // japanese
            // 
            this.japanese.BackgroundImage = global::GitUI.Properties.Resources.Japanese;
            this.japanese.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.japanese.Cursor = System.Windows.Forms.Cursors.Hand;
            this.japanese.Location = new System.Drawing.Point(303, 3);
            this.japanese.Name = "japanese";
            this.japanese.Size = new System.Drawing.Size(94, 44);
            this.japanese.TabIndex = 4;
            this.japanese.TabStop = false;
            this.japanese.Tag = "Japanese";
            this.japanese.Click += new System.EventHandler(this.japanese_Click);
            // 
            // spanish
            // 
            this.spanish.BackgroundImage = global::GitUI.Properties.Resources.Spanish;
            this.spanish.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.spanish.Cursor = System.Windows.Forms.Cursors.Hand;
            this.spanish.Location = new System.Drawing.Point(3, 53);
            this.spanish.Name = "spanish";
            this.spanish.Size = new System.Drawing.Size(94, 44);
            this.spanish.TabIndex = 5;
            this.spanish.TabStop = false;
            this.spanish.Tag = "Spanish";
            this.spanish.Click += new System.EventHandler(this.spanish_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.Controls.Add(this.brittish, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.dutch, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.italian, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.japanese, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.spanish, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(15, 34);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(400, 100);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 146);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(308, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "You can change the language at any time in the settings dialog";
            // 
            // FormChooseTranslation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 168);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormChooseTranslation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Choose language";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormChooseTranslation_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.brittish)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dutch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.italian)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.japanese)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spanish)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox brittish;
        private System.Windows.Forms.PictureBox dutch;
        private System.Windows.Forms.PictureBox italian;
        private System.Windows.Forms.PictureBox japanese;
        private System.Windows.Forms.PictureBox spanish;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
    }
}