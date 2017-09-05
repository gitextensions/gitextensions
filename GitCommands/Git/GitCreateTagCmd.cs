using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCommands.Git
{
    public enum TagOperation
    {
        Lightweight = 0,
        Annotate,
        SignWithDefaultKey,
        SignWithSpecificKey
    };

    public static class TagOperationExtensions
    {
        public static bool CanProvideMessage(this TagOperation operationType)
        {
            switch (operationType)
            {
                case TagOperation.Lightweight:
                    return false;
                case TagOperation.Annotate:
                case TagOperation.SignWithDefaultKey:
                case TagOperation.SignWithSpecificKey:
                    return true;
                default:
                    throw new NotSupportedException("Invalid TagOperation: " + operationType);
            }
        }
    }

    public sealed class GitCreateTagArgs
    {
        public string Revision;
        public string TagName;
        public bool Force = false;
        public TagOperation OperationType = TagOperation.Lightweight;
        public string TagMessage = "";
        public string SignKeyId = "";
    }

    public sealed class GitCreateTagCmd : GitCommand
    {
        public GitCreateTagArgs Args;
        public string TagMessageFileName;

        public GitCreateTagCmd(GitCreateTagArgs aArgs)
        {
            Args = aArgs;
        }

        public override string GitComandName()
        {
            return "tag";
        }

        protected override IEnumerable<string> CollectArguments()
        {
            if (Args.Force)
            {
                yield return "-f";
            }

            string operationArg;
            if (GetArgumentForOperation(out operationArg))
            {
                yield return operationArg;
            }

            if (Args.OperationType.CanProvideMessage())
            {
                yield return "-F " + TagMessageFileName.Quote();
            }

            yield return Args.TagName.Trim().Quote();
            yield return "--";
            yield return Args.Revision.Quote();
        }

        private bool GetArgumentForOperation(out string operationArg)
        {
            switch (Args.OperationType)
            {
                /* Lightweight */
                case TagOperation.Lightweight:
                    operationArg = null;
                    return false;

                /* Annotate */
                case TagOperation.Annotate:
                    operationArg = "-a";
                    return true;

                /* Sign with default GPG */
                case TagOperation.SignWithDefaultKey:
                    operationArg = "-s";
                    return true;

                /* Sign with specific GPG */
                case TagOperation.SignWithSpecificKey:
                    operationArg = $"-u {Args.SignKeyId}";
                    return true;

                /* Error */
                default:
                    throw new NotSupportedException("Invalid TagOperation: " + Args.OperationType);
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
            if (string.IsNullOrEmpty(Args.Revision))
            {
                throw new ArgumentNullException("Args.Revision");
            }

            if (string.IsNullOrEmpty(Args.TagName))
            {
                throw new ArgumentNullException("Args.TagName");
            }

            if (Args.OperationType.CanProvideMessage() && TagMessageFileName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("TagMessageFileName");
            }

            if (Args.OperationType == TagOperation.SignWithSpecificKey && string.IsNullOrEmpty(Args.SignKeyId))
            {
                throw new ArgumentNullException("Args.SignKeyId");
            }
        }
    }
}
