#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using GitExtUtils.GitUI;

namespace GitUI
{
    // Inspired by https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms/src/System/Windows/Forms/ToolStripSettingsManager.cs

    public interface IToolStripSettingsManager
    {
        void Load(Form ownerForm, Action<ToolStrip[]> invalidSettingsHandler, params ToolStrip[]? toolStrips);
        void Save(Form ownerForm, params ToolStrip[]? toolStrips);
    }

    [Export(typeof(IToolStripSettingsManager))]
    internal partial class ToolStripExSettingsManager : IToolStripSettingsManager
    {
        private void ApplySettings(Form ownerForm, Dictionary<ToolStrip, ToolStripExSettings> toolStripSettingsToApply)
        {
            if (toolStripSettingsToApply.Count == 0)
            {
                return;
            }

            // Build up a hash of where we want the ToolStrips to go
            Dictionary<string, List<ToolStrip>> toolStripPanelDestinations = new();

            foreach (var toolStripSettings in toolStripSettingsToApply)
            {
                string destinationPanel = toolStripSettings.Value.ToolStripPanelName!;
                if (!toolStripPanelDestinations.ContainsKey(destinationPanel))
                {
                    toolStripPanelDestinations[destinationPanel] = new();
                }

                toolStripPanelDestinations[destinationPanel].Add(toolStripSettings.Key);
            }

            // Build up a list of the toolstrippanels to party on.
            List<ToolStripPanel> toolStripPanels = new();
            FindControls(true, ownerForm.Controls, toolStripPanels);
            foreach (ToolStripPanel toolStripPanel in toolStripPanels)
            {
                // Set all the controls to visible false.
                foreach (Control c in toolStripPanel.Controls)
                {
                    c.Visible = false;
                }

                string toolStripPanelName = GetToolStripPanelKey(toolStripPanel, defaultValue: toolStripPanel.Name);

                // Get the associated toolstrips for this panel
                if (toolStripPanelDestinations.ContainsKey(toolStripPanelName))
                {
                    List<ToolStrip> toolStrips = toolStripPanelDestinations[toolStripPanelName];
                    if (toolStrips is null)
                    {
                        continue;
                    }

                    toolStripPanel.BeginInit();

                    // Add the strips in the desired (i.e. as persisted) order - by location top-bottom/left-right.
                    //
                    // NOTE: since we currently only support the horizontal orientation, hence we use YXComparer.
                    // In the future, if we decide to support vertical strips orientation, we'll need to use a XYComparer.
                    // Refer to https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms/src/System/Windows/Forms/ToolStripPanel.ToolStripPanelControlCollection.XYComparer.cs
                    toolStrips.Sort(new YXComparer(toolStripSettingsToApply));

                    foreach (ToolStrip toolStrip in toolStrips)
                    {
                        ToolStripExSettings settings = toolStripSettingsToApply[toolStrip];

                        toolStrip.Visible = settings.Visible;

                        // The location is scaled to the current dpi
                        Point location = DpiUtil.Scale(settings.Location);

                        // If the strip was docked to its neighbour when saved, we may need to adjust the strip's location
                        // because the strips have variable length when the app is running, e.g., the working directory selector
                        // has a variable length, so does the branches selector.
                        if (settings.DockedToNeighbour && toolStripPanel.Controls.Count > 0)
                        {
                            ToolStrip neighbour = (ToolStrip)toolStripPanel.Controls[toolStripPanel.Controls.Count - 1];
                            location = new(neighbour.Bounds.X + neighbour.Bounds.Width + 1, toolStrip.Location.Y);
                        }

                        toolStripPanel.Join(toolStrip, location);
                    }

                    toolStripPanel.EndInit();
                }
            }
        }

        private void FindControls<T>(bool searchAllChildren, Control.ControlCollection controlsToLookIn, List<T> foundControls)
            where T : Control
        {
            // Perform breadth first search - as it's likely people will want controls belonging
            // to the same parent close to each other.
            for (int i = 0; i < controlsToLookIn.Count; i++)
            {
                if (controlsToLookIn[i] is null)
                {
                    continue;
                }

                if (controlsToLookIn[i] is T control)
                {
                    foundControls.Add(control);
                }
            }

            // Optional recursive search for controls in child collections.
            if (searchAllChildren)
            {
                for (int i = 0; i < controlsToLookIn.Count; i++)
                {
                    if (controlsToLookIn[i] is null || controlsToLookIn[i] is Form)
                    {
                        continue;
                    }

                    if ((controlsToLookIn[i].Controls is not null) && controlsToLookIn[i].Controls.Count > 0)
                    {
                        // If it has a valid child collection, append those results to our collection.
                        FindControls(searchAllChildren, controlsToLookIn[i].Controls, foundControls);
                    }
                }
            }
        }

        private string GetSettingsKey(Form ownerForm, string toolStripName)
            => $"{ownerForm.Name}.{toolStripName}";

