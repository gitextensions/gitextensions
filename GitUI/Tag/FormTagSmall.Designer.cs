﻿using GitUI.SpellChecker;

namespace GitUI.Tag
{
    partial class FormTagSmall
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
            this.Ok = new System.Windows.Forms.Button();
            this.TName = new System.Windows.Forms.TextBox();
            this.annotate = new System.Windows.Forms.CheckBox();
            this.pushTag = new System.Windows.Forms.CheckBox();
            this.tagMessage = new GitUI.SpellChecker.EditNetSpell();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "Tag name";
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(341, 5);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(108, 23);
            this.Ok.TabIndex = 7;
            this.Ok.Text = "Create tag";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // TName
            // 
            this.TName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TName.Location = new System.Drawing.Point(108, 7);
            this.TName.Name = "TName";
            this.TName.Size = new System.Drawing.Size(227, 21);
            this.TName.TabIndex = 6;
            this.TName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.NameKeyUp);
            // 
            // annotate
            // 
            this.annotate.AutoSize = true;
            this.annotate.Location = new System.Drawing.Point(108, 53);
            this.annotate.Name = "annotate";
            this.annotate.Size = new System.Drawing.Size(116, 16);
            this.annotate.TabIndex = 9;
            this.annotate.Text = "Create annotated tag";
            this.annotate.UseVisualStyleBackColor = true;
            this.annotate.CheckedChanged += new System.EventHandler(this.AnnotateCheckedChanged);
            // 
            // pushTag
            // 
            this.pushTag.AutoSize = true;
            this.pushTag.Location = new System.Drawing.Point(108, 34);
            this.pushTag.Name = "pushTag";
            this.pushTag.Size = new System.Drawing.Size(63, 16);
            this.pushTag.TabIndex = 10;
            this.pushTag.Text = "Push tag";
            this.pushTag.UseVisualStyleBackColor = true;
            // 
            // tagMessage
            // 
            this.tagMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tagMessage.Enabled = false;
            this.tagMessage.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.tagMessage.Location = new System.Drawing.Point(108, 73);
            this.tagMessage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tagMessage.MistakeFont = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Underline);
            this.tagMessage.Name = "tagMessage";
            this.tagMessage.Size = new System.Drawing.Size(339, 82);
            this.tagMessage.TabIndex = 11;
            this.tagMessage.WatermarkText = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 12);
            this.label2.TabIndex = 12;
            this.label2.Text = "Message";
            // 
            // FormTagSmall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 167);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tagMessage);
            this.Controls.Add(this.annotate);
            this.Controls.Add(this.pushTag);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.TName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormTagSmall";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create tag";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox TName;
        private System.Windows.Forms.CheckBox annotate;
        private System.Windows.Forms.CheckBox pushTag;
        private EditNetSpell tagMessage;
        private System.Windows.Forms.Label label2;
    }
}