﻿using System;
using System.Text;
using BugReporter.Serialization;
using GitExtensions;
using GitExtUtils;

namespace BugReporter
{
    public interface IErrorReportMarkDownBodyBuilder
    {
        string Build(SerializableException exception, string? environmentInfo, string? additionalInfo);
    }

    public sealed class ErrorReportMarkDownBodyBuilder : IErrorReportMarkDownBodyBuilder
    {
        public string Build(SerializableException exception, string? environmentInfo, string? additionalInfo)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var sb = new StringBuilder();

            sb.AppendLine(@"<!--
    :warning: Review existing issues to see whether someone else has already reported your issue.
-->

:warning: The sections below must be filled in and this text must be removed or the issue will be closed.

## Current behaviour

<!-- Be as specific and detailed as possible to help us identify your issue. -->


## Expected behaviour


## Steps to reproduce

<!-- Take some time to try and reproduce the issue, then explain how to do so here. -->


## Error Details");
            sb.AppendLine("```");
            sb.AppendLine(exception.ToString());
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine();

            if (!Strings.IsNullOrWhiteSpace(additionalInfo))
            {
                sb.AppendLine("## Additional information");
                sb.AppendLine(additionalInfo.Trim());
                sb.AppendLine();
                sb.AppendLine();
            }

            try
            {
                sb.AppendLine("## Environment");

                if (!Strings.IsNullOrWhiteSpace(environmentInfo))
                {
                    sb.AppendLine(environmentInfo);
                }
                else
                {
                    sb.AppendLine("System information is not supplied");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to retrieve system information.");
                sb.AppendLine("Exception:");
                sb.AppendLine("```");
                sb.AppendLine(ex.ToString());
                sb.AppendLine("```");
            }

            return sb.ToString();
        }
    }
}
