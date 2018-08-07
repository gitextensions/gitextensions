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
            this.annotate = new System.Windows.Forms.ComboBox();
            this.keyIdLbl = new System.Windows.Forms.Label();
            this.textBoxGpgKey = new System.Windows.Forms.TextBox();
            this.pushTag = new System.Windows.Forms.CheckBox();
            this.tagMessage = new GitUI.SpellChecker.EditNetSpell();
            this.label2 = new System.Windows.Forms.Label();
            this.ForceTag = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.commitPickerSmallControl1 = new GitUI.UserControls.CommitPickerSmallControl();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "Tag name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Image = global::GitUI.Properties.Images.TagCreate;
            this.Ok.Location = new System.Drawing.Point(317, 200);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(128, 26);
            this.Ok.TabIndex = 10;
            this.Ok.Text = "Create tag";
            this.Ok.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Ok.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // textBoxTagName
            // 
            this.textBoxTagName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTagName.Location = new System.Drawing.Point(141, 3);
            this.textBoxTagName.Name = "textBoxTagName";
            this.textBoxTagName.Size = new System.Drawing.Size(304, 21);
            this.textBoxTagName.TabIndex = 1;
            // 
            // annotate
            // 
            this.annotate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.annotate.Location = new System.Drawing.Point(141, 87);
            this.annotate.Name = "annotate";
            this.annotate.Size = new System.Drawing.Size(150, 21);
            this.annotate.TabIndex = 5;
            this.annotate.SelectedIndexChanged += new System.EventHandler(this.AnnotateDropDownChanged);
            // 
            // keyIdLbl
            // 
            this.keyIdLbl.AutoSize = true;
            this.keyIdLbl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.keyIdLbl.Enabled = false;
            this.keyIdLbl.Location = new System.Drawing.Point(3, 112);
            this.keyIdLbl.Name = "keyIdLbl";
            this.keyIdLbl.Size = new System.Drawing.Size(132, 28);
            this.keyIdLbl.TabIndex = 6;
            this.keyIdLbl.Text = "Specific Key Id";
            this.keyIdLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxGpgKey
            // 
            this.textBoxGpgKey.Enabled = false;
            this.textBoxGpgKey.Location = new System.Drawing.Point(141, 115);
            this.textBoxGpgKey.MaxLength = 8;
            this.textBoxGpgKey.Name = "textBoxGpgKey";
            this.textBoxGpgKey.Size = new System.Drawing.Size(60, 21);
            this.textBoxGpgKey.TabIndex = 7;
            // 
            // pushTag
            // 
            this.pushTag.AutoSize = true;
            this.pushTag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pushTag.Location = new System.Drawing.Point(141, 59);
            this.pushTag.Name = "pushTag";
            this.pushTag.Size = new System.Drawing.Size(304, 22);
            this.pushTag.TabIndex = 4;
            this.pushTag.Text = "Push tag to \'{0}\'";
            this.pushTag.UseVisualStyleBackColor = true;
            // 
            // tagMessage
            // 
            this.tagMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tagMessage.Enabled = false;
            this.tagMessage.Location = new System.Drawing.Point(140, 142);
            this.tagMessage.Margin = new System.Windows.Forms.Padding(2);
            this.tagMessage.Name = "tagMessage";
            this.tagMessage.Size = new System.Drawing.Size(327, 75);
            this.tagMessage.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 140);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label2.Size = new System.Drawing.Size(132, 57);
            this.label2.TabIndex = 8;
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
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 33);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(132, 23);
            this.label3.TabIndex = 2;
            this.label3.Text = "Create tag at this revision";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // commitPickerSmallControl1
            // 
            this.commitPickerSmallControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitPickerSmallControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commitPickerSmallControl1.Location = new System.Drawing.Point(141, 31);
            this.commitPickerSmallControl1.MinimumSize = new System.Drawing.Size(100, 26);
            this.commitPickerSmallControl1.Name = "commitPickerSmallControl1";
            this.commitPickerSmallControl1.Size = new System.Drawing.Size(304, 26);
            this.commitPickerSmallControl1.TabIndex = 3;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.Ok, 1, 6);
            this.tableLayoutPanel2.Controls.Add(this.tagMessage, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.commitPickerSmallControl1, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.textBoxGpgKey, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.keyIdLbl, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.annotate, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.pushTag, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.textBoxTagName, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(8, 8);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 7;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(448, 229);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // FormCreateTag
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(464, 245);
            this.Controls.Add(this.tableLayoutPanel2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(455, 260);
            this.Name = "FormCreateTag";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create tag";
            this.Load += new System.EventHandler(this.FormCreateTag_Load);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox textBoxTagName;
        private System.Windows.Forms.ComboBox annotate;
        private System.Windows.Forms.Label keyIdLbl;
        private System.Windows.Forms.TextBox textBoxGpgKey;
        private System.Windows.Forms.CheckBox pushTag;
        private EditNetSpell tagMessage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox ForceTag;
        private System.Windows.Forms.Label label3;
        private UserControls.CommitPickerSmallControl commitPickerSmallControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}