using System;
using System.Collections.Generic;

namespace GitCommands.Git.Tag
{
    public sealed class GitCreateTagCmd : GitCommand
    {
        public GitCreateTagCmd(GitCreateTagArgs args, string tagMessageFileName)
        {
            Arguments = args;
            TagMessageFileName = tagMessageFileName;
        }

        public GitCreateTagArgs Arguments { get; }

        public string TagMessageFileName { get; }

        public override string GitComandName()
        {
            return "tag";
        }

        protected override IEnumerable<string> CollectArguments()
        {
            if (Arguments.Force)
            {
                yield return "-f";
            }

            string operationArg = GetArgumentForOperation();
            if (!string.IsNullOrWhiteSpace(operationArg))
            {
                yield return operationArg;
            }

            if (Arguments.Operation.CanProvideMessage())
            {
                yield return "-F " + TagMessageFileName.Quote();
            }

            yield return Arguments.TagName.Trim().Quote();
            yield return "--";
            yield return Arguments.Revision.Quote();
        }

        private string GetArgumentForOperation()
        {
            switch (Arguments.Operation)
            {
                /* Lightweight */
                case TagOperation.Lightweight:
                    return null;

                /* Annotate */
                case TagOperation.Annotate:
                    return "-a";

                /* Sign with default GPG */
                case TagOperation.SignWithDefaultKey:
                    return "-s";

                /* Sign with specific GPG */
                case TagOperation.SignWithSpecificKey:
                    return $"-u {Arguments.SignKeyId}";

                /* Error */
                default:
                    throw new NotSupportedException($"Invalid tag operation: {Arguments.Operation}");
            }
        }

        public override bool AccessesRemote()
        {
            return false;
        }

        public override bool ChangesRepoState()
        {
            return true;
        }

        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(Arguments.Revision))
            {
                throw new ArgumentException("Revision is required.");
            }

            if (string.IsNullOrWhiteSpace(Arguments.TagName))
            {
                throw new ArgumentException("TagName is required.");
            }

            if (Arguments.Operation.CanProvideMessage() && string.IsNullOrWhiteSpace(TagMessageFileName))
            {
                throw new ArgumentException("TagMessageFileName is required.");
            }

            if (Arguments.Operation == TagOperation.SignWithSpecificKey && string.IsNullOrWhiteSpace(Arguments.SignKeyId))
            {
                throw new ArgumentException("SignKeyId is required.");
            }
        }
    }
}
