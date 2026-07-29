using System.Globalization;
using GitExtensions.ParityCapture;

namespace GitExtensions.ParityDiff;

// parity-scaffolding: Compares temporary capture trees and images.
internal static class CaptureComparer
{
    private const string CaptureCategory = "capture";
    private const string ColorCategory = "color";
    private const string ControlCategory = "control";
    private const string FontCategory = "font";
    private const string GeometryCategory = "geometry";
    private const string ImageCategory = "image";
    private const string LayoutCategory = "layout";
    private const string StateCategory = "state";
    private const string TextCategory = "text";

    public static CaptureComparison Compare(
        CaptureKey key,
        CaptureManifestEntry? referenceEntry,
        CaptureManifestEntry? candidateEntry,
        string referenceDirectory,
        string candidateDirectory,
        DiffTolerance tolerance)
    {
        List<ParityFinding> findings = [];
        if (referenceEntry is null || candidateEntry is null)
        {
            findings.Add(CreateFinding(
                CaptureCategory,
                referenceEntry is null ? "capture.missingReference" : "capture.missingCandidate",
                "$capture",
                "The request is absent from one manifest.",
                referenceEntry is null ? null : "present",
                candidateEntry is null ? null : "present"));
            return CreateUnavailable(key, referenceEntry, candidateEntry, findings);
        }

        if (referenceEntry.Status != CaptureStateStatus.Captured
            || candidateEntry.Status != CaptureStateStatus.Captured)
        {
            if (referenceEntry.Status != candidateEntry.Status
                || referenceEntry.Status == CaptureStateStatus.Failed)
            {
                findings.Add(CreateFinding(
                    CaptureCategory,
                    "capture.status",
                    "$capture",
                    "The requested state is not captured on both sides.",
                    referenceEntry.Status.ToString(),
                    candidateEntry.Status.ToString()));
            }

            return CreateUnavailable(key, referenceEntry, candidateEntry, findings);
        }

        string referenceTreePath = ResolveArtifact(referenceDirectory, referenceEntry.TreeFile, "tree");
        string candidateTreePath = ResolveArtifact(candidateDirectory, candidateEntry.TreeFile, "tree");
        CaptureDocument reference = CaptureJson.Deserialize(File.ReadAllText(referenceTreePath));
        CaptureDocument candidate = CaptureJson.Deserialize(File.ReadAllText(candidateTreePath));
        ValidateDocumentKey(key, reference, "reference");
        ValidateDocumentKey(key, candidate, "candidate");
        CompareDocuments(reference, candidate, tolerance, findings);

        string referenceImagePath = ResolveArtifact(referenceDirectory, referenceEntry.ImageFile, "image");
        string candidateImagePath = ResolveArtifact(candidateDirectory, candidateEntry.ImageFile, "image");
        PixelMetrics pixels = CompareImages(referenceImagePath, candidateImagePath, tolerance.Pixels, findings);
        return new CaptureComparison
        {
            Key = key,
            Status = "compared",
            ReferenceStatus = referenceEntry.Status,
            CandidateStatus = candidateEntry.Status,
            ReferenceNote = referenceEntry.Note,
            CandidateNote = candidateEntry.Note,
            Pixels = pixels,
            Findings = findings
        };
    }

