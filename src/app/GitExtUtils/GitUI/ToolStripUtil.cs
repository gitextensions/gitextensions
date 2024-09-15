namespace GitExtUtils.GitUI;

public static class ToolStripUtil
{
    /// <summary>
    /// Enumerates all descendant ToolStripItem items.
    /// </summary>
    public static IEnumerable<ToolStripItem> FindToolStripItems(this ToolStrip toolStrip)
    {
        Queue<ToolStripItem> queue = new();

        foreach (ToolStripItem item in toolStrip.Items)
        {
            queue.Enqueue(item);
        }

        while (queue.Count != 0)
        {
            ToolStripItem item = queue.Dequeue();

            yield return item;

            if (item is ToolStripDropDownItem toolStripDropDownItem)
            {
                foreach (ToolStripItem dropDownItem in toolStripDropDownItem.DropDownItems)
                {
                    queue.Enqueue(dropDownItem);
                }
            }
        }
    }
}
