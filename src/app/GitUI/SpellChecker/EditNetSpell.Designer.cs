namespace GitUI.SpellChecker
{
    partial class EditNetSpell
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SpellCheckContextMenu = new ContextMenuStrip(components);
            toolStripSeparator1 = new ToolStripSeparator();
            SpellCheckTimer = new System.Windows.Forms.Timer(components);
            TextBox = new RichTextBox();
            AutoComplete = new ListBox();
            AutoCompleteTimer = new System.Windows.Forms.Timer(components);
            AutoCompleteToolTip = new ToolTip(components);
            AutoCompleteToolTipTimer = new System.Windows.Forms.Timer(components);
            SpellCheckContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // SpellCheckContextMenu
            // 
            SpellCheckContextMenu.Items.AddRange(new ToolStripItem[] {
            toolStripSeparator1});
            SpellCheckContextMenu.Name = "SpellCheckContextMenu";
            SpellCheckContextMenu.Size = new Size(61, 10);
            SpellCheckContextMenu.Opening += SpellCheckContextMenuOpening;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(57, 6);
            // 
            // SpellCheckTimer
            // 
            SpellCheckTimer.Interval = 250;
            SpellCheckTimer.Tick += SpellCheckTimerTick;
            SpellCheckTimer.Enabled = false;
            // 
            // TextBox
            // 
            TextBox.AcceptsTab = true;
            TextBox.BorderStyle = BorderStyle.None;
            TextBox.ContextMenuStrip = SpellCheckContextMenu;
            TextBox.Dock = DockStyle.Fill;
            TextBox.Location = new Point(0, 0);
            TextBox.Margin = new Padding(0);
            TextBox.Name = "TextBox";
            TextBox.Size = new Size(386, 336);
            TextBox.TabIndex = 1;
            TextBox.Text = "";
            TextBox.KeyDown += TextBox_KeyDown;
            TextBox.KeyPress += TextBox_KeyPress;
            TextBox.KeyUp += TextBox_KeyUp;
            TextBox.Leave += TextBoxLeave;
            TextBox.GotFocus += TextBox_GotFocus;
            TextBox.LostFocus += TextBox_LostFocus;
            TextBox.MouseDown += TextBox_MouseDown;
            TextBox.WordWrap = false;
            // 
            // AutoComplete
            // 
            AutoComplete.BorderStyle = BorderStyle.FixedSingle;
            AutoComplete.FormattingEnabled = true;
            AutoComplete.ItemHeight = 15;
            AutoComplete.Location = new Point(167, 243);
            AutoComplete.Name = "AutoComplete";
            AutoComplete.Size = new Size(120, 92);
            AutoComplete.Sorted = true;
            AutoComplete.TabIndex = 2;
            AutoComplete.Visible = false;
            AutoComplete.Click += AutoComplete_Click;
            // 
            // AutoCompleteTimer
            // 
            AutoCompleteTimer.Interval = 200;
            AutoCompleteTimer.Tick += AutoCompleteTimer_Tick;
            // 
            // AutoCompleteToolTipTimer
            // 
            AutoCompleteToolTipTimer.Interval = 2000;
            AutoCompleteToolTipTimer.Tick += AutoCompleteToolTipTimer_Tick;
            // 
            // EditNetSpell
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(AutoComplete);
            Controls.Add(TextBox);
            Margin = new Padding(0);
            Name = "EditNetSpell";
            Size = new Size(386, 336);
            SpellCheckContextMenu.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private ContextMenuStrip SpellCheckContextMenu;
        private ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Timer SpellCheckTimer;
        private RichTextBox TextBox;
        private ListBox AutoComplete;
        private System.Windows.Forms.Timer AutoCompleteTimer;
        private ToolTip AutoCompleteToolTip;
        private System.Windows.Forms.Timer AutoCompleteToolTipTimer;
    }
}
