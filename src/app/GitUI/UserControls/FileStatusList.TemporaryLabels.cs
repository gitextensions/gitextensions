using GitUI.UserControls;

namespace GitUI;

partial class FileStatusList
{
    private readonly TemporaryFileLabels _temporaryLabels = new();
    private ToolStripMenuItem[]? _temporaryLabelMenuItems;

    /// <summary>
    ///  Gets whether temporary labels are enabled for this list.
    ///  Labels are toggled from the toolbar, are off by default and live only
    ///  as long as this list (for example a <c>FormDiff</c> window).
    ///  Disabling the labels hides them; the assignments are kept, so
    ///  re-enabling the labels shows them again.
    /// </summary>
    public bool TemporaryLabelsEnabled { get; private set; }

    private ToolStripMenuItem[] TemporaryLabelMenuItems => _temporaryLabelMenuItems ??=
    [
        tsmiTemporaryLabel1,
        tsmiTemporaryLabel2,
        tsmiTemporaryLabel3,
        tsmiTemporaryLabel4,
        tsmiTemporaryLabel5,
        tsmiTemporaryLabel6,
        tsmiTemporaryLabel7,
        tsmiTemporaryLabel8,
        tsmiTemporaryLabel9,
    ];

    private void TemporaryLabelsEnable_Click(object sender, EventArgs e)
    {
        SetTemporaryLabelsEnabled(tsmiEnableTemporaryLabels.Checked);
    }

    private void TemporaryLabelMenuItem_Click(object sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item && item.Tag is int label)
        {
            SetTemporaryLabelForSelectedItems(label);
        }
    }

    private void ClearTemporaryLabel_Click(object sender, EventArgs e)
    {
        SetTemporaryLabelForSelectedItems(TemporaryFileLabels.NoLabel);
    }

    private void ClearAllTemporaryLabels_Click(object sender, EventArgs e)
    {
        if (_temporaryLabels.Count == 0)
        {
            return;
        }

        _temporaryLabels.ClearAll();
        RefreshTemporaryLabels();
    }

    private void SetTemporaryLabelsEnabled(bool enabled)
    {
        if (TemporaryLabelsEnabled == enabled)
        {
            return;
        }

        TemporaryLabelsEnabled = enabled;
        RefreshTemporaryLabels();
    }

    private void SetTemporaryLabelForSelectedItems(int label)
    {
        bool changed = false;

        foreach (FileStatusItem item in SelectedItems)
        {
            changed |= label == TemporaryFileLabels.NoLabel
                ? _temporaryLabels.Clear(item.Item.Name)
                : _temporaryLabels.TryAssign(item.Item.Name, label);
        }

        if (changed)
        {
            RefreshTemporaryLabels();
        }
    }

    private void RefreshTemporaryLabels()
    {
        // Rebuild the list so the label markers are applied to the nodes;
        // the selection is preserved by UpdateFileStatusListView.
        UpdateFileStatusListView(GitItemStatusesWithDescription, updateCausedByFilter: true);
    }
}
