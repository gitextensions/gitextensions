using GitUIPluginInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

    public interface IGitTagController
    {
        /// <summary>
        /// Create the Tag depending on input parameter.
        /// </summary>
        /// <param name="revision">Commit revision to be tagged</param>
        /// <param name="tagName">Name of tag</param>
        /// <param name="force">Force parameter</param>
        /// <param name="operationType">The operation to perform on the tag  (Lightweight, Annotate, Sign with defaul key, Sign with specific key)</param>
        /// <param name="tagMessage">Tag Message</param>
        /// <param name="keyId">Specific Key ID to be used instead of default one</param>
        /// <returns>Output string from RunGitCmd.</returns>
        string CreateTag(string revision, string tagName, bool force, TagOperation operationType = TagOperation.Lightweight, string tagMessage = "", string keyId = "");
    }


    public class GitTagController : IGitTagController
    {
        private IGitModule _module;

        public GitTagController(IGitModule module)
        {
            if(module == null)
            {
                throw new ArgumentNullException("module");
            }

            _module = module;
        }

        /// <summary>
        /// Create the Tag depending on input parameter.
        /// </summary>
        /// <param name="revision">Commit revision to be tagged</param>
        /// <param name="inputTagName">Name of tag</param>
        /// <param name="force">Force parameter</param>
        /// <param name="operationType">The operation to perform on the tag (Lightweight, Annotate, Sign with defaul key, Sign with specific key)</param>
        /// <param name="tagMessage">Tag Message</param>
        /// <param name="keyId">Specific Key ID to be used instead of default one</param>
        /// <returns>Output string from RunGitCmd.</returns>
        public string CreateTag(string revision, string inputTagName, bool force, TagOperation operationType = TagOperation.Lightweight, string tagMessage = "", string keyId = "")
        {
            if (string.IsNullOrEmpty(revision))
            {
                throw new ArgumentNullException("revision");
            }

            if (string.IsNullOrEmpty(inputTagName))
            {
                throw new ArgumentNullException("tagName");
            }

            string tagMessageFileName = Path.Combine(_module.GetGitDirectory(), "TAGMESSAGE");

            if (operationType.CanProvideMessage())
            {
                File.WriteAllText(tagMessageFileName, tagMessage);
            }

            string tagName = inputTagName.Trim();
            string forced = force ? "-f" : "";

            string tagSwitch = "";

            switch (operationType)
            {
                /* Lightweight */
                case TagOperation.Lightweight:
                    /* tagSwitch is already ok */
                    break;

                /* Annotate */
                case TagOperation.Annotate:
                    tagSwitch = "-a";
                    break;

                /* Sign with default GPG */
                case TagOperation.SignWithDefaultKey:
                    tagSwitch = "-s";
                    break;

                /* Sign with specific GPG */
                case TagOperation.SignWithSpecificKey:
                    if(string.IsNullOrEmpty(keyId))
                    {
                        throw new ArgumentNullException("keyId");
                    }
                    tagSwitch = $"-u {keyId}";
                    break;
                    
                /* Error */
                default:
                    throw new NotSupportedException("Invalid TagOperation: " + operationType);
            }

            if (operationType.CanProvideMessage())
            {
                tagSwitch = tagSwitch + " -F " + tagMessageFileName.Quote();
            }

            return _module.RunGitCmd($"tag {forced} {tagSwitch} \"{tagName}\" -- \"{revision}\"");
        }
    }
}
