using GitUI.SpellChecker;

namespace GitUI.CommandsDialogs
{
    partial class FormCreateTag
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
            this.textBoxTagName = new System.Windows.Forms.TextBox();
            this.annotate = new System.Windows.Forms.CheckBox();
            this.pushTag = new System.Windows.Forms.CheckBox();
            this.tagMessage = new GitUI.SpellChecker.EditNetSpell();
            this.label2 = new System.Windows.Forms.Label();
            this.ForceTag = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.commitPickerSmallControl1 = new GitUI.UserControls.CommitPickerSmallControl();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Tag name";
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Image = global::GitUI.Properties.Resources.IconTagCreate;
            this.Ok.Location = new System.Drawing.Point(317, 11);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(108, 25);
            this.Ok.TabIndex = 6;
            this.Ok.Text = "Create tag";
            this.Ok.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Ok.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // textBoxTagName
            // 
            this.textBoxTagName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTagName.Location = new System.Drawing.Point(103, 12);
            this.textBoxTagName.Name = "textBoxTagName";
            this.textBoxTagName.Size = new System.Drawing.Size(208, 23);
            this.textBoxTagName.TabIndex = 1;
            // 
            // annotate
            // 
            this.annotate.AutoSize = true;
            this.annotate.Location = new System.Drawing.Point(103, 3);
            this.annotate.Name = "annotate";
            this.annotate.Size = new System.Drawing.Size(137, 19);
            this.annotate.TabIndex = 3;
            this.annotate.Text = "Create annotated tag";
            this.annotate.UseVisualStyleBackColor = true;
            this.annotate.CheckedChanged += new System.EventHandler(this.AnnotateCheckedChanged);
            // 
            // pushTag
            // 
            this.pushTag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pushTag.AutoSize = true;
            this.pushTag.Location = new System.Drawing.Point(313, 46);
            this.pushTag.Name = "pushTag";
            this.pushTag.Size = new System.Drawing.Size(109, 19);
            this.pushTag.TabIndex = 2;
            this.pushTag.Text = "Push tag to \'{0}\'";
            this.pushTag.UseVisualStyleBackColor = true;
            // 
            // tagMessage
            // 
            this.tagMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tagMessage.Enabled = false;
            this.tagMessage.Location = new System.Drawing.Point(103, 28);
            this.tagMessage.Margin = new System.Windows.Forms.Padding(2);
            this.tagMessage.Name = "tagMessage";
            this.tagMessage.Size = new System.Drawing.Size(322, 75);
            this.tagMessage.TabIndex = 5;
            this.tagMessage.TextBoxFont = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Message";
            // 
            // ForceTag
            // 
            this.ForceTag.AutoSize = true;
            this.ForceTag.Location = new System.Drawing.Point(280, 34);
            this.ForceTag.Name = "ForceTag";
            this.ForceTag.Size = new System.Drawing.Size(55, 19);
            this.ForceTag.TabIndex = 13;
            this.ForceTag.Text = "Force";
            this.ForceTag.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(434, 222);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.commitPickerSmallControl1);
            this.flowLayoutPanel1.Controls.Add(this.label4);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.MinimumSize = new System.Drawing.Size(50, 30);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(428, 30);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 5);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(140, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "Create tag at this revision";
            // 
            // commitPickerSmallControl1
            // 
            this.commitPickerSmallControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitPickerSmallControl1.Location = new System.Drawing.Point(149, 3);
            this.commitPickerSmallControl1.MinimumSize = new System.Drawing.Size(100, 26);
            this.commitPickerSmallControl1.Name = "commitPickerSmallControl1";
            this.commitPickerSmallControl1.Size = new System.Drawing.Size(129, 26);
            this.commitPickerSmallControl1.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(284, 5);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(129, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "or choose another one.";
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.textBoxTagName);
            this.panel1.Controls.Add(this.Ok);
            this.panel1.Controls.Add(this.pushTag);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 39);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(428, 69);
            this.panel1.TabIndex = 3;
            // 
            // panel2
            // 
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.tagMessage);
            this.panel2.Controls.Add(this.annotate);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 114);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(428, 105);
            this.panel2.TabIndex = 4;
            // 
            // FormCreateTagAtRevision
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(434, 222);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(450, 260);
            this.Name = "FormCreateTagAtRevision";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create tag";
            this.Load += new System.EventHandler(this.FormCreateTag_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox textBoxTagName;
        private System.Windows.Forms.CheckBox annotate;
        private System.Windows.Forms.CheckBox pushTag;
        private EditNetSpell tagMessage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox ForceTag;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label3;
        private UserControls.CommitPickerSmallControl commitPickerSmallControl1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
    }
}