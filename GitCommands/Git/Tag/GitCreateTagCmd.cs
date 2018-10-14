using System;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Git.Tag
{
    public sealed class GitCreateTagCmd : GitCommand
    {
        [CanBeNull]
        public string TagMessageFileName { get; }
        public GitCreateTagArgs CreateTagArguments { get; }

        public GitCreateTagCmd(GitCreateTagArgs args, [CanBeNull] string tagMessageFileName)
        {
            CreateTagArguments = args;
            TagMessageFileName = tagMessageFileName;
        }

        public override bool AccessesRemote => false;
        public override bool ChangesRepoState => true;

        protected override ArgumentString BuildArguments()
        {
            return new GitArgumentBuilder("tag")
            {
                { CreateTagArguments.Force, "-f" },
                GetArgumentForOperation(),
                { CreateTagArguments.Operation.CanProvideMessage(), $"-F {TagMessageFileName.Quote()}" },
                CreateTagArguments.TagName.Trim().Quote(),
                "--",
                CreateTagArguments.ObjectId
            };

            string GetArgumentForOperation()
            {
                switch (CreateTagArguments.Operation)
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
                        return $"-u {CreateTagArguments.SignKeyId}";

                    /* Error */
                    default:
                        throw new NotSupportedException($"Invalid tag operation: {CreateTagArguments.Operation}");
                }
            }
        }

        public override void Validate()
        {
            if (CreateTagArguments.ObjectId == null)
            {
                throw new ArgumentException("Revision is required.");
            }

            if (string.IsNullOrWhiteSpace(CreateTagArguments.TagName))
            {
                throw new ArgumentException("TagName is required.");
            }

            if (CreateTagArguments.Operation.CanProvideMessage() && string.IsNullOrWhiteSpace(TagMessageFileName))
            {
                throw new ArgumentException("TagMessageFileName is required.");
            }

            if (CreateTagArguments.Operation == TagOperation.SignWithSpecificKey && string.IsNullOrWhiteSpace(CreateTagArguments.SignKeyId))
            {
                throw new ArgumentException("SignKeyId is required.");
            }
        }
    }
}