    private static void CompareColumns(
        IReadOnlyList<CaptureColumn> reference,
        IReadOnlyList<CaptureColumn> candidate,
        string path,
        DiffTolerance tolerance,
        ICollection<ParityFinding> findings)
    {
        Dictionary<string, CaptureColumn> candidateColumns = candidate.ToDictionary(GetColumnKey, StringComparer.Ordinal);
        foreach (CaptureColumn referenceColumn in reference)
        {
            string key = GetColumnKey(referenceColumn);
            string columnPath = $"{path}/column[{key}]";
            if (!candidateColumns.Remove(key, out CaptureColumn? candidateColumn))
            {
                findings.Add(CreateFinding(
                    ControlCategory,
                    "column.missing",
                    columnPath,
                    "The reference column is missing from the candidate.",
                    key,
                    null));
                continue;
            }

            CompareDecimal(
                referenceColumn.WidthDip,
                candidateColumn.WidthDip,
                tolerance.GeometryDip,
                GeometryCategory,
                "column.widthDip",
                columnPath,
                findings);
            CompareValue(referenceColumn.DisplayIndex, candidateColumn.DisplayIndex, LayoutCategory, "column.displayIndex", columnPath, findings);
            CompareValue(referenceColumn.Visible, candidateColumn.Visible, StateCategory, "column.visible", columnPath, findings);
            CompareValue(referenceColumn.Resizable, candidateColumn.Resizable, StateCategory, "column.resizable", columnPath, findings);
            CompareValue(referenceColumn.SortMode, candidateColumn.SortMode, StateCategory, "column.sortMode", columnPath, findings);
            CompareValue(referenceColumn.Alignment, candidateColumn.Alignment, LayoutCategory, "column.alignment", columnPath, findings);
            CompareValue(referenceColumn.HeaderText, candidateColumn.HeaderText, TextCategory, "column.headerText", columnPath, findings);
            CompareValue(referenceColumn.HeaderAlignment, candidateColumn.HeaderAlignment, LayoutCategory, "column.headerAlignment", columnPath, findings);
            CompareColors(referenceColumn.Colors, candidateColumn.Colors, columnPath, findings);
        }

        foreach ((string key, CaptureColumn _) in candidateColumns.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            findings.Add(CreateFinding(
                ControlCategory,
                "column.extra",
                $"{path}/column[{key}]",
                "The candidate contains a column absent from the reference.",
                null,
                key));
        }
    }

    private static void CompareColors(
        CaptureColors reference,
        CaptureColors candidate,
        string path,
        ICollection<ParityFinding> findings)
    {
        CompareValue(reference.Foreground, candidate.Foreground, ColorCategory, "color.foreground", path, findings);
        CompareValue(reference.Background, candidate.Background, ColorCategory, "color.background", path, findings);
        CompareValue(reference.Border, candidate.Border, ColorCategory, "color.border", path, findings);
        CompareValue(reference.SelectionForeground, candidate.SelectionForeground, ColorCategory, "color.selectionForeground", path, findings);
        CompareValue(reference.SelectionBackground, candidate.SelectionBackground, ColorCategory, "color.selectionBackground", path, findings);
        CompareValue(reference.InactiveSelectionForeground, candidate.InactiveSelectionForeground, ColorCategory, "color.inactiveSelectionForeground", path, findings);
        CompareValue(reference.InactiveSelectionBackground, candidate.InactiveSelectionBackground, ColorCategory, "color.inactiveSelectionBackground", path, findings);
        CompareValue(reference.DisabledForeground, candidate.DisabledForeground, ColorCategory, "color.disabledForeground", path, findings);
        CompareValue(reference.DisabledBackground, candidate.DisabledBackground, ColorCategory, "color.disabledBackground", path, findings);
        CompareValue(reference.GridLine, candidate.GridLine, ColorCategory, "color.gridLine", path, findings);

        string[] additionalKeys = reference.Additional.Keys
            .Concat(candidate.Additional.Keys)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        foreach (string key in additionalKeys)
        {
            reference.Additional.TryGetValue(key, out string? referenceValue);
            candidate.Additional.TryGetValue(key, out string? candidateValue);
            CompareValue(referenceValue, candidateValue, ColorCategory, $"color.additional.{key}", path, findings);
        }
    }

    private static void CompareDecimal(
        decimal? reference,
        decimal? candidate,
        decimal tolerance,
        string category,
        string code,
        string path,
        ICollection<ParityFinding> findings)
    {
        if (reference is null || candidate is null)
        {
            CompareValue(reference, candidate, category, code, path, findings);
            return;
        }

        decimal delta = Math.Abs(reference.Value - candidate.Value);
        if (delta <= tolerance)
        {
            return;
        }

        findings.Add(new ParityFinding
        {
            Category = category,
            Code = code,
            Path = path,
            Message = "The metric delta exceeds its declared tolerance.",
            ReferenceValue = Format(reference.Value),
            CandidateValue = Format(candidate.Value),
            Delta = Format(delta),
            Tolerance = Format(tolerance)
        });
    }

