namespace GitUI
{
    partial class FormModifiedDeletedCreated
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormModifiedDeletedCreated));
            this.Label = new System.Windows.Forms.Label();
            this.Created = new System.Windows.Forms.Button();
            this.Deleted = new System.Windows.Forms.Button();
            this.Abort = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Label
            // 
            this.Label.AutoSize = true;
            this.Label.Location = new System.Drawing.Point(12, 22);
            this.Label.Name = "Label";
            this.Label.Size = new System.Drawing.Size(137, 13);
            this.Label.TabIndex = 0;
            this.Label.Text = "Use created or deleted file?";
            // 
            // Created
            // 
            this.Created.Location = new System.Drawing.Point(13, 56);
            this.Created.Name = "Created";
            this.Created.Size = new System.Drawing.Size(75, 23);
            this.Created.TabIndex = 1;
            this.Created.Text = "Created";
            this.Created.UseVisualStyleBackColor = true;
            this.Created.Click += new System.EventHandler(this.Created_Click);
            // 
            // Deleted
            // 
            this.Deleted.Location = new System.Drawing.Point(119, 56);
            this.Deleted.Name = "Deleted";
            this.Deleted.Size = new System.Drawing.Size(75, 23);
            this.Deleted.TabIndex = 2;
            this.Deleted.Text = "Deleted";
            this.Deleted.UseVisualStyleBackColor = true;
            this.Deleted.Click += new System.EventHandler(this.Deleted_Click);
            // 
            // Abort
            // 
            this.Abort.Location = new System.Drawing.Point(226, 56);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(75, 23);
            this.Abort.TabIndex = 3;
            this.Abort.Text = "Abort";
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.Abort_Click);
            // 
            // FormModifiedDeletedCreated
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(317, 95);
            this.Controls.Add(this.Abort);
            this.Controls.Add(this.Deleted);
            this.Controls.Add(this.Created);
            this.Controls.Add(this.Label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormModifiedDeletedCreated";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Solve mergeconflict";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label Label;
        private System.Windows.Forms.Button Deleted;
        private System.Windows.Forms.Button Abort;
        public System.Windows.Forms.Button Created;
    }
}