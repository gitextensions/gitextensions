using System.Windows.Forms;
namespace GitUI.SpellChecker
{
    partial class EditNetSpell
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
            this.SpellCheckContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SpellCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.TextBox = new System.Windows.Forms.RichTextBox();
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
            // 
            // TextBox
            // 
            this.TextBox.AcceptsTab = true;
            this.TextBox.ContextMenuStrip = this.SpellCheckContextMenu;
            this.TextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextBox.Location = new System.Drawing.Point(0, 0);
            this.TextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.TextBox.Name = "TextBox";
            this.TextBox.Size = new System.Drawing.Size(441, 448);
            this.TextBox.TabIndex = 1;
            this.TextBox.Text = "";
            this.TextBox.SelectionChanged += new System.EventHandler(this.TextBox_SelectionChanged);
            this.TextBox.SizeChanged += new System.EventHandler(this.TextBoxSizeChanged);
            this.TextBox.TextChanged += new System.EventHandler(this.TextBoxTextChanged);
            this.TextBox.Enter += new System.EventHandler(this.TextBox_Enter);
            this.TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyDown);
            this.TextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            this.TextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyUp);
            this.TextBox.Leave += new System.EventHandler(this.TextBoxLeave);
            // 
            // EditNetSpell
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TextBox);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "EditNetSpell";
            this.Size = new System.Drawing.Size(441, 448);
            this.Load += new System.EventHandler(this.EditNetSpellLoad);
            this.SpellCheckContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip SpellCheckContextMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Timer SpellCheckTimer;
        private System.Windows.Forms.RichTextBox TextBox;
    }
}
