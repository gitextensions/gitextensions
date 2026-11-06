using System.Reflection;
using System.Runtime.ExceptionServices;
using ResourceManager.CommitDataRenders;

namespace GitExtensions.ParityCapture;

/// <summary>
///  Borrows only capture-owned commit-header formatters without changing the application clock.
/// </summary>
internal sealed class CaptureFixtureClock : IDisposable
{
    private const string HeaderTypeName = "GitUI.CommitInfo.CommitInfoHeader";
    private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
    private readonly List<FormatterValue> _originalValues = [];
    private bool _disposed;

    private CaptureFixtureClock()
    {
    }

    /// <summary>
    ///  Installs the existing formatter's UTC-time input on actual capture-owned header instances.
    /// </summary>
    /// <param name="controls">The capture consumer's bounded root and descendant enumeration.</param>
    /// <param name="fixtureNowUtc">An explicit UTC fixture time, or null to retain every live formatter unchanged.</param>
    /// <returns>A scope restoring the exact previous formatter identities.</returns>
    internal static CaptureFixtureClock Apply(IEnumerable<object> controls, DateTime? fixtureNowUtc)
    {
        CaptureFixtureClock scope = new();
        if (fixtureNowUtc is not DateTime now)
        {
            return scope;
        }

        if (now.Kind != DateTimeKind.Utc)
        {
            throw new InvalidDataException("The capture fixture clock must be an explicit UTC DateTime.");
        }

        ArgumentNullException.ThrowIfNull(controls);
        IDateFormatter formatter = new DateFormatter(() => now);
        HashSet<object> visited = new(ReferenceEqualityComparer.Instance);
        try
        {
            foreach (object control in controls)
            {
                if (!visited.Add(control))
                {
                    continue;
                }

                Type? headerType = control.GetType();
                while (headerType is not null && headerType.FullName != HeaderTypeName)
                {
                    headerType = headerType.BaseType;
                }

                if (headerType is null)
                {
                    continue;
                }

                FieldInfo headerFormatter = RequireField(headerType, "_dateFormatter", typeof(IDateFormatter));
                FieldInfo rendererField = RequireField(headerType, "_commitDataHeaderRenderer", typeof(ICommitDataHeaderRenderer));
                CommitDataHeaderRenderer renderer = rendererField.GetValue(control) as CommitDataHeaderRenderer
                    ?? throw new InvalidOperationException("The actual capture header did not retain its original CommitDataHeaderRenderer.");
                FieldInfo rendererFormatter = RequireField(typeof(CommitDataHeaderRenderer), "_dateFormatter", typeof(IDateFormatter));

                // The renderer captured its formatter during construction. Replacing only
                // the header's field would leave the actual rendered relative time live.
                scope.Replace(control, headerFormatter, formatter);
                scope.Replace(renderer, rendererFormatter, formatter);
            }
        }
        catch (Exception acquisitionFailure)
        {
            try
            {
                scope.Dispose();
            }
            catch (Exception restorationFailure)
            {
                throw new AggregateException("The capture clock could not be acquired or completely restored.", acquisitionFailure, restorationFailure);
            }

            throw;
        }

        return scope;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        ExceptionDispatchInfo? failure = null;
        for (int index = _originalValues.Count - 1; index >= 0; index--)
        {
            FormatterValue original = _originalValues[index];
            try
            {
                original.Field.SetValue(original.Owner, original.Value);
            }
            catch (Exception exception)
            {
                failure ??= ExceptionDispatchInfo.Capture(exception);
            }
        }

        _originalValues.Clear();
        failure?.Throw();
    }

    private static FieldInfo RequireField(Type owner, string name, Type expectedType)
    {
        FieldInfo field = owner.GetField(name, InstanceFields)
            ?? throw new MissingFieldException(owner.FullName, name);
        if (field.FieldType != expectedType || field.IsStatic)
        {
            throw new InvalidOperationException($"The capture clock does not recognize {owner.FullName}.{name}.");
        }

        return field;
    }

    private void Replace(object owner, FieldInfo field, IDateFormatter formatter)
    {
        IDateFormatter original = field.GetValue(owner) as IDateFormatter
            ?? throw new InvalidOperationException($"The actual capture field {field.DeclaringType?.FullName}.{field.Name} did not retain its original formatter.");

        // Instance-readonly injection belongs only to the capture consumer. If this
        // runtime refuses it, fail explicitly instead of replacing rendered text.
        field.SetValue(owner, formatter);
        _originalValues.Add(new FormatterValue(owner, field, original));
    }

    private sealed record FormatterValue(object Owner, FieldInfo Field, IDateFormatter Value);
}
