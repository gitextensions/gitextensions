using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitUI.HelperDialogs;
using GitUI.Properties;

namespace GitExtensionsTests;

// parity-scaffolding: Verifies the no-services construction path used by the Avalonia designer.
[TestFixture]
[NonParallelizable]
public sealed class PreviewabilityTests
{
    private const string EvidencePathEnvironmentVariable = "GITEXT_PREVIEWABILITY_REPORT";

    [AvaloniaTest]
    [Category("P0_8B")]
    public void Every_AXAML_view_should_construct_in_design_mode_without_runtime_services()
    {
        IReadOnlyList<ViewDescriptor> views = GetViewDescriptors();
        List<PreviewabilityResult> results = [];
        bool originalDesignMode = Design.IsDesignMode;
        SetDesignMode(true);
        try
        {
            foreach (ViewDescriptor view in views)
            {
                results.Add(Construct(view));
            }
        }
        finally
        {
            SetDesignMode(originalDesignMode);
        }

        WriteEvidence(results);
        PreviewabilityResult[] failures = results.Where(result => result.Status == "failed").ToArray();
        failures.Should().BeEmpty(
            "every AXAML view must construct without commands, services, a repository, or network access; "
            + string.Join(
                Environment.NewLine,
                failures.Select(failure =>
                    $"{failure.Path} ({failure.ClassName}): {failure.ErrorType}: {failure.ErrorMessage}")));
    }

    [Test]
    [Category("P0_8B")]
    public void Every_AXAML_control_type_used_by_markup_should_have_a_public_parameterless_constructor()
    {
        string repositoryRoot = FindRepositoryRoot();
        IReadOnlyList<Assembly> assemblies = LoadPortableViewAssemblies();
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        List<string> inaccessibleTypes = [];
        foreach (string path in GetAxamlPaths(repositoryRoot))
        {
            XDocument document = XDocument.Load(path, LoadOptions.None);
            foreach (XElement element in document.Root!.DescendantsAndSelf())
            {
                string xmlNamespace = element.Name.NamespaceName;
                if (!xmlNamespace.StartsWith("using:", StringComparison.Ordinal))
                {
                    continue;
                }

                if (element.Attribute(xaml + "Class") is not null)
                {
                    continue;
                }

                string typeName = $"{xmlNamespace["using:".Length..]}.{element.Name.LocalName}";
                Type? type = assemblies
                    .Select(assembly => assembly.GetType(typeName, throwOnError: false))
                    .FirstOrDefault(candidate => candidate is not null);
                if (type is not null
                    && typeof(Control).IsAssignableFrom(type)
                    && type.GetConstructor(Type.EmptyTypes) is null)
                {
                    inaccessibleTypes.Add(
                        $"{Normalize(Path.GetRelativePath(repositoryRoot, path))}: {type.FullName}");
                }
            }
        }

        inaccessibleTypes.Should().BeEmpty(
            "the out-of-process Avalonia previewer constructs markup controls from a generated assembly");
    }

    private static PreviewabilityResult Construct(ViewDescriptor view)
    {
        if (view.ViewType is null)
        {
            return PreviewabilityResult.Failed(
                view,
                typeof(TypeLoadException).FullName!,
                $"The AXAML class '{view.ClassName}' could not be resolved from the portable UI assemblies.");
        }

        try
        {
            object instance = Activator.CreateInstance(view.ViewType)
                ?? throw new InvalidOperationException($"Activator returned null for '{view.ClassName}'.");
            if (instance is not Control control)
            {
                throw new InvalidOperationException(
                    $"The AXAML class '{view.ClassName}' is not an Avalonia control.");
            }

            if (control is Window window)
            {
                // FormStatus follows the original by replacing the app icon with operation-state badges.
                if (window is not FormStatus && !ReferenceEquals(window.Icon, Images.ApplicationIcon))
                {
                    throw new InvalidOperationException(
                        $"The AXAML window '{view.ClassName}' does not use the Git Extensions application icon.");
                }

                window.Close();
            }

            if (control is IDisposable disposable)
            {
                disposable.Dispose();
            }

            return PreviewabilityResult.Passed(view);
        }
        catch (Exception exception)
        {
            Exception root = Unwrap(exception);
            return PreviewabilityResult.Failed(
                view,
                root.GetType().FullName ?? root.GetType().Name,
                Normalize(root.Message));
        }
    }

