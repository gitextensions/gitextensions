using System.Text;

namespace GitExtensions.ParityDiff;

// parity-scaffolding: Formats temporary parity findings for human review.
internal static class HumanReportWriter
{
    public static string Write(ParityDiffResult result)
    {
        StringBuilder report = new();
        report.AppendLine("# Parity comparison");
        report.AppendLine();
        report.AppendLine($"- Reference: `{result.ReferenceManifest}`");
        report.AppendLine($"- Candidate: `{result.CandidateManifest}`");
        report.AppendLine($"- Requests: {result.Summary.RequestCount}");
        report.AppendLine($"- Compared captures: {result.Summary.ComparedCaptureCount}");
        report.AppendLine($"- Unavailable captures: {result.Summary.UnavailableCaptureCount}");
        report.AppendLine($"- Findings: {result.Summary.FindingCount}");
        report.AppendLine();

        foreach (CaptureComparison comparison in result.Captures.Where(
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

            report.AppendLine();
            foreach (ParityFinding finding in comparison.Findings)
            {
                report.Append($"- **{finding.Code}** `{finding.Path}`: {finding.Message}");
                if (finding.ReferenceValue is not null || finding.CandidateValue is not null)
                {
                    report.Append($" (reference `{finding.ReferenceValue ?? "<missing>"}`, candidate `{finding.CandidateValue ?? "<missing>"}`)");
                }

                report.AppendLine();
            }

            report.AppendLine();
        }

        return report.ToString();
    }
}