        private string GetToolStripPanelKey(ToolStripPanel toolStripPanel, string defaultValue)
        {
            // Handle the ToolStripPanels inside a ToolStripContainer
            if (toolStripPanel.Parent is ToolStripContainer toolStripContainer && !string.IsNullOrEmpty(toolStripContainer.Name))
            {
                return $"{toolStripContainer.Name}.{toolStripPanel.Dock}";
            }

            return defaultValue;
        }

        private string GetToolStripPanelName(ToolStrip toolStrip)
        {
            if (toolStrip.Parent is not ToolStripPanel parentPanel)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(parentPanel.Name))
            {
                return parentPanel.Name;
            }

            string name = GetToolStripPanelKey(parentPanel, defaultValue: string.Empty);
            Debug.Assert(!string.IsNullOrEmpty(name), "ToolStrip was parented to a panel, but we couldn't figure out its name.");

            return name;
        }

        public void Load(Form ownerForm, Action<ToolStrip[]> invalidSettingsHandler, params ToolStrip[]? toolStrips)
        {
            if (toolStrips is null || toolStrips.Length == 0)
            {
                throw new ArgumentException($"At least one ToolStrip is required.", nameof(toolStrips));
            }

            bool settingsValid = true;
            var settings = new Dictionary<ToolStrip, ToolStripExSettings>(toolStrips.Length);
            foreach (ToolStrip toolStrip in toolStrips)
            {
                ToolStripExSettings toolStripSettings = new(GetSettingsKey(ownerForm, toolStrip.Name));
                settings[toolStrip] = toolStripSettings;

                settingsValid &= HasValidDestinationPanel(toolStripSettings, toolStrip.Name);
            }

            if (settingsValid)
            {
                ApplySettings(ownerForm, settings);
            }
            else
            {
                invalidSettingsHandler(toolStrips);
            }

            return;

            bool HasValidDestinationPanel(ToolStripExSettings toolStripSettings, string toolStripName)
            {
                string? destinationPanel = !string.IsNullOrEmpty(toolStripSettings.ToolStripPanelName) ? toolStripSettings.ToolStripPanelName : null;
                if (destinationPanel is null)
                {
                    // Not in a panel.

                    if (!toolStripSettings.IsDefault)
                    {
                        // NOTE: any toolstrip that we want to track the location of must be placed in a ToolStripPanel.
                        // Whilst the original ToolStripSettingsManager supports tracking of toolstrips outside panels, we don't.
                        // Please rework the UI as necessary.

                        Debug.Assert(false, $"ToolStrip '{toolStripName}' must be parented to a panel.");
                    }

                    return false;
                }

                return true;
            }
        }

        public void Save(Form ownerForm, params ToolStrip[]? toolStrips)
        {
            if (toolStrips is null || toolStrips.Length == 0)
            {
                throw new ArgumentException($"At least one ToolStrip is required.", nameof(toolStrips));
            }

            foreach (ToolStrip toolStrip in toolStrips)
            {
                ToolStripExSettings toolStripSettings = new(GetSettingsKey(ownerForm, toolStrip.Name));

                toolStripSettings.ToolStripPanelName = GetToolStripPanelName(toolStrip);
                toolStripSettings.Visible = toolStrip.Visible;

                // This is O(n^2), but we don't have many strips to iterate over.
                foreach (ToolStrip strip in toolStrips)
                {
                    if (strip.Bounds.Contains(toolStrip.Location.X - 2, toolStrip.Location.Y + 2))
                    {
                        toolStripSettings.DockedToNeighbour = true;
                        break;
                    }
                }

                // Store the location at 96dpi
                toolStripSettings.Location = DpiUtil.Scale(toolStrip.Location, 96);

                toolStripSettings.Save();
            }
        }

        // Inspired by https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms/src/System/Windows/Forms/ToolStripPanel.ToolStripPanelControlCollection.YXComparer.cs
        // Sort by Y, then X.
        private class YXComparer : IComparer<ToolStrip>
        {
            private readonly Dictionary<ToolStrip, ToolStripExSettings> _toolStripSettingsToApply;

            public YXComparer(Dictionary<ToolStrip, ToolStripExSettings> toolStripSettingsToApply)
            {
                _toolStripSettingsToApply = toolStripSettingsToApply;
            }

            public int Compare(ToolStrip? first, ToolStrip? second)
            {
                ToolStripExSettings firstSettings = _toolStripSettingsToApply[first!];
                ToolStripExSettings secondSettings = _toolStripSettingsToApply[second!];

                if (firstSettings.Location.Y < secondSettings.Location.Y)
                {
                    return -1;
                }

                if (firstSettings.Location.Y == secondSettings.Location.Y)
                {
                    return firstSettings.Location.X - secondSettings.Location.X;
                }

                return 1;
            }
        }
    }
}