    private static IReadOnlyList<ViewDescriptor> GetViewDescriptors(
        [CallerFilePath] string thisFilePath = "")
    {
        string repositoryRoot = FindRepositoryRoot(thisFilePath);
        IReadOnlyList<Assembly> assemblies = LoadPortableViewAssemblies();
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        List<ViewDescriptor> views = [];
        foreach (string path in GetAxamlPaths(repositoryRoot))
        {
            XDocument document = XDocument.Load(path, LoadOptions.None);
            string? className = document.Root?.Attribute(xaml + "Class")?.Value;
            if (string.IsNullOrWhiteSpace(className))
            {
                continue;
            }

            Type? viewType = assemblies
                .Select(assembly => assembly.GetType(className, throwOnError: false))
                .FirstOrDefault(type => type is not null);
            views.Add(new ViewDescriptor(
                Normalize(Path.GetRelativePath(repositoryRoot, path)),
                className,
                viewType));
        }

        return views;
    }

    private static IEnumerable<string> GetAxamlPaths(string repositoryRoot)
    {
        string[] sourceRoots =
        [
            Path.Combine(repositoryRoot, "src", "app", "GitUI.Avalonia"),
            Path.Combine(repositoryRoot, "src", "plugins"),
        ];
        return sourceRoots
            .SelectMany(root => Directory.EnumerateFiles(root, "*.axaml", SearchOption.AllDirectories))
            .Order(StringComparer.Ordinal);
    }

    private static IReadOnlyList<Assembly> LoadPortableViewAssemblies()
    {
        string[] assemblyPaths = Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll")
            .Where(path =>
            {
                string name = Path.GetFileNameWithoutExtension(path);
                return name == "GitUI.Avalonia"
                       || name.StartsWith("GitExtensions.Plugins.", StringComparison.Ordinal);
            })
            .Order(StringComparer.Ordinal)
            .ToArray();
        return assemblyPaths
            .Select(path => AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(path)))
            .ToArray();
    }

    private static void WriteEvidence(IReadOnlyList<PreviewabilityResult> results)
    {
        string? outputPath = Environment.GetEnvironmentVariable(EvidencePathEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return;
        }

        string fullPath = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        PreviewabilityReport report = new()
        {
            SchemaVersion = 1,
            Summary = new PreviewabilitySummary
            {
                ViewCount = results.Count,
                PassedCount = results.Count(result => result.Status == "passed"),
                FailedCount = results.Count(result => result.Status == "failed"),
            },
            Views = results,
        };
        File.WriteAllText(
            fullPath,
            JsonSerializer.Serialize(
                report,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                }) + Environment.NewLine);
    }

    private static Exception Unwrap(Exception exception)
    {
        while (exception is TargetInvocationException { InnerException: not null }
               or TypeInitializationException { InnerException: not null })
        {
            exception = exception.InnerException;
        }

        return exception;
    }

    private static void SetDesignMode(bool value)
    {
        PropertyInfo property = typeof(Design).GetProperty(
            nameof(Design.IsDesignMode),
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Avalonia's design-mode property could not be resolved.");
        property.SetValue(null, value);
    }

    private static string FindRepositoryRoot(
        [CallerFilePath] string startPath = "")
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(startPath)!);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"Could not find the repository root from '{startPath}'.");
    }

    private static string Normalize(string value) =>
        value.Replace('\\', '/');

    private sealed record ViewDescriptor(
        string Path,
        string ClassName,
        Type? ViewType);

    private sealed record PreviewabilityReport
    {
        public required int SchemaVersion { get; init; }

        public required PreviewabilitySummary Summary { get; init; }

        public required IReadOnlyList<PreviewabilityResult> Views { get; init; }
    }

    private sealed record PreviewabilitySummary
    {
        public required int ViewCount { get; init; }

        public required int PassedCount { get; init; }

        public required int FailedCount { get; init; }
    }

    private sealed record PreviewabilityResult
    {
        public required string Path { get; init; }

        public required string ClassName { get; init; }

        public required string Assembly { get; init; }

        public required string Status { get; init; }

        public string? ErrorType { get; init; }

        public string? ErrorMessage { get; init; }

        public static PreviewabilityResult Passed(ViewDescriptor view) =>
            new()
            {
                Path = view.Path,
                ClassName = view.ClassName,
                Assembly = view.ViewType!.Assembly.GetName().Name!,
                Status = "passed",
            };

        public static PreviewabilityResult Failed(
            ViewDescriptor view,
            string errorType,
            string errorMessage) =>
            new()
            {
                Path = view.Path,
                ClassName = view.ClassName,
                Assembly = view.ViewType?.Assembly.GetName().Name ?? "",
                Status = "failed",
                ErrorType = errorType,
                ErrorMessage = errorMessage,
            };
    }
}
