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
        Annotate = 1,
        SignWithDefaultKey = 2,
        SignWithSpecifiKey = 3
    };

    public interface IGitTagController
    {
        /// <summary>
        /// Create the Tag depending on input parameter.
        /// </summary>
        /// <param name="revision">Commit revision to be tagged</param>
        /// <param name="tagName">Name of tag</param>
        /// <param name="force">Force parameter</param>
        /// <param name="operationType">Available tag operationType (Lightweight, Annotate, Sign with defaul key, Sign with specific key)</param>
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
            _module = module ?? throw new ArgumentNullException("module");
        }

        /// <summary>
        /// Create the Tag depending on input parameter.
        /// </summary>
        /// <param name="revision">Commit revision to be tagged</param>
        /// <param name="tagName">Name of tag</param>
        /// <param name="force">Force parameter</param>
        /// <param name="operation">Available tag operationType (Lightweight, Annotate, Sign with defaul key, Sign with specific key)</param>
        /// <param name="tagMessage">Tag Message</param>
        /// <param name="keyId">Specific Key ID to be used instead of default one</param>
        /// <returns>Output string from RunGitCmd.</returns>
        public string CreateTag(string revision, string tagName, bool force, TagOperation operationType = TagOperation.Lightweight, string tagMessage = "", string keyId = "")
        {
            if (string.IsNullOrEmpty(revision))
            {
                throw new ArgumentNullException("revision");
            }

            if (string.IsNullOrEmpty(tagName))
            {
                throw new ArgumentNullException("tagName");
            }

            if (operationType > TagOperation.Lightweight)
            {
                File.WriteAllText(Path.Combine(_module.GetGitDirectory(), "TAGMESSAGE"), tagMessage);
            }

            string strCommand = "";
            string _tagName = tagName.Trim();
            string _forced = force ? " - f" : "";
            string _directory = _module.GetGitDirectory();

            switch (operationType)
            {
                /* Lightweight */
                case TagOperation.Lightweight:
                    strCommand = $"tag {_forced} \"{_tagName}\" \"{revision}\"";
                    break;

                /* Annotate */
                case TagOperation.Annotate:
                    strCommand = $"tag \"{_tagName}\" -a {_forced} -F \"{_directory}\\TAGMESSAGE\" -- \"{revision}\"";
                    break;

                /* Sign with default GPG */
                case TagOperation.SignWithDefaultKey:
                    strCommand = $"tag \"{_tagName}\" -s {_forced} -F \"{_directory}\\TAGMESSAGE\" -- \"{revision}\"";
                    break;

                /* Sign with specific GPG */
                case TagOperation.SignWithSpecifiKey:
                    if(string.IsNullOrEmpty(keyId))
                    {
                        throw new ArgumentNullException("keyId");
                    }
                    strCommand = $"tag \"{_tagName}\" -u {keyId} {_forced} -F \"{_directory}\\TAGMESSAGE\" -- \"{revision}\"";
                    break;
                    
                /* Error */
                default:
                    throw new NotSupportedException("Invalid TagOperation");
            }

            return _module.RunGitCmd(strCommand);
        }
    }
}
