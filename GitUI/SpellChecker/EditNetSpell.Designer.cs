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
            this.components = new System.ComponentModel.Container();
            this.SpellCheckContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SpellCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.TextBox = new System.Windows.Forms.RichTextBox();
            this.AutoComplete = new System.Windows.Forms.ListBox();
            this.AutoCompleteTimer = new System.Windows.Forms.Timer(this.components);
            this.AutoCompleteToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.AutoCompleteToolTipTimer = new System.Windows.Forms.Timer(this.components);
            this.SpellCheckContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // SpellCheckContextMenu
            // 
            this.SpellCheckContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1});
            this.SpellCheckContextMenu.Name = "SpellCheckContextMenu";
            this.SpellCheckContextMenu.Size = new System.Drawing.Size(61, 10);
            this.SpellCheckContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.SpellCheckContextMenuOpening);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(57, 6);
            // 
            // SpellCheckTimer
            // 
            this.SpellCheckTimer.Interval = 250;
            this.SpellCheckTimer.Tick += new System.EventHandler(this.SpellCheckTimerTick);
            this.SpellCheckTimer.Enabled = false;
            // 
            // TextBox
            // 
            this.TextBox.AcceptsTab = true;
            this.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBox.ContextMenuStrip = this.SpellCheckContextMenu;
            this.TextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextBox.Location = new System.Drawing.Point(0, 0);
            this.TextBox.Margin = new System.Windows.Forms.Padding(0);
            this.TextBox.Name = "TextBox";
            this.TextBox.Size = new System.Drawing.Size(386, 336);
            this.TextBox.TabIndex = 1;
            this.TextBox.Text = "";
            this.TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyDown);
            this.TextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            this.TextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyUp);
            this.TextBox.Leave += new System.EventHandler(this.TextBoxLeave);
            this.TextBox.GotFocus += TextBox_GotFocus;
            this.TextBox.LostFocus += TextBox_LostFocus;
            // 
            // AutoComplete
            // 
            this.AutoComplete.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AutoComplete.FormattingEnabled = true;
            this.AutoComplete.ItemHeight = 15;
            this.AutoComplete.Location = new System.Drawing.Point(167, 243);
            this.AutoComplete.Name = "AutoComplete";
            this.AutoComplete.Size = new System.Drawing.Size(120, 92);
            this.AutoComplete.Sorted = true;
            this.AutoComplete.TabIndex = 2;
            this.AutoComplete.Visible = false;
            this.AutoComplete.Click += new System.EventHandler(this.AutoComplete_Click);
            // 
            // AutoCompleteTimer
            // 
            this.AutoCompleteTimer.Interval = 200;
            this.AutoCompleteTimer.Tick += new System.EventHandler(this.AutoCompleteTimer_Tick);
            // 
            // AutoCompleteToolTipTimer
            // 
            this.AutoCompleteToolTipTimer.Interval = 2000;
            this.AutoCompleteToolTipTimer.Tick += new System.EventHandler(this.AutoCompleteToolTipTimer_Tick);
            // 
            // EditNetSpell
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.AutoComplete);
            this.Controls.Add(this.TextBox);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "EditNetSpell";
            this.Size = new System.Drawing.Size(386, 336);
            this.SpellCheckContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip SpellCheckContextMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Timer SpellCheckTimer;
        private System.Windows.Forms.RichTextBox TextBox;
        private System.Windows.Forms.ListBox AutoComplete;
        private System.Windows.Forms.Timer AutoCompleteTimer;
        private System.Windows.Forms.ToolTip AutoCompleteToolTip;
        private System.Windows.Forms.Timer AutoCompleteToolTipTimer;
    }
}
