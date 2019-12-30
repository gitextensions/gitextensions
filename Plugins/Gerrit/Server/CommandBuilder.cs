using System;
using System.Collections.Generic;
using System.Linq;

namespace Gerrit.Server
{
    public abstract class CommandBuilder
    {
        protected string TargetRef = "for";
        protected List<string> CommandArguments { get; } = new List<string>();

        private static IEnumerable<string> SplitAndComposeArguments(string argumentName, string values)
        {
            values = values?.Trim();
            if (string.IsNullOrEmpty(values))
            {
                return Enumerable.Empty<string>();
            }

            return ComposeArguments(argumentName, values.Split(new[] { ' ', ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static IEnumerable<string> ComposeArguments(string argumentName, string[] values)
        {
            return values.Where(text => !string.IsNullOrEmpty(text))
                .Select(text => $"{argumentName}={text}");
        }

        private static IEnumerable<string> ComposeArgument(string argumentName, string value)
        {
            return ComposeArguments(argumentName, new[] { value?.Trim() });
        }

        public CommandBuilder WithReviewers(string text)
        {
            CommandArguments.AddRange(SplitAndComposeArguments("r", text));
            return this;
        }

        public CommandBuilder WithCC(string text)
        {
            CommandArguments.AddRange(SplitAndComposeArguments("cc", text));
            return this;
        }

        public CommandBuilder WithTopic(string topic)
        {
            CommandArguments.AddRange(ComposeArgument(@"topic", topic));
            return this;
        }

        public CommandBuilder WithHashTag(string hashTag)
        {
            CommandArguments.AddRange(ComposeArgument(@"hashtag", hashTag));
            return this;
        }

        public abstract CommandBuilder WithPublishType(string publishType);

        public virtual string Build(string branch)
        {
            string targetBranch = $"refs/{TargetRef}/{branch}";
            if (CommandArguments.Count > 0)
            {
                targetBranch += "%" + string.Join(",", CommandArguments);
            }

            return targetBranch;
        }
    }
}
