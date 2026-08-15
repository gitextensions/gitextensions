using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GitUI.ScriptsEngine;

/// <summary>
///  Basic implementation of <see cref="IScriptOptionsProvider"/>.
///  It replaces all script options of all implementations of <see cref="IScriptOptionsProvider"/> with an empty string.
/// </summary>
internal partial class ScriptOptionsProviderBase : IScriptOptionsProvider
{
    private static readonly string[] _options;

    [GeneratedRegex(@"^(System|Microsoft|netstandard|Accessibility|Ben\.Demystifier|BenjaminAbt\.StrongOf|ExCSS|Git\.Hub|ICSharpCode\.TextEditor|ResourceManager|SmartFormat|TestableIO|Testably|PresentationCore|UIAutomationTypes|WindowsBase|ZString)[.,]", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex _excludedAssemblies { get; }

    static ScriptOptionsProviderBase()
    {
        Type interfaceType = typeof(IScriptOptionsProvider);
        _options = [.. AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(GetTypes)
            .Where(type => type != interfaceType && interfaceType.IsAssignableFrom(type))
            .SelectMany(implementingType =>
                {
                    PropertyInfo? property = implementingType.GetProperty(nameof(ImplementedOptions), BindingFlags.Static | BindingFlags.NonPublic);
                    return (string[])property!.GetValue(obj: null)!;
                })];

        static Type[] GetTypes(Assembly assembly)
        {
            try
            {
                if (assembly.FullName is not string name || _excludedAssemblies.IsMatch(name))
                {
                    return [];
                }

                return assembly.GetTypes();
            }
            catch (Exception ex)
            {
                // Ignore outdated plugins, which may reference assemblies that are no longer available.
                Trace.WriteLine(ex);
                return [];
            }
        }
    }

    /// <summary>
    ///  The default implementation of <see cref="IScriptOptionsProvider"/> if no specific provider applies.
    /// </summary>
    public static IScriptOptionsProvider Default { get; } = new ScriptOptionsProviderBase();

    /// <summary>
    ///  This implementation of <see cref="IScriptOptionsProvider"/> does not add script options.
    /// </summary>
    /// <remarks>
    ///  But the static ctor reads this property values by means of reflection.
    /// </remarks>
    private static string[] ImplementedOptions => [];

    IReadOnlyList<string> IScriptOptionsProvider.Options => _options;

    public virtual IEnumerable<string> GetValues(string option)
        => _options.Contains(option)
            ? []
            : throw new InvalidOperationException(@$"The {nameof(ScriptsEngine)} shall not ask for values of option ""{option}"" not in {nameof(IScriptOptionsProvider.Options)}");
}
