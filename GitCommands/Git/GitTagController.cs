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
        /// <param name="operation">The operation to perform on the tag (Lightweight, Annotate, Sign with defaul key, Sign with specific key)</param>
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

            if (operationType > TagOperation.Lightweight)
            {
                File.WriteAllText(Path.Combine(_module.GetGitDirectory(), "TAGMESSAGE"), tagMessage);
            }

            string tagName = inputTagName.Trim();
            string forced = force ? "-f" : "";
            string directory = _module.GetGitDirectory();

            string tagSwitch = "";

            switch (operationType)
            {
                /* Lightweight */
                case TagOperation.Lightweight:
                    /* tagSwitch is already ok */
                    break;

                /* Annotate */
                case TagOperation.Annotate:
                    tagSwitch = $"-a -F \"{directory}\\TAGMESSAGE\"";
                    break;

                /* Sign with default GPG */
                case TagOperation.SignWithDefaultKey:
                    tagSwitch = $"-s -F \"{directory}\\TAGMESSAGE\"";
                    break;

                /* Sign with specific GPG */
                case TagOperation.SignWithSpecificKey:
                    if(string.IsNullOrEmpty(keyId))
                    {
                        throw new ArgumentNullException("keyId");
                    }
                    tagSwitch = $"-u {keyId} -F \"{directory}\\TAGMESSAGE\"";
                    break;
                    
                /* Error */
                default:
                    throw new NotSupportedException("Invalid TagOperation");
            }

            return _module.RunGitCmd($"tag {forced} {tagSwitch} \"{tagName}\" -- \"{revision}\"");
        }
    }
}
