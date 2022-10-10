// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitHub.cs" company="Git Extensions">
//   Copyright (c) 2019 Igor Velikorossov. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using BugReporter.Serialization;

namespace BugReporter
{
    public sealed class GitHubUrlBuilder
    {
        private readonly IErrorReportUrlBuilder _errorReportUrlBuilder;

        public GitHubUrlBuilder(IErrorReportUrlBuilder errorReportUrlBuilder)
        {
            _errorReportUrlBuilder = errorReportUrlBuilder;
        }

        /// <summary>
        /// Generates a URL to create a new issue on GitHub.
        /// <see href="https://help.github.com/en/articles/about-automation-for-issues-and-pull-requests-with-query-parameters"/>
        /// </summary>
        /// <param name="url">url to create a new issue for the project.</param>
        /// <param name="exception">The exception to report.</param>
        /// <param name="exceptionInfo">Info string for GE specific exceptions (like GitExtUtils.ExternalOperationException) unknown in BugReporter.</param>
        /// <param name="environmentInfo">Git version, directory etc.</param>
        /// <param name="additionalInfo">Information from the popup textbox.</param>
        /// <returns>The URL as a string</returns>
        public string? Build(string url, SerializableException exception, string exceptionInfo, string? environmentInfo, string? additionalInfo)
        {
            if (string.IsNullOrWhiteSpace(url) || exception is null)
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

            string urlEncodedError = _errorReportUrlBuilder.Build(exception, exceptionInfo, environmentInfo, additionalInfo);
            return $"{validatedUri}{separator}title={Uri.EscapeDataString(subject)}&{urlEncodedError}";
        }
    }
}
