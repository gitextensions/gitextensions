using System;
using System.Net;
using GitCommands;
using GitUIPluginInterfaces;

namespace ResourceManager
{
    public interface ICommitInformationProvider
    {
        /// <summary>
        /// Gets the commit info from CommitData.
        /// </summary>
        /// <returns></returns>
        CommitInformation Get(CommitData data, bool showRevisionsAsLinks);
    }

    public sealed class CommitInformationProvider : ICommitInformationProvider
    {
        private readonly Func<IGitModule> _getModule;
        private readonly ILinkFactory _linkFactory;
        private readonly ICommitDataHeaderRenderer _commitDataHeaderRenderer;


        public CommitInformationProvider(Func<IGitModule> getModule, ILinkFactory linkFactory, ICommitDataHeaderRenderer commitDataHeaderRenderer)
        {
            _getModule = getModule;
            _linkFactory = linkFactory;
            _commitDataHeaderRenderer = commitDataHeaderRenderer;
        }

        public CommitInformationProvider(Func<IGitModule> getModule)
            : this(getModule, new LinkFactory(), new CommitDataHeaderRenderer(new LinkFactory()))
        {
        }


        /// <summary>
        /// Gets the commit info from CommitData.
        /// </summary>
        /// <returns></returns>
        public CommitInformation Get(CommitData data, bool showRevisionsAsLinks)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var header = _commitDataHeaderRenderer.Render(data, showRevisionsAsLinks);
            var body = "\n" + WebUtility.HtmlEncode((data.Body ?? "").Trim());

            if (showRevisionsAsLinks)
            {
                body = GitRevision.Sha1HashShortRegex.Replace(body, match => ProcessHashCandidate(match.Value));
            }

            return new CommitInformation(header, body);
        }


        private string ProcessHashCandidate(string hash)
        {
            var module = _getModule() as GitModule;
            if (module == null)
                return hash;
            string fullHash;
            if (!module.IsExistingCommitHash(hash, out fullHash))
                return hash;
            return _linkFactory.CreateCommitLink(fullHash, hash, true);
        }
    }
}