using System.Composition;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Shared, PartNotDiscoverable]
    [Export(typeof(ILinkFactory))]
    internal class MockLinkFactory : ILinkFactory
    {
        private readonly ILinkFactory _linkFactory = new LinkFactory();

        public string? LastExecutedLinkUri { get; private set; }

        public string CreateBranchLink(string noPrefixBranch)
            => _linkFactory.CreateBranchLink(noPrefixBranch);

        public string CreateCommitLink(ObjectId objectId, string? linkText = null, bool preserveGuidInLinkText = false)
            => _linkFactory.CreateCommitLink(objectId, linkText, preserveGuidInLinkText);

        public string CreateLink(string? caption, string uri)
            => _linkFactory.CreateLink(caption, uri);

        public string CreateShowAllLink(string what)
            => _linkFactory.CreateShowAllLink(what);

        public string CreateTagLink(string tag)
            => _linkFactory.CreateTagLink(tag);

        public void ExecuteLink(string? linkUri, Action<CommandEventArgs>? handleInternalLink = null, Action<string?>? showAll = null)
        {
            LastExecutedLinkUri = linkUri;

            _linkFactory.ExecuteLink(linkUri, handleInternalLink, showAll);
        }
    }
}
