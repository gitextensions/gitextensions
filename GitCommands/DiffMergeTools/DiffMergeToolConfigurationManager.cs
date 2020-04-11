using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GitCommands.Config;
using GitUIPluginInterfaces;

namespace GitCommands.DiffMergeTools
{
    public sealed class DiffMergeToolConfigurationManager
    {
        private readonly Func<IConfigValueStore> _getFileSettings;
        private readonly Func<string, IEnumerable<string>, string> _findFileInFolders;

        public DiffMergeToolConfigurationManager(Func<IConfigValueStore> getFileSettings)
            : this(getFileSettings, PathUtil.FindInFolders)
        {
            _getFileSettings = getFileSettings;
        }

        internal DiffMergeToolConfigurationManager(Func<IConfigValueStore> getFileSettings, Func<string, IEnumerable<string>, string> findFileInFolders)
        {
            _getFileSettings = getFileSettings;
            _findFileInFolders = findFileInFolders;
        }

        /// <summary>
        /// Gets the diff tool configured in the effective config under 'diff.guitool'.
        /// </summary>
        public string ConfiguredDiffTool => _getFileSettings()?.GetValue(SettingKeyString.DiffToolKey);

        /// <summary>
        /// Gets the merge tool configured in the effective config under 'merge.guitool' or 'merge.tool'.
        /// </summary>
        public string ConfiguredMergeTool
        {
            get
            {
                string mergetool = "";
                if (GitVersion.Current.SupportGuiMergeTool)
                {
                    mergetool = _getFileSettings()?.GetValue(SettingKeyString.MergeToolKey);
                }

                // Fallback and older Git
                if (string.IsNullOrEmpty(mergetool))
                {
                    mergetool = _getFileSettings()?.GetValue(SettingKeyString.MergeToolNoGuiKey);
                }

                return mergetool;
            }
        }

        /// <summary>
        /// Configures diff/merge tool.
        /// </summary>
        /// <param name="toolName">The name of the diff/merge tool.</param>
        /// <param name="toolType">Type of the tool.</param>
        /// <param name="toolPath">The location of the tool's executable.</param>
        /// <param name="toolCommand">The command.</param>
        public void ConfigureDiffMergeTool(string toolName, DiffMergeToolType toolType, string toolPath, string toolCommand)
        {
            if (string.IsNullOrWhiteSpace(toolName))
            {
                Debug.Fail("Diff/merge tool is required");
                return;
            }

            var fileSettings = _getFileSettings();
            if (fileSettings == null)
            {
                return;
            }

            (string toolKey, string prefix) = GetInfo(toolType);
            fileSettings.SetValue(toolKey, toolName);
            fileSettings.SetPathValue(string.Concat(prefix, ".", toolName, ".path"), toolPath);
            fileSettings.SetPathValue(string.Concat(prefix, ".", toolName, ".cmd"), toolCommand);
        }

