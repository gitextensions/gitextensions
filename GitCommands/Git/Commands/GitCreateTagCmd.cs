using System;
using GitCommands.Git.Extensions;
using GitCommands.Git.Tag;
using GitExtUtils;

namespace GitCommands.Git.Commands
{
    public sealed class GitCreateTagCmd : GitCommand
    {
        public string? TagMessageFileName { get; }
        public GitCreateTagArgs CreateTagArguments { get; }

        public GitCreateTagCmd(GitCreateTagArgs args, string? tagMessageFileName)
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

            string? GetArgumentForOperation()
            {
                return CreateTagArguments.Operation switch
                {
                    /* Lightweight */
                    TagOperation.Lightweight => null,
                    /* Annotate */
                    TagOperation.Annotate => "-a",
                    /* Sign with default GPG */
                    TagOperation.SignWithDefaultKey => "-s",
                    /* Sign with specific GPG */
                    TagOperation.SignWithSpecificKey => $"-u {CreateTagArguments.SignKeyId}",
                    _ => throw new NotSupportedException($"Invalid tag operation: {CreateTagArguments.Operation}")
                };
            }
        }

        public override void Validate()
        {
            if (CreateTagArguments.ObjectId is null)
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