    private static void CompareDocuments(
        CaptureDocument reference,
        CaptureDocument candidate,
        DiffTolerance tolerance,
        ICollection<ParityFinding> findings)
    {
        CompareValue(reference.Component.TypeName, candidate.Component.TypeName, CaptureCategory, "capture.component", "$capture", findings);
        CompareValue(reference.Capture.Theme.Id, candidate.Capture.Theme.Id, CaptureCategory, "capture.theme", "$capture", findings);
        CompareValue(reference.Capture.Theme.Kind, candidate.Capture.Theme.Kind, CaptureCategory, "capture.themeKind", "$capture", findings);
        CompareValue(reference.Capture.Theme.SourceSha256, candidate.Capture.Theme.SourceSha256, CaptureCategory, "capture.themeSource", "$capture", findings);
        CompareValue(reference.Capture.ScalePercent, candidate.Capture.ScalePercent, CaptureCategory, "capture.scale", "$capture", findings);
        CompareValue(reference.Capture.Dpi.X, candidate.Capture.Dpi.X, CaptureCategory, "capture.dpiX", "$capture", findings);
        CompareValue(reference.Capture.Dpi.Y, candidate.Capture.Dpi.Y, CaptureCategory, "capture.dpiY", "$capture", findings);
        CompareValue(reference.Capture.State, candidate.Capture.State, CaptureCategory, "capture.state", "$capture", findings);

        Dictionary<string, CaptureSurface> candidateSurfaces = candidate.Surfaces
            .ToDictionary(surface => surface.Role, StringComparer.Ordinal);
        foreach (CaptureSurface referenceSurface in reference.Surfaces)
        {
            string path = $"surface[{referenceSurface.Role}]";
            if (!candidateSurfaces.Remove(referenceSurface.Role, out CaptureSurface? candidateSurface))
            {
                findings.Add(CreateFinding(
                    ControlCategory,
                    "surface.missing",
                    path,
                    "The reference surface is missing from the candidate.",
                    referenceSurface.Role,
                    null));
                continue;
            }

            CompareNode(referenceSurface.Root, candidateSurface.Root, $"{path}/root", tolerance, findings);
            CompareFieldNodes(referenceSurface.Root, candidateSurface.Root, path, tolerance, findings);
            CompareFocusOrder(referenceSurface.Root, candidateSurface.Root, path, findings);
        }

        foreach (string role in candidateSurfaces.Keys.Order(StringComparer.Ordinal))
        {
            findings.Add(CreateFinding(
                ControlCategory,
                "surface.extra",
                $"surface[{role}]",
                "The candidate contains a surface absent from the reference.",
                null,
                role));
        }
    }

