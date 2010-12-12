namespace GitUI
{
    partial class FormOpenInCustomMergeTool
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
            this._NO_TRANSLATE_description = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.mergeTool = new System.Windows.Forms.ComboBox();
            this.browse = new System.Windows.Forms.Button();
            this.merge = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.mergeToolArguments = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // _NO_TRANSLATE_description
            // 
            this._NO_TRANSLATE_description.AutoSize = true;
            this._NO_TRANSLATE_description.Location = new System.Drawing.Point(15, 9);
            this._NO_TRANSLATE_description.Name = "_NO_TRANSLATE_description";
            this._NO_TRANSLATE_description.Size = new System.Drawing.Size(19, 13);
            this._NO_TRANSLATE_description.TabIndex = 0;
            this._NO_TRANSLATE_description.Text = "...";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Choose mergetool";
            // 
            // mergeTool
            // 
            this.mergeTool.FormattingEnabled = true;
            this.mergeTool.Location = new System.Drawing.Point(123, 44);
            this.mergeTool.Name = "mergeTool";
            this.mergeTool.Size = new System.Drawing.Size(264, 21);
            this.mergeTool.TabIndex = 2;
            // 
            // browse
            // 
            this.browse.Location = new System.Drawing.Point(393, 43);
            this.browse.Name = "browse";
            this.browse.Size = new System.Drawing.Size(75, 23);
            this.browse.TabIndex = 3;
            this.browse.Text = "Browse";
            this.browse.UseVisualStyleBackColor = true;
            this.browse.Click += new System.EventHandler(this.browse_Click);
            // 
            // merge
            // 
            this.merge.Location = new System.Drawing.Point(393, 160);
            this.merge.Name = "merge";
            this.merge.Size = new System.Drawing.Size(75, 23);
            this.merge.TabIndex = 4;
            this.merge.Text = "Merge";
            this.merge.UseVisualStyleBackColor = true;
            this.merge.Click += new System.EventHandler(this.merge_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(241, 65);
            this.label2.TabIndex = 5;
            this.label2.Text = "You can use the following tags in the arguments:\r\nBase file name: $BASE\r\nLocal fi" +
                "le name: $LOCAL\r\nRemote file name: $REMOTE\r\nMerge result file name: $MERGED";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Arguments";
            // 
            // mergeToolArguments
            // 
            this.mergeToolArguments.FormattingEnabled = true;
            this.mergeToolArguments.Items.AddRange(new object[] {
            "\"$BASE\" \"$LOCAL\" \"$REMOTE\" -o \"$MERGED\"",
            "\"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"",
            "\"$LOCAL\" \"$REMOTE\" \"$MERGED\""});
            this.mergeToolArguments.Location = new System.Drawing.Point(123, 75);
            this.mergeToolArguments.Name = "mergeToolArguments";
            this.mergeToolArguments.Size = new System.Drawing.Size(264, 21);
            this.mergeToolArguments.TabIndex = 7;
            // 
            // FormOpenInCustomMergeTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 195);
            this.Controls.Add(this.mergeToolArguments);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.merge);
            this.Controls.Add(this.browse);
            this.Controls.Add(this.mergeTool);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._NO_TRANSLATE_description);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormOpenInCustomMergeTool";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Merge";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _NO_TRANSLATE_description;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox mergeTool;
        private System.Windows.Forms.Button browse;
        private System.Windows.Forms.Button merge;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox mergeToolArguments;
    }
}