using GitCommands;
using GitUI.Models;

namespace GitUI.UserControls;

public partial class OutputHistoryPanelController : OutputHistoryControllerBase
{
    private SplitContainer _verticalSplitContainer;

    public OutputHistoryPanelController(IOutputHistoryModel outputHistoryModel,
                                        OutputHistoryControl outputHistoryControl,
                                        Control parent,
                                        SplitContainer verticalSplitContainer,
                                        SplitContainer horizontalSplitContainer)
        : base(outputHistoryModel, outputHistoryControl)
    {
        _verticalSplitContainer = verticalSplitContainer;

        if (!outputHistoryModel.Enabled)
        {
            return;
        }

        parent.Controls.Add(outputHistoryControl);
        parent.Controls.SetChildIndex(outputHistoryControl, 0);
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

            outputHistoryControl.Visible = !verticalSplitContainer.Panel2Collapsed && verticalSplitContainer.Visible;
            if (outputHistoryControl.Visible)
            {
                int height = verticalSplitContainer.Panel2.Height;
                int width = (2 * offset) + GetWidth();
                outputHistoryControl.SetBounds(margin, parent.Height - height - offset - margin, width, (2 * offset) + height);

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

    public override bool ToggleControl()
    {
        if (!_outputHistoryModel.Enabled)
        {
            return false;
        }

        bool show = !AppSettings.OutputHistoryPanelVisible.Value;
        AppSettings.OutputHistoryPanelVisible.Value = show;
        _verticalSplitContainer.Panel2Collapsed = !show;
        if (show)
        {
            _textBox.FindForm().ActiveControl = _textBox;
        }

        return true;
    }
}
