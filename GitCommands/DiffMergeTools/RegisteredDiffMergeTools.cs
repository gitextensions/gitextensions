using System;
using System.Collections.Generic;
using System.Linq;

namespace GitCommands.DiffMergeTools
{
    /// <summary>
    /// A dictionary of all configured diff/merge tools.
    /// </summary>
    public static class RegisteredDiffMergeTools
    {
        private static readonly IDictionary<string, DiffMergeTool> RegisteredTools = new Dictionary<string, DiffMergeTool>(StringComparer.InvariantCultureIgnoreCase);

        static RegisteredDiffMergeTools()
        {
            var diffTools = AppDomain.CurrentDomain.GetAssemblies()
                                                   .SelectMany(asm =>
                                                   {
                                                       try
                                                       {
                                                           return asm.GetTypes();
                                                       }
                                                       catch (Exception)
                                                       {
                                                           return Array.Empty<Type>();
                                                       }
                                                   })
                                                   .Where(t => t.IsSubclassOf(typeof(DiffMergeTool)))
                                                   .Select(t => (DiffMergeTool)Activator.CreateInstance(t));
            foreach (var tool in diffTools)
            {
                RegisteredTools.Add(tool.Name, tool);
            }
        }

        /// <summary>
        /// Gets the collection of all configured diff/merge tools.
        /// </summary>
        public static IEnumerable<string> All(DiffMergeToolType toolType)
        {
            return RegisteredTools.Where(t =>
                    (t.Value.IsDiffTool && toolType == DiffMergeToolType.Diff) ||
                    (t.Value.IsMergeTool && toolType == DiffMergeToolType.Merge))
                .Select(t => t.Key);
        }

        internal static DiffMergeTool Get(string toolName)
        {
            if (RegisteredTools.ContainsKey(toolName))
            {
                return RegisteredTools[toolName];
            }

            return null;
        }
    }
}