    private static void CompareFieldNodes(
        CaptureNode referenceRoot,
        CaptureNode candidateRoot,
        string surfacePath,
        DiffTolerance tolerance,
        ICollection<ParityFinding> findings)
    {
        List<CaptureNode> referenceNodes = Flatten(referenceRoot).Where(node => node.FieldName is not null).ToList();
        List<CaptureNode> candidateNodes = Flatten(candidateRoot).Where(node => node.FieldName is not null).ToList();
        Dictionary<string, CaptureNode> candidatesByField = candidateNodes
            .ToDictionary(node => node.FieldName!, StringComparer.Ordinal);
        HashSet<CaptureNode> matchedCandidates = new(ReferenceEqualityComparer.Instance);
        foreach (CaptureNode referenceNode in referenceNodes)
        {
            CaptureNode? candidateNode = null;
            bool aliased = false;
            if (candidatesByField.TryGetValue(referenceNode.FieldName!, out CaptureNode? exact))
            {
                candidateNode = exact;
            }
            else
            {
                candidateNode = candidateNodes.SingleOrDefault(
                    node => !matchedCandidates.Contains(node)
                            && (node.FieldAliases.Contains(referenceNode.FieldName!, StringComparer.Ordinal)
                                || referenceNode.FieldAliases.Contains(node.FieldName!, StringComparer.Ordinal)));
                aliased = candidateNode is not null;
            }

            string path = $"{surfacePath}/control[{referenceNode.FieldName}]";
            if (candidateNode is null)
            {
                candidateNode = FindRenameCandidate(referenceNode, candidateNodes, matchedCandidates);
                aliased = candidateNode is not null;
            }

            if (candidateNode is null)
            {
                findings.Add(CreateFinding(
                    ControlCategory,
                    "control.missing",
                    path,
                    "The reference field is missing from the candidate.",
                    referenceNode.FieldName,
                    null));
                continue;
            }

            matchedCandidates.Add(candidateNode);
            if (aliased || !string.Equals(referenceNode.FieldName, candidateNode.FieldName, StringComparison.Ordinal))
            {
                findings.Add(CreateFinding(
                    ControlCategory,
                    "control.renamed",
                    path,
                    "The control joined through an alias or a unique semantic match.",
                    referenceNode.FieldName,
                    candidateNode.FieldName));
            }

            CompareNode(referenceNode, candidateNode, path, tolerance, findings);
        }

        foreach (CaptureNode candidateNode in candidateNodes.Where(node => !matchedCandidates.Contains(node)))
        {
            findings.Add(CreateFinding(
                ControlCategory,
                "control.extra",
                $"{surfacePath}/control[{candidateNode.FieldName}]",
                "The candidate field is absent from the reference.",
                null,
                candidateNode.FieldName));
        }
    }

    private static void CompareFocusOrder(
        CaptureNode reference,
        CaptureNode candidate,
        string path,
        ICollection<ParityFinding> findings)
    {
        string referenceOrder = GetFocusOrder(reference);
        string candidateOrder = GetFocusOrder(candidate);
        CompareValue(referenceOrder, candidateOrder, LayoutCategory, "focus.order", path, findings);
    }

    private static void CompareFont(
        CaptureFont? reference,
        CaptureFont? candidate,
        string path,
        DiffTolerance tolerance,
        ICollection<ParityFinding> findings)
    {
        if (reference is null || candidate is null)
        {
            CompareValue(
                reference is null ? null : "present",
                candidate is null ? null : "present",
                FontCategory,
                "font.presence",
                path,
                findings);
            return;
        }

        CompareValue(reference.Family, candidate.Family, FontCategory, "font.family", path, findings);
        CompareDecimal(reference.SizePoints, candidate.SizePoints, tolerance.FontSizePoints, FontCategory, "font.sizePoints", path, findings);
        CompareValue(
            string.Join(",", reference.Style.Order(StringComparer.Ordinal)),
            string.Join(",", candidate.Style.Order(StringComparer.Ordinal)),
            FontCategory,
            "font.style",
            path,
            findings);
    }

