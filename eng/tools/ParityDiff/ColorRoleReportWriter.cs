using System.Text;

namespace GitExtensions.ParityDiff;

// parity-scaffolding: Formats strict framework-neutral color-role results for human review.
internal static class ColorRoleReportWriter
{
    public static string Write(ColorRoleResult result)
    {
        StringBuilder report = new();
        report.AppendLine("# Aggregate color-role comparison");
        report.AppendLine();
        report.AppendLine($"- Reference: `{result.ReferenceManifest}`");
        report.AppendLine($"- Candidate: `{result.CandidateManifest}`");
        report.AppendLine($"- Role catalog: `{result.RoleCatalog}`");
        report.AppendLine($"- Requests: {result.Summary.RequestCount}");
        report.AppendLine($"- Compared captures: {result.Summary.ComparedCaptureCount}");
        report.AppendLine($"- Unavailable captures: {result.Summary.UnavailableCaptureCount}");
        report.AppendLine($"- Declared roles: {result.Summary.DeclaredRoleCount}");
        report.AppendLine($"- Role comparisons: {result.Summary.RoleComparisonCount}");
        report.AppendLine($"- Exact matches: {result.Summary.MatchCount}");
        report.AppendLine($"- Findings: {result.Summary.FindingCount}");
        report.AppendLine();
        report.AppendLine("## Role meanings");
        report.AppendLine();
        foreach (ColorRoleDefinition role in result.Roles)
        {
            report.AppendLine($"- `{role.Id}` — {role.Meaning}");
        }

        report.AppendLine();
        foreach (ColorRoleCaptureComparison comparison in result.Captures.Where(
                     item => item.Status != "compared" || item.Findings.Count > 0))
        {
            report.AppendLine($"## {comparison.Key}");
            report.AppendLine();
            report.AppendLine($"Status: `{comparison.Status}`");
            if (comparison.ReferenceNote is not null)
            {
                report.AppendLine($"Reference note: {comparison.ReferenceNote}");
            }

            if (comparison.CandidateNote is not null)
            {
                report.AppendLine($"Candidate note: {comparison.CandidateNote}");
            }

            foreach (ColorRoleFinding finding in comparison.Findings)
            {
                report.AppendLine(
                    $"- **{finding.Code}** `{finding.Role}`: {finding.Message} "
                    + $"(reference `{finding.ReferenceValue ?? "<missing>"}`, "
                    + $"candidate `{finding.CandidateValue ?? "<missing>"}`)");
            }

            report.AppendLine();
        }

        return report.ToString();
    }
}
