using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor
{
    // simple manager which return highighting strategy implemented in current assembly
    class CustomSyntaxStrategyManager
    {
        const string StrategySuffix = "HighlightingStrategy";

        static readonly Dictionary<string, Type> StrategyTypes;

        static CustomSyntaxStrategyManager()
        {
            try
            {
                StrategyTypes = typeof(CustomSyntaxStrategyManager).Assembly
                    .GetTypes()
                    .Where(t => !t.IsAbstract && typeof(IHighlightingStrategy).IsAssignableFrom(t))
                    .ToDictionary(t => t.Name.EndsWith(StrategySuffix) ? t.Name.Substring(0, t.Name.Length - StrategySuffix.Length).ToUpperInvariant() : t.Name.ToUpperInvariant(), t => t)
                ;
            }
            catch
            {
                StrategyTypes = new Dictionary<string, Type>();
            }
        }

        public static IHighlightingStrategy TryFindCustomSyntaxStrategy(string syntax)
        {
            Type t;
            if (!StrategyTypes.TryGetValue(syntax.ToUpperInvariant(), out t))
                return null;

            return (IHighlightingStrategy)Activator.CreateInstance(t);
        }
    }
}