    private static PixelMetrics CompareImages(
        string referencePath,
        string candidatePath,
        PixelTolerance tolerance,
        ICollection<ParityFinding> findings)
    {
        PngImage reference = PngImage.Load(referencePath);
        PngImage candidate = PngImage.Load(candidatePath);
        PixelMetrics metrics = PixelComparer.Compare(reference, candidate, tolerance.MaximumChannelDelta);
        if (reference.Width != candidate.Width || reference.Height != candidate.Height)
        {
            findings.Add(CreateFinding(
                ImageCategory,
                "image.dimensions",
                "$image",
                "The captured image dimensions differ.",
                $"{reference.Width}x{reference.Height}",
                $"{candidate.Width}x{candidate.Height}"));
        }

        if (metrics.Ssim < tolerance.MinimumSsim)
        {
            findings.Add(new ParityFinding
            {
                Category = ImageCategory,
                Code = "image.ssim",
                Path = "$image",
                Message = "SSIM is below the declared minimum.",
                ReferenceValue = "1",
                CandidateValue = metrics.Ssim.ToString("0.######", CultureInfo.InvariantCulture),
                Delta = (1 - metrics.Ssim).ToString("0.######", CultureInfo.InvariantCulture),
                Tolerance = tolerance.MinimumSsim.ToString("0.######", CultureInfo.InvariantCulture)
            });
        }

        if (metrics.DifferentPixelFraction > tolerance.MaximumDifferentPixelFraction)
        {
            findings.Add(new ParityFinding
            {
                Category = ImageCategory,
                Code = "image.differentPixelFraction",
                Path = "$image",
                Message = "The per-pixel difference exceeds its declared budget.",
                CandidateValue = metrics.DifferentPixelFraction.ToString("0.######", CultureInfo.InvariantCulture),
                Tolerance = tolerance.MaximumDifferentPixelFraction.ToString("0.######", CultureInfo.InvariantCulture)
            });
        }

        if (metrics.MaximumChannelDelta > tolerance.MaximumChannelDelta)
        {
            findings.Add(new ParityFinding
            {
                Category = ImageCategory,
                Code = "image.maximumChannelDelta",
                Path = "$image",
                Message = "A channel delta exceeds its declared per-channel budget.",
                CandidateValue = metrics.MaximumChannelDelta.ToString(CultureInfo.InvariantCulture),
                Tolerance = tolerance.MaximumChannelDelta.ToString(CultureInfo.InvariantCulture)
            });
        }

        return metrics;
    }

    private static void CompareNode(
        CaptureNode reference,
        CaptureNode candidate,
        string path,
        DiffTolerance tolerance,
        ICollection<ParityFinding> findings)
    {
        CompareValue(reference.ControlKind, candidate.ControlKind, ControlCategory, "control.kind", path, findings);
        CompareRectangle(reference.BoundsDip, candidate.BoundsDip, $"{path}/boundsDip", tolerance.GeometryDip, findings);
        CompareSize(reference.ClientSizeDip, candidate.ClientSizeDip, $"{path}/clientSizeDip", tolerance.GeometryDip, findings);
        CompareThickness(reference.Padding.Dip, candidate.Padding.Dip, $"{path}/paddingDip", tolerance.GeometryDip, findings);
        CompareThickness(reference.Margin.Dip, candidate.Margin.Dip, $"{path}/marginDip", tolerance.GeometryDip, findings);
        CompareFont(reference.Font, candidate.Font, path, tolerance, findings);
        CompareColors(reference.Colors, candidate.Colors, path, findings);
        CompareValue(reference.BorderStyle, candidate.BorderStyle, LayoutCategory, "border.style", path, findings);
        CompareValue(reference.FlatStyle, candidate.FlatStyle, LayoutCategory, "border.flatStyle", path, findings);
        CompareDecimal(reference.BorderWidthDip, candidate.BorderWidthDip, tolerance.BorderWidthDip, LayoutCategory, "border.widthDip", path, findings);
        CompareCornerRadius(reference.CornerRadiusDip, candidate.CornerRadiusDip, path, tolerance.CornerRadiusDip, findings);
        CompareValue(string.Join(",", reference.Anchor), string.Join(",", candidate.Anchor), LayoutCategory, "layout.anchor", path, findings);
        CompareValue(reference.Dock, candidate.Dock, LayoutCategory, "layout.dock", path, findings);
        CompareValue(reference.AutoSize, candidate.AutoSize, LayoutCategory, "layout.autoSize", path, findings);
        CompareValue(reference.Alignment, candidate.Alignment, LayoutCategory, "layout.alignment", path, findings);
        CompareValue(reference.Text, candidate.Text, TextCategory, "text.value", path, findings);
        CompareValue(reference.ToolTip, candidate.ToolTip, TextCategory, "text.tooltip", path, findings);
        CompareValue(reference.TranslationSource, candidate.TranslationSource, TextCategory, "text.translationSource", path, findings);
        CompareValue(reference.TabIndex, candidate.TabIndex, LayoutCategory, "tab.index", path, findings);
        CompareValue(reference.TabStop, candidate.TabStop, LayoutCategory, "tab.stop", path, findings);
        CompareValue(reference.Enabled, candidate.Enabled, StateCategory, "state.enabled", path, findings);
        CompareValue(reference.Visible, candidate.Visible, StateCategory, "state.visible", path, findings);
        CompareValue(reference.Focused, candidate.Focused, StateCategory, "state.focused", path, findings);
        CompareValue(reference.ReadOnly, candidate.ReadOnly, StateCategory, "state.readOnly", path, findings);
        CompareValue(reference.CheckState, candidate.CheckState, StateCategory, "state.checkState", path, findings);
        CompareValue(reference.Selected, candidate.Selected, StateCategory, "state.selected", path, findings);
        CompareValue(reference.Expanded, candidate.Expanded, StateCategory, "state.expanded", path, findings);
        CompareColumns(reference.Columns, candidate.Columns, path, tolerance, findings);
    }

