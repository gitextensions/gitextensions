#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Forms;
using GitExtUtils.GitUI;

namespace GitUI
{
    // Inspired by https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms/src/System/Windows/Forms/ToolStripSettingsManager.cs

    public interface IToolStripSettingsManager
    {
        void Load(Form ownerForm, params ToolStrip[]? toolStrips);
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
                string? destinationPanel = !string.IsNullOrEmpty(toolStripSettings.Value.ToolStripPanelName) ? toolStripSettings.Value.ToolStripPanelName : null;
                if (destinationPanel is null)
                {
                    // Not in a panel.

                    if (!toolStripSettings.Value.IsDefault)
                    {
                        // NOTE: any toolstrip that we want to track the location of must be placed in a ToolStripPanel.
                        // Whilst the original ToolStripSettingsManager supports tracking of toolstrips outside panels, we don't.
                        // Please rework the UI as necessary.

                        Debug.Assert(false, $"ToolStrip '{toolStripSettings.Key.Name}' must be parented to a panel.");
                    }

                    return;
                }
                else
                {
                    // This toolStrip is in a ToolStripPanel. We will process it below.
                    if (!toolStripPanelDestinations.ContainsKey(destinationPanel))
                    {
                        toolStripPanelDestinations[destinationPanel] = new();
                    }

                    toolStripPanelDestinations[destinationPanel].Add(toolStripSettings.Key);
                }
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

                    // We need to sort the toolstrips, so that we add with the bottom-most/right-most strips first. This ensures
                    // correct order of strips.
                    //
                    // NOTE: since we currently only support the horizontal orientation, hence we use YXComparer.
                    // In the future, if we decide to support vertical strips orientation, we'll need to use a XYComparer.
                    // Refer to https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms/src/System/Windows/Forms/ToolStripPanel.ToolStripPanelControlCollection.XYComparer.cs
                    toolStrips.Sort(new YXComparer());

                    foreach (var toolStrip in toolStrips)
                    {
                        var settings = toolStripSettingsToApply[toolStrip];

                        toolStrip.Visible = settings.Visible;

                        // The location is scaled to the current dpi
                        toolStripPanel.Join(toolStrip, DpiUtil.Scale(settings.Location));
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

        public void Load(Form ownerForm, params ToolStrip[]? toolStrips)
        {
            if (toolStrips is null || toolStrips.Length == 0)
            {
                throw new ArgumentException($"At least one ToolStrip is required.", nameof(toolStrips));
            }

            var settings = new Dictionary<ToolStrip, ToolStripExSettings>(toolStrips.Length);
            foreach (ToolStrip toolStrip in toolStrips)
            {
                settings[toolStrip] = new(GetSettingsKey(ownerForm, toolStrip.Name));
            }

            ApplySettings(ownerForm, settings);
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

                // Store the location at 96dpi
                toolStripSettings.Location = DpiUtil.Scale(toolStrip.Location, 96);

                toolStripSettings.Save();
            }
        }

        // Copied from https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms/src/System/Windows/Forms/ToolStripPanel.ToolStripPanelControlCollection.YXComparer.cs
        // Sort by Y, then X.
        private class YXComparer : IComparer<Control>
        {
            public int Compare(Control? first, Control? second)
            {
                if (first!.Bounds.Y < second!.Bounds.Y)
                {
                    return -1;
                }

                if (first.Bounds.Y == second.Bounds.Y)
                {
                    if (first.Bounds.X < second.Bounds.X)
                    {
                        return -1;
                    }

                    return 1;
                }

                return 1;
            }
        }
    }
}
