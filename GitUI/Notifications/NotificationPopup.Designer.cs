namespace GitUI.Notifications
{
    partial class NotificationPopup
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOldest = new System.Windows.Forms.PictureBox();
            this.btnOlder = new System.Windows.Forms.PictureBox();
            this.btnNewer = new System.Windows.Forms.PictureBox();
            this.btnNewest = new System.Windows.Forms.PictureBox();
            this.btnDiscard = new System.Windows.Forms.PictureBox();
            this.txtText = new System.Windows.Forms.TextBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.imgIcon = new System.Windows.Forms.PictureBox();
            this.btnXofY = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnOldest)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOlder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnNewer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnNewest)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDiscard)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.btnXofY);
            this.flowLayoutPanel1.Controls.Add(this.btnOldest);
            this.flowLayoutPanel1.Controls.Add(this.btnOlder);
            this.flowLayoutPanel1.Controls.Add(this.btnNewer);
            this.flowLayoutPanel1.Controls.Add(this.btnNewest);
            this.flowLayoutPanel1.Controls.Add(this.btnDiscard);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(248, 29);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // btnOldest
            // 
            this.btnOldest.Location = new System.Drawing.Point(84, 3);
            this.btnOldest.Name = "btnOldest";
            this.btnOldest.Size = new System.Drawing.Size(25, 16);
            this.btnOldest.TabIndex = 6;
            this.btnOldest.TabStop = false;
            this.toolTip.SetToolTip(this.btnOldest, "Oldest");
            // 
            // btnOlder
            // 
            this.btnOlder.Location = new System.Drawing.Point(115, 3);
            this.btnOlder.Name = "btnOlder";
            this.btnOlder.Size = new System.Drawing.Size(25, 16);
            this.btnOlder.TabIndex = 2;
            this.btnOlder.TabStop = false;
            this.toolTip.SetToolTip(this.btnOlder, "Older");
            // 
            // btnNewer
            // 
            this.btnNewer.Location = new System.Drawing.Point(146, 3);
            this.btnNewer.Name = "btnNewer";
            this.btnNewer.Size = new System.Drawing.Size(25, 16);
            this.btnNewer.TabIndex = 1;
            this.btnNewer.TabStop = false;
            this.toolTip.SetToolTip(this.btnNewer, "Newer");
            // 
            // btnNewest
            // 
            this.btnNewest.Location = new System.Drawing.Point(177, 3);
            this.btnNewest.Name = "btnNewest";
            this.btnNewest.Size = new System.Drawing.Size(25, 16);
            this.btnNewest.TabIndex = 5;
            this.btnNewest.TabStop = false;
            this.toolTip.SetToolTip(this.btnNewest, "Newest");
            // 
            // btnDiscard
            // 
            this.btnDiscard.Location = new System.Drawing.Point(208, 3);
            this.btnDiscard.Name = "btnDiscard";
            this.btnDiscard.Size = new System.Drawing.Size(25, 16);
            this.btnDiscard.TabIndex = 3;
            this.btnDiscard.TabStop = false;
            // 
            // txtText
            // 
            this.txtText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtText.Location = new System.Drawing.Point(3, 70);
            this.txtText.Name = "txtText";
            this.txtText.ReadOnly = true;
            this.txtText.Size = new System.Drawing.Size(258, 25);
            this.txtText.TabIndex = 1;
            // 
            // imgIcon
            // 
            this.imgIcon.Location = new System.Drawing.Point(12, 28);
            this.imgIcon.Name = "imgIcon";
            this.imgIcon.Size = new System.Drawing.Size(16, 16);
            this.imgIcon.TabIndex = 4;
            this.imgIcon.TabStop = false;
            // 
            // btnXofY
            // 
            this.btnXofY.FlatAppearance.BorderSize = 0;
            this.btnXofY.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnXofY.Location = new System.Drawing.Point(3, 3);
            this.btnXofY.Name = "btnXofY";
            this.btnXofY.Size = new System.Drawing.Size(75, 23);
            this.btnXofY.TabIndex = 7;
            this.btnXofY.Text = "3 of 5";
            this.btnXofY.UseVisualStyleBackColor = true;
            // 
            // NotificationPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(248, 112);
            this.Controls.Add(this.imgIcon);
            this.Controls.Add(this.txtText);
            this.Controls.Add(this.flowLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotificationPopup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnOldest)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOlder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnNewer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnNewest)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDiscard)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.PictureBox btnNewer;
        private System.Windows.Forms.PictureBox btnOlder;
        private System.Windows.Forms.PictureBox btnDiscard;
        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.PictureBox btnOldest;
        private System.Windows.Forms.PictureBox btnNewest;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.PictureBox imgIcon;
        private System.Windows.Forms.Button btnXofY;
    }
}