    private static void CompareCornerRadius(
        CaptureCornerRadius? reference,
        CaptureCornerRadius? candidate,
        string path,
        decimal tolerance,
        ICollection<ParityFinding> findings)
    {
        if (reference is null || candidate is null)
        {
            CompareValue(
                reference is null ? null : "present",
                candidate is null ? null : "present",
                LayoutCategory,
                "cornerRadius.presence",
                path,
                findings);
            return;
        }

        CompareDecimal(reference.TopLeft, candidate.TopLeft, tolerance, LayoutCategory, "cornerRadius.topLeft", path, findings);
        CompareDecimal(reference.TopRight, candidate.TopRight, tolerance, LayoutCategory, "cornerRadius.topRight", path, findings);
        CompareDecimal(reference.BottomRight, candidate.BottomRight, tolerance, LayoutCategory, "cornerRadius.bottomRight", path, findings);
        CompareDecimal(reference.BottomLeft, candidate.BottomLeft, tolerance, LayoutCategory, "cornerRadius.bottomLeft", path, findings);
    }

    private static void CompareRectangle(
        CaptureRectangleF reference,
        CaptureRectangleF candidate,
        string path,
        decimal tolerance,
        ICollection<ParityFinding> findings)
    {
        CompareDecimal(reference.X, candidate.X, tolerance, GeometryCategory, "geometry.x", path, findings);
        CompareDecimal(reference.Y, candidate.Y, tolerance, GeometryCategory, "geometry.y", path, findings);
        CompareDecimal(reference.Width, candidate.Width, tolerance, GeometryCategory, "geometry.width", path, findings);
        CompareDecimal(reference.Height, candidate.Height, tolerance, GeometryCategory, "geometry.height", path, findings);
    }

    private static void CompareSize(
        CaptureSizeF reference,
        CaptureSizeF candidate,
        string path,
        decimal tolerance,
        ICollection<ParityFinding> findings)
    {
        CompareDecimal(reference.Width, candidate.Width, tolerance, GeometryCategory, "geometry.width", path, findings);
        CompareDecimal(reference.Height, candidate.Height, tolerance, GeometryCategory, "geometry.height", path, findings);
    }

    private static void CompareThickness(
        CaptureThicknessF reference,
        CaptureThicknessF candidate,
        string path,
        decimal tolerance,
        ICollection<ParityFinding> findings)
    {
        CompareDecimal(reference.Left, candidate.Left, tolerance, GeometryCategory, "geometry.left", path, findings);
        CompareDecimal(reference.Top, candidate.Top, tolerance, GeometryCategory, "geometry.top", path, findings);
        CompareDecimal(reference.Right, candidate.Right, tolerance, GeometryCategory, "geometry.right", path, findings);
        CompareDecimal(reference.Bottom, candidate.Bottom, tolerance, GeometryCategory, "geometry.bottom", path, findings);
    }

