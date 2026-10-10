namespace GitUI.CommandsDialogs
{
    partial class FormToolbarsLayout
    {
        // Required designer variable.
        private System.ComponentModel.IContainer components = null;

        // Clean up any resources being used.
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

        // Required method for Designer support - do not modify
        // the contents of this method with the code editor.
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panelToolbarGrid = new System.Windows.Forms.Panel();
            this.labelInstructions = new System.Windows.Forms.Label();
            this.checkBoxSyncIconText = new System.Windows.Forms.CheckBox();
            this.buttonAddRow = new System.Windows.Forms.Button();
            this.buttonRemoveRow = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonLocate = new System.Windows.Forms.Button();
            this.flowLayoutPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelInstructions
            // 
            this.labelInstructions.AutoSize = true;
            this.labelInstructions.Location = new System.Drawing.Point(12, 9);
            this.labelInstructions.Name = "labelInstructions";
            this.labelInstructions.Size = new System.Drawing.Size(400, 13);
            this.labelInstructions.TabIndex = 0;
            this.labelInstructions.Text = "Drag toolbars to reposition them. Drop between rows to create a new row.";
            // 
            // panelToolbarGrid
            // 
            this.panelToolbarGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelToolbarGrid.AutoScroll = true;
            this.panelToolbarGrid.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panelToolbarGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelToolbarGrid.Location = new System.Drawing.Point(12, 35);
            this.panelToolbarGrid.Name = "panelToolbarGrid";
            this.panelToolbarGrid.Size = new System.Drawing.Size(710, 320);
            this.panelToolbarGrid.TabIndex = 1;
            //
            // checkBoxSyncIconText
            //
            this.checkBoxSyncIconText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxSyncIconText.AutoSize = true;
            this.checkBoxSyncIconText.Location = new System.Drawing.Point(12, 365);
            this.checkBoxSyncIconText.Name = "checkBoxSyncIconText";
            this.checkBoxSyncIconText.Size = new System.Drawing.Size(500, 19);
            this.checkBoxSyncIconText.TabIndex = 9;
            this.checkBoxSyncIconText.Text = "Sync icon text with icon size (increases icon text proportionally to icon size)";
            this.checkBoxSyncIconText.UseVisualStyleBackColor = true;
            //
            // buttonAddRow
            //
            this.buttonAddRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddRow.Location = new System.Drawing.Point(12, 395);
            this.buttonAddRow.Name = "buttonAddRow";
            this.buttonAddRow.Size = new System.Drawing.Size(100, 28);
            this.buttonAddRow.TabIndex = 2;
            this.buttonAddRow.Text = "+ Add Row";
            this.buttonAddRow.UseVisualStyleBackColor = true;
            this.buttonAddRow.Click += new System.EventHandler(this.ButtonAddRow_Click);
            // 
            // buttonRemoveRow
            // 
            this.buttonRemoveRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemoveRow.Enabled = false;
            this.buttonRemoveRow.Location = new System.Drawing.Point(118, 395);
            this.buttonRemoveRow.Name = "buttonRemoveRow";
            this.buttonRemoveRow.Size = new System.Drawing.Size(130, 28);
            this.buttonRemoveRow.TabIndex = 3;
            this.buttonRemoveRow.Text = "- Remove Empty Row";
            this.buttonRemoveRow.UseVisualStyleBackColor = true;
            this.buttonRemoveRow.Click += new System.EventHandler(this.ButtonRemoveRow_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonReset.Location = new System.Drawing.Point(260, 395);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(100, 28);
            this.buttonReset.TabIndex = 7;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.ButtonReset_Click);
            // 
            // buttonLocate
            // 
            this.buttonLocate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLocate.Location = new System.Drawing.Point(366, 395);
            this.buttonLocate.Name = "buttonLocate";
            this.buttonLocate.Size = new System.Drawing.Size(100, 28);
            this.buttonLocate.TabIndex = 8;
            this.buttonLocate.Text = "Locate";
            this.buttonLocate.UseVisualStyleBackColor = true;
            this.buttonLocate.Click += new System.EventHandler(this.ButtonLocate_Click);
            //
            // buttonOK
            //
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 28);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 28);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            //
            // buttonApply
            //
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 28);
            this.buttonApply.TabIndex = 6;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.ButtonApply_Click);
            //
            // flowLayoutPanelButtons
            //
            // Size = 3 buttons * (75 width + 3 left margin + 3 right margin) = 243 wide; height = 28 + 3 + 3 = 34.
            // Location.X = ClientSize.Width 734 - 12 right margin - 243 = 479, so the panel's right edge
            // sits 12 px from the form's right edge (matching panelToolbarGrid). AutoSize is OFF on purpose:
            // when true, the panel re-grows after the Anchor offset is computed and the right edge drifts.
            this.flowLayoutPanelButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanelButtons.Controls.Add(this.buttonApply);
            this.flowLayoutPanelButtons.Controls.Add(this.buttonCancel);
            this.flowLayoutPanelButtons.Controls.Add(this.buttonOK);
            this.flowLayoutPanelButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanelButtons.Location = new System.Drawing.Point(479, 391);
            this.flowLayoutPanelButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanelButtons.Name = "flowLayoutPanelButtons";
            this.flowLayoutPanelButtons.Size = new System.Drawing.Size(243, 34);
            this.flowLayoutPanelButtons.TabIndex = 10;
            this.flowLayoutPanelButtons.WrapContents = false;
            //
            // FormToolbarsLayout
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(734, 435);
            this.Controls.Add(this.buttonLocate);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.flowLayoutPanelButtons);
            this.Controls.Add(this.buttonRemoveRow);
            this.Controls.Add(this.buttonAddRow);
            this.Controls.Add(this.checkBoxSyncIconText);
            this.Controls.Add(this.panelToolbarGrid);
            this.Controls.Add(this.labelInstructions);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(834, 380);
            this.Name = "FormToolbarsLayout";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Toolbars Layout";
            this.flowLayoutPanelButtons.ResumeLayout(false);
            this.flowLayoutPanelButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Panel panelToolbarGrid;
        private System.Windows.Forms.Label labelInstructions;
        private System.Windows.Forms.CheckBox checkBoxSyncIconText;
        private System.Windows.Forms.Button buttonAddRow;
        private System.Windows.Forms.Button buttonRemoveRow;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonLocate;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelButtons;
    }
}
