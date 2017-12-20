namespace ResourceManager
{
    public class CommitInformation
    {
        public CommitInformation(string header, string body)
        {
            Header = header;
            Body = body;
        }

        public string Header { get; }
        public string Body { get; }
    }
}
