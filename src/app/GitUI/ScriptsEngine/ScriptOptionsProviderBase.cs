using System.Reflection;

namespace GitUI.ScriptsEngine;

/// <summary>
///  Basic implementation of <see cref="IScriptOptionsProvider"/>.
///  It replaces all script options of all implementations of <see cref="IScriptOptionsProvider"/> with an empty string.
/// </summary>
internal class ScriptOptionsProviderBase : IScriptOptionsProvider
{
    private static readonly string[] _options;

    static ScriptOptionsProviderBase()
    {
        Type interfaceType = typeof(IScriptOptionsProvider);
        _options = [.. AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type != interfaceType && interfaceType.IsAssignableFrom(type))
            .SelectMany(implementingType =>
                {
                    PropertyInfo property = implementingType.GetProperty(nameof(ImplementedOptions), BindingFlags.Static | BindingFlags.NonPublic);
                    return (string[])property.GetValue(obj: null);
                })];
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
