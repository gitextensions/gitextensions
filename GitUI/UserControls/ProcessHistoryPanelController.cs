using GitUI.Models;

namespace GitUI.UserControls;

public partial class ProcessHistoryPanelController : ProcessHistoryControllerBase
{
    private SplitContainer _verticalSplitContainer;

    public ProcessHistoryPanelController(IProcessHistoryModel processHistoryModel,
                                         ProcessHistoryControl processHistoryControl,
                                         Control parent,
                                         SplitContainer verticalSplitContainer,
                                         SplitContainer horizontalSplitContainer)
        : base(processHistoryModel, processHistoryControl)
    {
        _verticalSplitContainer = verticalSplitContainer;

        if (!processHistoryModel.Enabled)
        {
            _verticalSplitContainer.Panel2Collapsed = true;
            return;
        }

        parent.Controls.Add(processHistoryControl);
        parent.Controls.SetChildIndex(processHistoryControl, 0);
        Control heightVictimControl = horizontalSplitContainer.Panel1.Controls[0];
        verticalSplitContainer.Invalidated += (_, _) => SetSize();
        horizontalSplitContainer.Invalidated += (_, _) => SetSize();
        for (Control? splitterParent = horizontalSplitContainer.Parent; splitterParent is not null; splitterParent = splitterParent.Parent)
        {
            splitterParent.VisibleChanged += (_, _) => SetSize();
        }

        return;

        void SetSize()
        {
            const int margin = 6;
            const int offset = 1;
            const int h1offset = 1;

            processHistoryControl.Visible = !verticalSplitContainer.Panel2Collapsed && verticalSplitContainer.Visible;
            if (processHistoryControl.Visible)
            {
                int height = verticalSplitContainer.Panel2.Height;
                int width = (2 * offset) + GetWidth();
                processHistoryControl.SetBounds(margin, parent.Height - height - offset - margin, width, (2 * offset) + height);

                heightVictimControl.Dock = DockStyle.Top;
                heightVictimControl.Height = horizontalSplitContainer.Height - height;
            }
            else
            {
                heightVictimControl.Dock = DockStyle.Fill;
            }

            return;

            int GetWidth()
            {
                if (!horizontalSplitContainer.Visible)
                {
                    return h1offset + verticalSplitContainer.Panel2.ClientSize.Width;
                }

                int left = verticalSplitContainer.Panel2.PointToScreen(new(horizontalSplitContainer.Panel1.ClientRectangle.Left, 0)).X;
                int right = horizontalSplitContainer.Panel1.PointToScreen(new(horizontalSplitContainer.Panel1.ClientRectangle.Right, 0)).X;
                return right - left;
            }
        }
    }

    public override void ToggleControl()
    {
        bool show = _processHistoryModel.Enabled && _verticalSplitContainer.Panel2Collapsed;
        _verticalSplitContainer.Panel2Collapsed = !show;
        if (show)
        {
            _textBox.FindForm().ActiveControl = _textBox;
        }
    }
}
