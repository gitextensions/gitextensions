namespace GitUI.CommandsDialogs
{
    partial class SearchControl<T>
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
            _backgroundLoader.Cancel();

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
            listBoxSearchResult = new ListBox();
            txtSearchBox = new GitUI.UserControls.TextBoxEx();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // listBoxSearchResult
            // 
            listBoxSearchResult.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            listBoxSearchResult.BorderStyle = BorderStyle.FixedSingle;
            listBoxSearchResult.FormattingEnabled = true;
            listBoxSearchResult.Location = new Point(0, 20);
            listBoxSearchResult.Margin = new Padding(0);
            listBoxSearchResult.Name = "listBoxSearchResult";
            listBoxSearchResult.Size = new Size(64, 15);
            listBoxSearchResult.TabIndex = 2;
            listBoxSearchResult.DoubleClick += listBoxSearchResult_DoubleClick;
            // 
            // txtSearchBox
            // 
            txtSearchBox.Dock = DockStyle.Fill;
            txtSearchBox.Location = new Point(0, 0);
            txtSearchBox.Margin = new Padding(0);
            txtSearchBox.Name = "txtSearchBox";
            txtSearchBox.Size = new Size(64, 20);
            txtSearchBox.TabIndex = 1;
            txtSearchBox.TextChanged += txtSearchBox_TextChange;
            txtSearchBox.KeyDown += txtSearchBox_KeyDown;
            txtSearchBox.KeyUp += txtSearchBox_KeyUp;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(txtSearchBox, 0, 0);
            tableLayoutPanel1.Controls.Add(listBoxSearchResult, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(64, 23);
            tableLayoutPanel1.TabIndex = 8;
            // 
            // SearchControl
            // 
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.Transparent;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(0);
            MinimumSize = new Size(64, 23);
            Name = "SearchControl";
            Size = new Size(64, 23);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private ListBox listBoxSearchResult;
        private GitUI.UserControls.TextBoxEx txtSearchBox;
        private TableLayoutPanel tableLayoutPanel1;
    }
}