    private static void CompareValue<T>(
        T reference,
        T candidate,
        string category,
        string code,
        string path,
        ICollection<ParityFinding> findings)
    {
        if (EqualityComparer<T>.Default.Equals(reference, candidate))
        {
            return;
        }

        findings.Add(CreateFinding(
            category,
            code,
            path,
            "The exact values differ.",
            Format(reference),
            Format(candidate)));
    }

    private static CaptureComparison CreateUnavailable(
        CaptureKey key,
        CaptureManifestEntry? reference,
        CaptureManifestEntry? candidate,
        IReadOnlyList<ParityFinding> findings) =>
        new()
        {
            Key = key,
            Status = "unavailable",
            ReferenceStatus = reference?.Status,
            CandidateStatus = candidate?.Status,
            ReferenceNote = reference?.Note,
            CandidateNote = candidate?.Note,
            Findings = findings
        };

    private static ParityFinding CreateFinding(
        string category,
        string code,
        string path,
        string message,
        string? reference,
        string? candidate) =>
        new()
        {
            Category = category,
            Code = code,
            Path = path,
            Message = message,
            ReferenceValue = reference,
            CandidateValue = candidate
        };

    private static CaptureNode? FindRenameCandidate(
        CaptureNode reference,
        IEnumerable<CaptureNode> candidates,
        IReadOnlySet<CaptureNode> matched)
    {
        CaptureNode[] semanticMatches = candidates
            .Where(candidate => !matched.Contains(candidate)
                                && string.Equals(reference.ControlKind, candidate.ControlKind, StringComparison.Ordinal)
                                && ((!string.IsNullOrEmpty(reference.TranslationSource)
                                     && string.Equals(reference.TranslationSource, candidate.TranslationSource, StringComparison.Ordinal))
                                    || (!string.IsNullOrEmpty(reference.Text)
                                        && string.Equals(reference.Text, candidate.Text, StringComparison.Ordinal))))
            .ToArray();
        return semanticMatches.Length == 1 ? semanticMatches[0] : null;
    }

    private static IEnumerable<CaptureNode> Flatten(CaptureNode root)
    {
        yield return root;
        foreach (CaptureNode child in root.Children)
        {
            foreach (CaptureNode descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }

    private static string? Format<T>(T value) =>
        value switch
        {
            null => null,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };

    private static string GetColumnKey(CaptureColumn column) =>
        column.FieldName ?? column.Name ?? column.Index.ToString(CultureInfo.InvariantCulture);

    private static string GetFocusOrder(CaptureNode root) =>
        string.Join(
            ",",
            Flatten(root)
                .Select((node, index) => (Node: node, Index: index))
                .Where(item => item.Node.FieldName is not null
                               && item.Node.TabStop == true
                               && item.Node.Enabled != false
                               && item.Node.Visible != false)
                .OrderBy(item => item.Node.TabIndex ?? int.MaxValue)
                .ThenBy(item => item.Index)
                .Select(item => item.Node.FieldName));

    private static string ResolveArtifact(string manifestDirectory, string? relativePath, string kind)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new InvalidDataException($"A captured manifest entry does not name its {kind} artifact.");
        }

        string path = Path.GetFullPath(Path.Combine(manifestDirectory, relativePath));
        string root = Path.GetFullPath(manifestDirectory) + Path.DirectorySeparatorChar;
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!path.StartsWith(root, comparison) || !File.Exists(path))
        {
            throw new InvalidDataException($"The {kind} artifact '{relativePath}' is missing or outside its manifest directory.");
        }

        return path;
    }

    private static void ValidateDocumentKey(CaptureKey key, CaptureDocument document, string side)
    {
        if (!string.Equals(key.ComponentType, document.Component.TypeName, StringComparison.Ordinal)
            || !string.Equals(key.ThemeId, document.Capture.Theme.Id, StringComparison.Ordinal)
            || key.ScalePercent != document.Capture.ScalePercent
            || !string.Equals(key.State, document.Capture.State, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"The {side} tree metadata does not match manifest key '{key}'.");
        }
    }
}
