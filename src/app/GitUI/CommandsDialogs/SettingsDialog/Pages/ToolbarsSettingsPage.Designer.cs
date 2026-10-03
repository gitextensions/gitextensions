namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

partial class ToolbarsSettingsPage
{
    // Required designer variable.
    private System.ComponentModel.IContainer components = null;

    // Clean up any resources being used.
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components is not null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    // Required method for Designer support - do not modify
    // the contents of this method with the code editor.
    private void InitializeComponent()
    {
        mainTableLayoutPanel = new TableLayoutPanel();
        topPanel = new Panel();
        labelToolbar = new Label();
        checkBoxToolbarVisible = new CheckBox();
        comboBoxToolbar = new ComboBox();
        buttonAddToolbar = new Button();
        buttonRemoveToolbar = new Button();
        labelPosition = new Label();
        buttonToolbarLayout = new Button();
        buttonLocateToolbar = new Button();
        middlePanel = new Panel();
        leftPanel = new Panel();
        labelAvailable = new Label();
        comboBoxCategory = new ComboBox();
        textBoxFilterAvailable = new TextBox();
        buttonClearAvailableFilter = new Button();
        listBoxAvailable = new ListBox();
        centerButtonsPanel = new Panel();
        buttonAddAll = new Button();
        buttonAdd = new Button();
        buttonRemove = new Button();
        buttonMoveUp = new Button();
        buttonMoveDown = new Button();
        buttonClearCurrent = new Button();
        buttonUndo = new Button();
        rightPanel = new Panel();
        labelShow = new Label();
        buttonShowIconText = new Button();
        buttonShowAllIconText = new Button();
        textBoxFilterCurrent = new TextBox();
        buttonClearCurrentFilter = new Button();
        listBoxCurrent = new ListBox();
        bottomPanel = new Panel();
        labelNoFormBrowse = new Label();
        mainTableLayoutPanel.SuspendLayout();
        topPanel.SuspendLayout();
        middlePanel.SuspendLayout();
        leftPanel.SuspendLayout();
        centerButtonsPanel.SuspendLayout();
        rightPanel.SuspendLayout();
        bottomPanel.SuspendLayout();
        SuspendLayout();
        // 
        // mainTableLayoutPanel
        // 
        mainTableLayoutPanel.ColumnCount = 1;
        mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainTableLayoutPanel.Controls.Add(topPanel, 0, 0);
        mainTableLayoutPanel.Controls.Add(middlePanel, 0, 1);
        mainTableLayoutPanel.Controls.Add(bottomPanel, 0, 2);
        mainTableLayoutPanel.Dock = DockStyle.Fill;
        mainTableLayoutPanel.Location = new Point(8, 8);
        mainTableLayoutPanel.Margin = new Padding(0);
        mainTableLayoutPanel.Name = "mainTableLayoutPanel";
        mainTableLayoutPanel.RowCount = 3;
        mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
        mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
        mainTableLayoutPanel.Size = new Size(784, 534);
        mainTableLayoutPanel.TabIndex = 0;
        // 
        // topPanel
        // 
        topPanel.Controls.Add(labelToolbar);
        topPanel.Controls.Add(checkBoxToolbarVisible);
        topPanel.Controls.Add(comboBoxToolbar);
        topPanel.Controls.Add(buttonAddToolbar);
        topPanel.Controls.Add(buttonRemoveToolbar);
        topPanel.Controls.Add(labelPosition);
        topPanel.Controls.Add(buttonToolbarLayout);
        topPanel.Controls.Add(buttonLocateToolbar);
        topPanel.Dock = DockStyle.Fill;
        topPanel.Location = new Point(0, 0);
        topPanel.Margin = new Padding(0);
        topPanel.Name = "topPanel";
        topPanel.Size = new Size(784, 45);
        topPanel.TabIndex = 0;
        // 
        // labelToolbar
        // 
        labelToolbar.AutoSize = true;
        labelToolbar.Location = new Point(3, 13);
        labelToolbar.Name = "labelToolbar";
        labelToolbar.Size = new Size(50, 15);
        labelToolbar.TabIndex = 0;
        labelToolbar.Text = "Toolbar:";
        // 
        // checkBoxToolbarVisible
        // 
        checkBoxToolbarVisible.AutoSize = true;
        checkBoxToolbarVisible.Checked = true;
        checkBoxToolbarVisible.CheckState = CheckState.Checked;
        checkBoxToolbarVisible.Location = new Point(75, 15);
        checkBoxToolbarVisible.Name = "checkBoxToolbarVisible";
        checkBoxToolbarVisible.Size = new Size(15, 14);
        checkBoxToolbarVisible.TabIndex = 1;
        checkBoxToolbarVisible.UseVisualStyleBackColor = true;
        checkBoxToolbarVisible.CheckedChanged += CheckBoxToolbarVisible_CheckedChanged;
        // 
        // comboBoxToolbar
        // 
        comboBoxToolbar.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBoxToolbar.FormattingEnabled = true;
        comboBoxToolbar.Location = new Point(100, 9);
        comboBoxToolbar.Name = "comboBoxToolbar";
        comboBoxToolbar.Size = new Size(190, 23);
        comboBoxToolbar.TabIndex = 2;
        comboBoxToolbar.SelectedIndexChanged += ComboBoxToolbar_SelectedIndexChanged;
        // 
        // buttonAddToolbar
        // 
        buttonAddToolbar.Image = global::GitUI.Properties.Images.RemoteAdd;
        buttonAddToolbar.Location = new Point(296, 7);
        buttonAddToolbar.Name = "buttonAddToolbar";
        buttonAddToolbar.Size = new Size(30, 28);
        buttonAddToolbar.TabIndex = 3;
        buttonAddToolbar.UseVisualStyleBackColor = true;
        buttonAddToolbar.Click += ButtonAddToolbar_Click;
        // 
        // buttonRemoveToolbar
        // 
        buttonRemoveToolbar.Image = global::GitUI.Properties.Images.RemoteDelete;
        buttonRemoveToolbar.Location = new Point(332, 7);
        buttonRemoveToolbar.Name = "buttonRemoveToolbar";
        buttonRemoveToolbar.Size = new Size(30, 28);
        buttonRemoveToolbar.TabIndex = 4;
        buttonRemoveToolbar.UseVisualStyleBackColor = true;
        buttonRemoveToolbar.Click += ButtonRemoveToolbar_Click;
        // 
        // labelPosition
        // 
        labelPosition.AutoSize = true;
        labelPosition.Location = new Point(463, 13);
        labelPosition.Name = "labelPosition";
        labelPosition.Size = new Size(117, 15);
        labelPosition.TabIndex = 5;
        labelPosition.Text = "Set toolbars position:";
        // 
        // buttonToolbarLayout
        // 
        buttonToolbarLayout.Location = new Point(667, 7);
        buttonToolbarLayout.Name = "buttonToolbarLayout";
        buttonToolbarLayout.Size = new Size(75, 28);
        buttonToolbarLayout.TabIndex = 6;
        buttonToolbarLayout.Text = "Layout";
        buttonToolbarLayout.UseVisualStyleBackColor = true;
        buttonToolbarLayout.Click += ButtonToolbarLayout_Click;
        // 
        // buttonLocateToolbar
        // 
        buttonLocateToolbar.Location = new Point(748, 7);
        buttonLocateToolbar.Name = "buttonLocateToolbar";
        buttonLocateToolbar.Size = new Size(75, 28);
        buttonLocateToolbar.TabIndex = 7;
        buttonLocateToolbar.Text = "Locate";
        buttonLocateToolbar.UseVisualStyleBackColor = true;
        buttonLocateToolbar.Click += ButtonLocateToolbar_Click;
        // 
        // middlePanel
        // 
        middlePanel.Controls.Add(leftPanel);
        middlePanel.Controls.Add(centerButtonsPanel);
        middlePanel.Controls.Add(rightPanel);
        middlePanel.Controls.Add(labelNoFormBrowse);
        middlePanel.Dock = DockStyle.Fill;
        middlePanel.Location = new Point(0, 45);
        middlePanel.Margin = new Padding(0);
        middlePanel.Name = "middlePanel";
        middlePanel.Size = new Size(784, 449);
        middlePanel.TabIndex = 1;
        // 
        // leftPanel
        // 
        leftPanel.Controls.Add(labelAvailable);
        leftPanel.Controls.Add(comboBoxCategory);
        leftPanel.Controls.Add(textBoxFilterAvailable);
        leftPanel.Controls.Add(buttonClearAvailableFilter);
        leftPanel.Controls.Add(listBoxAvailable);
        leftPanel.Dock = DockStyle.Left;
        leftPanel.Location = new Point(0, 0);
        leftPanel.Margin = new Padding(0);
        leftPanel.Name = "leftPanel";
        leftPanel.Size = new Size(370, 449);
        leftPanel.TabIndex = 0;
        // 
        // labelAvailable
        // 
        labelAvailable.AutoSize = true;
        labelAvailable.Location = new Point(3, 5);
        labelAvailable.Name = "labelAvailable";
        labelAvailable.Size = new Size(99, 15);
        labelAvailable.TabIndex = 0;
        labelAvailable.Text = "Available actions:";
        // 
        // comboBoxCategory
        // 
        comboBoxCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBoxCategory.FormattingEnabled = true;
        comboBoxCategory.Location = new Point(157, 2);
        comboBoxCategory.Name = "comboBoxCategory";
        comboBoxCategory.Size = new Size(205, 23);
        comboBoxCategory.TabIndex = 1;
        comboBoxCategory.SelectedIndexChanged += ComboBoxCategory_SelectedIndexChanged;
        // 
        // textBoxFilterAvailable
        // 
        textBoxFilterAvailable.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        textBoxFilterAvailable.Location = new Point(3, 40);
        textBoxFilterAvailable.Name = "textBoxFilterAvailable";
        textBoxFilterAvailable.PlaceholderText = "Filter...";
        textBoxFilterAvailable.Size = new Size(332, 23);
        textBoxFilterAvailable.TabIndex = 2;
        textBoxFilterAvailable.TextChanged += TextBoxFilterAvailable_TextChanged;
        // 
        // buttonClearAvailableFilter
        // 
        buttonClearAvailableFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        buttonClearAvailableFilter.Location = new Point(338, 39);
        buttonClearAvailableFilter.Name = "buttonClearAvailableFilter";
        buttonClearAvailableFilter.Size = new Size(25, 25);
        buttonClearAvailableFilter.TabIndex = 3;
        buttonClearAvailableFilter.Text = "✕";
        buttonClearAvailableFilter.UseVisualStyleBackColor = true;
        buttonClearAvailableFilter.Click += ButtonClearAvailableFilter_Click;
        // 
        // listBoxAvailable
        // 
        listBoxAvailable.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listBoxAvailable.DrawMode = DrawMode.OwnerDrawFixed;
        listBoxAvailable.FormattingEnabled = true;
        listBoxAvailable.ItemHeight = 24;
        listBoxAvailable.Location = new Point(3, 68);
        listBoxAvailable.Name = "listBoxAvailable";
        listBoxAvailable.Size = new Size(360, 374);
        listBoxAvailable.TabIndex = 4;
        listBoxAvailable.DrawItem += ListBox_DrawItem;
        listBoxAvailable.MouseDoubleClick += ListBoxAvailable_MouseDoubleClick;
        // 
        // centerButtonsPanel
        // 
        centerButtonsPanel.Controls.Add(buttonAddAll);
        centerButtonsPanel.Controls.Add(buttonAdd);
        centerButtonsPanel.Controls.Add(buttonRemove);
        centerButtonsPanel.Controls.Add(buttonMoveUp);
        centerButtonsPanel.Controls.Add(buttonMoveDown);
        centerButtonsPanel.Controls.Add(buttonClearCurrent);
        centerButtonsPanel.Controls.Add(buttonUndo);
        centerButtonsPanel.Location = new Point(370, 0);
        centerButtonsPanel.Margin = new Padding(0);
        centerButtonsPanel.Name = "centerButtonsPanel";
        centerButtonsPanel.Size = new Size(90, 449);
        centerButtonsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        centerButtonsPanel.TabIndex = 1;
        // 
        // buttonAddAll
        // 
        buttonAddAll.Image = global::GitUI.Properties.Images.ToolbarArrowAddAll;
        buttonAddAll.Location = new Point(18, 68);
        buttonAddAll.Name = "buttonAddAll";
        buttonAddAll.Size = new Size(50, 45);
        buttonAddAll.TabIndex = 0;
        buttonAddAll.UseVisualStyleBackColor = true;
        buttonAddAll.Click += ButtonAddAll_Click;
        //
        // buttonAdd
        //
        buttonAdd.Image = global::GitUI.Properties.Images.ToolbarArrowRight;
        buttonAdd.Location = new Point(18, 123);
        buttonAdd.Name = "buttonAdd";
        buttonAdd.Size = new Size(50, 45);
        buttonAdd.TabIndex = 1;
        buttonAdd.UseVisualStyleBackColor = true;
        buttonAdd.Click += ButtonAdd_Click;
        //
        // buttonRemove
        //
        buttonRemove.Image = global::GitUI.Properties.Images.ToolbarArrowLeft;
        buttonRemove.Location = new Point(18, 178);
        buttonRemove.Name = "buttonRemove";
        buttonRemove.Size = new Size(50, 45);
        buttonRemove.TabIndex = 2;
        buttonRemove.UseVisualStyleBackColor = true;
        buttonRemove.Click += ButtonRemove_Click;
        //
        // buttonMoveUp
        //
        buttonMoveUp.Image = global::GitUI.Properties.Images.ToolbarArrowUp;
        buttonMoveUp.Location = new Point(18, 233);
        buttonMoveUp.Name = "buttonMoveUp";
        buttonMoveUp.Size = new Size(50, 45);
        buttonMoveUp.TabIndex = 3;
        buttonMoveUp.UseVisualStyleBackColor = true;
        buttonMoveUp.Click += ButtonMoveUp_Click;
        //
        // buttonMoveDown
        //
        buttonMoveDown.Image = global::GitUI.Properties.Images.ToolbarArrowDown;
        buttonMoveDown.Location = new Point(18, 288);
        buttonMoveDown.Name = "buttonMoveDown";
        buttonMoveDown.Size = new Size(50, 45);
        buttonMoveDown.TabIndex = 4;
        buttonMoveDown.UseVisualStyleBackColor = true;
        buttonMoveDown.Click += ButtonMoveDown_Click;
        //
        // buttonClearCurrent
        //
        buttonClearCurrent.Image = global::GitUI.Properties.Images.ToolbarCross;
        buttonClearCurrent.Location = new Point(18, 343);
        buttonClearCurrent.Name = "buttonClearCurrent";
        buttonClearCurrent.Size = new Size(50, 45);
        buttonClearCurrent.TabIndex = 5;
        buttonClearCurrent.UseVisualStyleBackColor = true;
        buttonClearCurrent.Click += ButtonClearCurrent_Click;
        //
        // buttonUndo
        //
        buttonUndo.Image = global::GitUI.Properties.Images.ToolbarUndo;
        buttonUndo.Location = new Point(18, 398);
        buttonUndo.Name = "buttonUndo";
        buttonUndo.Size = new Size(50, 45);
        buttonUndo.TabIndex = 6;
        buttonUndo.UseVisualStyleBackColor = true;
        buttonUndo.Enabled = false;
        buttonUndo.Click += ButtonUndo_Click;
        // 
        // rightPanel
        // 
        rightPanel.Controls.Add(labelShow);
        rightPanel.Controls.Add(buttonShowIconText);
        rightPanel.Controls.Add(buttonShowAllIconText);
        rightPanel.Controls.Add(textBoxFilterCurrent);
        rightPanel.Controls.Add(buttonClearCurrentFilter);
        rightPanel.Controls.Add(listBoxCurrent);
        rightPanel.Location = new Point(460, 0);
        rightPanel.Margin = new Padding(0);
        rightPanel.Name = "rightPanel";
        rightPanel.Size = new Size(384, 449);
        rightPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        rightPanel.TabIndex = 2;
        //
        // labelShow  (replaces labelCurrent — "Current actions:" label)
        //
        labelShow.AutoSize = true;
        labelShow.Location = new Point(3, 8);
        labelShow.Name = "labelShow";
        labelShow.Size = new Size(120, 15);
        labelShow.TabIndex = 0;
        labelShow.Text = "Show icon text (*):";
        //
        // buttonShowAllIconText  — right edge aligns with listBoxCurrent right edge (x=3+330=333)
        //                          width=80 → x = 333-80 = 253
        //
        buttonShowAllIconText.Location = new Point(258, 3);
        buttonShowAllIconText.Name = "buttonShowAllIconText";
        buttonShowAllIconText.Size = new Size(105, 30);
        buttonShowAllIconText.TabIndex = 2;
        buttonShowAllIconText.Text = "For all icons";
        buttonShowAllIconText.UseVisualStyleBackColor = true;
        buttonShowAllIconText.Click += ButtonShowAllIconText_Click;
        //
        // buttonShowIconText  — immediately left of buttonShowAllIconText, 4px gap
        //                       x = 258 - 105 - 4 = 149
        //
        buttonShowIconText.Location = new Point(149, 3);
        buttonShowIconText.Name = "buttonShowIconText";
        buttonShowIconText.Size = new Size(105, 30);
        buttonShowIconText.TabIndex = 1;
        buttonShowIconText.Text = "For this icon";
        buttonShowIconText.UseVisualStyleBackColor = true;
        buttonShowIconText.Click += ButtonShowIconText_Click;
        // 
        // textBoxFilterCurrent
        // 
        textBoxFilterCurrent.Location = new Point(3, 40);
        textBoxFilterCurrent.Name = "textBoxFilterCurrent";
        textBoxFilterCurrent.PlaceholderText = "Filter...";
        textBoxFilterCurrent.Size = new Size(332, 23);
        textBoxFilterCurrent.TabIndex = 3;
        textBoxFilterCurrent.TextChanged += TextBoxFilterCurrent_TextChanged;
        // 
        // buttonClearCurrentFilter
        // 
        buttonClearCurrentFilter.Location = new Point(338, 39);
        buttonClearCurrentFilter.Name = "buttonClearCurrentFilter";
        buttonClearCurrentFilter.Size = new Size(25, 25);
        buttonClearCurrentFilter.TabIndex = 4;
        buttonClearCurrentFilter.Text = "✕";
        buttonClearCurrentFilter.UseVisualStyleBackColor = true;
        buttonClearCurrentFilter.Click += ButtonClearCurrentFilter_Click;
        // 
        // listBoxCurrent
        // 
        listBoxCurrent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        listBoxCurrent.DrawMode = DrawMode.OwnerDrawFixed;
        listBoxCurrent.FormattingEnabled = true;
        listBoxCurrent.ItemHeight = 24;
        listBoxCurrent.Location = new Point(3, 68);
        listBoxCurrent.Name = "listBoxCurrent";
        listBoxCurrent.Size = new Size(360, 374);
        listBoxCurrent.TabIndex = 5;
        listBoxCurrent.DrawItem += ListBox_DrawItem;
        listBoxCurrent.MouseDoubleClick += ListBoxCurrent_MouseDoubleClick;
        // 
        // bottomPanel
        // 
        bottomPanel.Dock = DockStyle.Fill;
        bottomPanel.Location = new Point(0, 494);
        bottomPanel.Margin = new Padding(0);
        bottomPanel.Name = "bottomPanel";
        bottomPanel.Size = new Size(784, 0);
        bottomPanel.TabIndex = 2;
        //
        // labelNoFormBrowse — in middlePanel, anchored to the bottom-left
        //
        labelNoFormBrowse.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        labelNoFormBrowse.AutoSize = true;
        labelNoFormBrowse.ForeColor = Color.Red;
        labelNoFormBrowse.Location = new Point(3, 435);
        labelNoFormBrowse.Name = "labelNoFormBrowse";
        labelNoFormBrowse.Size = new Size(367, 15);
        labelNoFormBrowse.TabIndex = 1;
        labelNoFormBrowse.Text = "Please open a repository window to customize toolbars.";
        labelNoFormBrowse.Visible = false;
        // 
        // ToolbarsSettingsPage
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Controls.Add(mainTableLayoutPanel);
        MinimumSize = new Size(870, 550);
        Name = "ToolbarsSettingsPage";
        Padding = new Padding(8);
        Size = new Size(870, 550);
        Text = "Toolbars";
        mainTableLayoutPanel.ResumeLayout(false);
        topPanel.ResumeLayout(false);
        topPanel.PerformLayout();
        middlePanel.ResumeLayout(false);
        leftPanel.ResumeLayout(false);
        leftPanel.PerformLayout();
        centerButtonsPanel.ResumeLayout(false);
        rightPanel.ResumeLayout(false);
        rightPanel.PerformLayout();
        bottomPanel.ResumeLayout(false);
        bottomPanel.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel mainTableLayoutPanel;
    private Panel topPanel;
    private Label labelToolbar;
    private CheckBox checkBoxToolbarVisible;
    private ComboBox comboBoxToolbar;
    private Button buttonAddToolbar;
    private Button buttonRemoveToolbar;
    private Label labelPosition;
    private Button buttonToolbarLayout;
    private Button buttonLocateToolbar;
    private Panel middlePanel;
    private Panel leftPanel;
    private Label labelAvailable;
    private ComboBox comboBoxCategory;
    private TextBox textBoxFilterAvailable;
    private Button buttonClearAvailableFilter;
    private ListBox listBoxAvailable;
    private Panel centerButtonsPanel;
    private Button buttonAddAll;
    private Button buttonAdd;
    private Button buttonRemove;
    private Button buttonMoveUp;
    private Button buttonMoveDown;
    private Button buttonClearCurrent;
    private Button buttonUndo;
    private Panel rightPanel;
    private Label labelShow;
    private Button buttonShowIconText;
    private Button buttonShowAllIconText;
    private TextBox textBoxFilterCurrent;
    private Button buttonClearCurrentFilter;
    private ListBox listBoxCurrent;
    private Panel bottomPanel;
    private Label labelNoFormBrowse;
}

