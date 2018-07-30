namespace GitCommands.ExternalLinks
{
    public sealed class ExternalLink
    {
        public string Caption { get; }
        public string Uri { get; }

        public ExternalLink(string caption, string uri)
        {
            Caption = caption;
            Uri = uri;
        }

        public override bool Equals(object obj) => obj is ExternalLink link && string.Equals(Uri, link.Uri);
        public override int GetHashCode() => Uri.GetHashCode();
    }
}
