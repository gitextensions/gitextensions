// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitHub.cs" company="Git Extensions">
//   Copyright (c) 2019 Igor Velikorossov. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace GitUI.NBugReports
{
    public sealed class GitHubUrlBuilder
    {
        private readonly IErrorReportMarkDownBodyBuilder _errorReportMarkDownBodyBuilder;

        public GitHubUrlBuilder(IErrorReportMarkDownBodyBuilder errorReportMarkDownBodyBuilder)
        {
            _errorReportMarkDownBodyBuilder = errorReportMarkDownBodyBuilder;
        }

        /// <summary>
        /// Generates a URL to create a new issue on GitHub.
        /// </summary>
        /// <see href="https://help.github.com/en/articles/about-automation-for-issues-and-pull-requests-with-query-parameters"/>
        public string Build(string url, Exception exception, string environmentInfo, string additionalInfo)
        {
            if (string.IsNullOrWhiteSpace(url) || exception == null)
            {
                return null;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri validatedUri) ||
                !(validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps))
            {
                return null;
            }

            string separator = !string.IsNullOrWhiteSpace(validatedUri.Query) ? "&" : "?";

            string subject = $"[NBug] {exception.Message}";
            if (subject.Length > 69)
            {
                // if the subject is longer than 70 characters, trim it
                subject = subject.Substring(0, 66) + "...";
            }

            string body = Uri.EscapeDataString(_errorReportMarkDownBodyBuilder.Build(exception, environmentInfo, additionalInfo));

            return $"{validatedUri}{separator}title={Uri.EscapeDataString(subject)}&body={body}";
        }
    }
}