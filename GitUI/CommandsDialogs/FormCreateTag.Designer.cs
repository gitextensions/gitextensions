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
            if (disposing && (components is not null))
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
            label1 = new Label();
            Ok = new Button();
            textBoxTagName = new TextBox();
            annotate = new ComboBox();
            keyIdLbl = new Label();
            textBoxGpgKey = new TextBox();
            pushTag = new CheckBox();
            tagMessage = new GitUI.SpellChecker.EditNetSpell();
            label2 = new Label();
            ForceTag = new CheckBox();
            label3 = new Label();
            commitPickerSmallControl1 = new GitUI.UserControls.CommitPickerSmallControl();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(132, 28);
            label1.TabIndex = 0;
            label1.Text = "Tag name";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Ok
            // 
            Ok.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Ok.Image = Properties.Images.TagCreate;
            Ok.Location = new Point(317, 200);
            Ok.Name = "Ok";
            Ok.Size = new Size(128, 26);
            Ok.TabIndex = 10;
            Ok.Text = "Create tag";
            Ok.TextAlign = ContentAlignment.MiddleRight;
            Ok.TextImageRelation = TextImageRelation.ImageBeforeText;
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += OkClick;
            // 
            // textBoxTagName
            // 
            textBoxTagName.Dock = DockStyle.Fill;
            textBoxTagName.Location = new Point(141, 3);
            textBoxTagName.Name = "textBoxTagName";
            textBoxTagName.Size = new Size(304, 21);
            textBoxTagName.TabIndex = 1;
            // 
            // annotate
            // 
            annotate.DropDownStyle = ComboBoxStyle.DropDownList;
            annotate.Location = new Point(141, 87);
            annotate.Name = "annotate";
            annotate.Size = new Size(150, 21);
            annotate.TabIndex = 5;
            annotate.SelectedIndexChanged += AnnotateDropDownChanged;
            // 
            // keyIdLbl
            // 
            keyIdLbl.AutoSize = true;
            keyIdLbl.Dock = DockStyle.Fill;
            keyIdLbl.Enabled = false;
            keyIdLbl.Location = new Point(3, 112);
            keyIdLbl.Name = "keyIdLbl";
            keyIdLbl.Size = new Size(132, 28);
            keyIdLbl.TabIndex = 6;
            keyIdLbl.Text = "Specific Key Id";
            keyIdLbl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // textBoxGpgKey
            // 
            textBoxGpgKey.Enabled = false;
            textBoxGpgKey.Location = new Point(141, 115);
            textBoxGpgKey.MaxLength = 16;
            textBoxGpgKey.Name = "textBoxGpgKey";
            textBoxGpgKey.Size = new Size(60, 21);
            textBoxGpgKey.TabIndex = 7;
            // 
            // pushTag
            // 
            pushTag.AutoSize = true;
            pushTag.Dock = DockStyle.Fill;
            pushTag.Location = new Point(141, 59);
            pushTag.Name = "pushTag";
            pushTag.Size = new Size(304, 22);
            pushTag.TabIndex = 4;
            pushTag.Text = "Push tag to \'{0}\'";
            pushTag.UseVisualStyleBackColor = true;
            // 
            // tagMessage
            // 
            tagMessage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tagMessage.Enabled = false;
            tagMessage.Location = new Point(140, 142);
            tagMessage.Margin = new Padding(2);
            tagMessage.Name = "tagMessage";
            tagMessage.Size = new Size(327, 75);
            tagMessage.TabIndex = 9;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(3, 140);
            label2.Name = "label2";
            label2.Padding = new Padding(0, 2, 0, 0);
            label2.Size = new Size(132, 57);
            label2.TabIndex = 8;
            label2.Text = "Message";
            // 
            // ForceTag
            // 
            ForceTag.AutoSize = true;
            ForceTag.Location = new Point(280, 34);
            ForceTag.Name = "ForceTag";
            ForceTag.Size = new Size(55, 19);
            ForceTag.TabIndex = 13;
            ForceTag.Text = "Force";
            ForceTag.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Location = new Point(3, 33);
            label3.Margin = new Padding(3, 5, 3, 0);
            label3.Name = "label3";
            label3.Size = new Size(132, 23);
            label3.TabIndex = 2;
            label3.Text = "Create tag at this revision";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // commitPickerSmallControl1
            // 
            commitPickerSmallControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            commitPickerSmallControl1.Dock = DockStyle.Fill;
            commitPickerSmallControl1.Location = new Point(141, 31);
            commitPickerSmallControl1.MinimumSize = new Size(100, 26);
            commitPickerSmallControl1.Name = "commitPickerSmallControl1";
            commitPickerSmallControl1.Size = new Size(304, 26);
            commitPickerSmallControl1.TabIndex = 3;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(Ok, 1, 6);
            tableLayoutPanel2.Controls.Add(tagMessage, 1, 5);
            tableLayoutPanel2.Controls.Add(label2, 0, 5);
            tableLayoutPanel2.Controls.Add(commitPickerSmallControl1, 1, 1);
            tableLayoutPanel2.Controls.Add(textBoxGpgKey, 1, 4);
            tableLayoutPanel2.Controls.Add(keyIdLbl, 0, 4);
            tableLayoutPanel2.Controls.Add(annotate, 1, 3);
            tableLayoutPanel2.Controls.Add(pushTag, 1, 2);
            tableLayoutPanel2.Controls.Add(label3, 0, 1);
            tableLayoutPanel2.Controls.Add(textBoxTagName, 1, 0);
            tableLayoutPanel2.Controls.Add(label1, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(8, 8);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 7;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tableLayoutPanel2.Size = new Size(448, 229);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // FormCreateTag
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(464, 245);
            Controls.Add(tableLayoutPanel2);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(455, 260);
            Name = "FormCreateTag";
            Padding = new Padding(8);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create tag";
            Load += FormCreateTag_Load;
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private Label label1;
        private Button Ok;
        private TextBox textBoxTagName;
        private ComboBox annotate;
        private Label keyIdLbl;
        private TextBox textBoxGpgKey;
        private CheckBox pushTag;
        private EditNetSpell tagMessage;
        private Label label2;
        private CheckBox ForceTag;
        private Label label3;
        private UserControls.CommitPickerSmallControl commitPickerSmallControl1;
        private TableLayoutPanel tableLayoutPanel2;
    }
}