        /// <summary>
        /// Gets the command for the diff/merge tool configured in the effective config.
        /// </summary>
        /// <param name="toolName">The name of the diff/merge tool.</param>
        /// <param name="toolType">Type of the tool.</param>
        /// <returns>The command for the diff/merge tool configured in the effective config. </returns>
        public string GetToolCommand(string toolName, DiffMergeToolType toolType)
        {
            if (string.IsNullOrWhiteSpace(toolName))
            {
                return string.Empty;
            }

            var command = GetToolSetting(toolName, toolType, "cmd");
            if (!string.IsNullOrWhiteSpace(command))
            {
                return command;
            }

            var config = LoadDiffMergeToolConfig(toolName, null);

            switch (toolType)
            {
                case DiffMergeToolType.Diff: return config.FullDiffCommand;
                case DiffMergeToolType.Merge: return config.FullMergeCommand;
                default: throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets the path to the diff/merge tool configured in the effective config.
        /// </summary>
        /// <param name="toolName">The name of the diff/merge tool.</param>
        /// <param name="toolType">Type of the tool.</param>
        /// <returns>The path to the diff/merge tool configured in the effective config. </returns>
        public string GetToolPath(string toolName, DiffMergeToolType toolType)
        {
            if (string.IsNullOrWhiteSpace(toolName))
            {
                return string.Empty;
            }

            var path = GetToolSetting(toolName, toolType, "path");
            if (!string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            return LoadDiffMergeToolConfig(toolName, null).Path;
        }

        public DiffMergeToolConfiguration LoadDiffMergeToolConfig(string toolName, string userSuppliedPath)
        {
            if (string.IsNullOrWhiteSpace(toolName))
            {
                throw new ArgumentException(@"Invalid diff/merge tool requested", nameof(toolName));
            }

            string fullPath;

            var diffTool = RegisteredDiffMergeTools.Get(toolName);
            if (diffTool == null)
            {
                var exeName = toolName + ".exe";
                if (!string.IsNullOrWhiteSpace(userSuppliedPath))
                {
                    fullPath = userSuppliedPath;
                }
                else
                {
                    PathUtil.TryFindFullPath(exeName, out fullPath);
                }

                return new DiffMergeToolConfiguration(exeName, fullPath ?? string.Empty, null, null);
            }

            if (!string.IsNullOrWhiteSpace(userSuppliedPath))
            {
                fullPath = userSuppliedPath;
            }
            else
            {
                var pathsToSearch = new[] { UnquoteString(GetToolSetting(diffTool.Name, DiffMergeToolType.Merge, "path")) }.Union(diffTool.SearchPaths);
                fullPath = _findFileInFolders(diffTool.ExeFileName, pathsToSearch);
            }

            return new DiffMergeToolConfiguration(diffTool.ExeFileName, fullPath, diffTool.DiffCommand, diffTool.MergeCommand);
        }

        /// <summary>
        /// Unset currently configured diff/merge tool.
        /// </summary>
        /// <param name="toolType">Type of the tool.</param>
        public void UnsetCurrentTool(DiffMergeToolType toolType)
        {
            var fileSettings = _getFileSettings();
            if (fileSettings == null)
            {
                return;
            }

            (string toolKey, string _) = GetInfo(toolType);
            fileSettings.SetValue(toolKey, "");
        }

        private (string toolKey, string prefix) GetInfo(DiffMergeToolType toolType)
        {
            switch (toolType)
            {
                case DiffMergeToolType.Diff:
                    return (SettingKeyString.DiffToolKey, "difftool");

                case DiffMergeToolType.Merge:
                    return (SettingKeyString.MergeToolKey, "mergetool");

                default: throw new NotSupportedException();
            }
        }

        private string GetToolSetting(string toolName, DiffMergeToolType toolType, string settingSuffix)
        {
            (string toolKey, string prefix) = GetInfo(toolType);

            if (string.IsNullOrWhiteSpace(toolName))
            {
                toolName = _getFileSettings()?.GetValue(toolKey);
            }

            return string.IsNullOrWhiteSpace(toolName) ?
                string.Empty :
                _getFileSettings()?.GetValue(string.Concat(prefix, ".", toolName, ".", settingSuffix));
        }

        private static string UnquoteString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            int length = str.Length;
            if (length > 1 && str[0] == '\"' && str[length - 1] == '\"')
            {
                str = str.Substring(1, length - 2);
            }

            return str;
        }

        internal TestAccessor GetTestAccessor()
            => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            private readonly DiffMergeToolConfigurationManager _manager;

            public TestAccessor(DiffMergeToolConfigurationManager manager)
            {
                _manager = manager;
            }

            public (string toolKey, string prefix) GetInfo(DiffMergeToolType toolType)
                => _manager.GetInfo(toolType);

            public string GetToolSetting(string toolName, DiffMergeToolType toolType, string settingSuffix)
                => _manager.GetToolSetting(toolName, toolType, settingSuffix);
        }
    }
}
