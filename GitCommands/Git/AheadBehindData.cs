namespace GitCommands.Git
{
    public struct AheadBehindData
    {
        // gone: "plumbing" expression, see https://git-scm.com/docs/git-for-each-ref#Documentation/git-for-each-ref.txt-upstream
        public static readonly string Gone = "gone";
        public string Branch { get; set; }
        public string RemoteRef { get; set; }
        public string AheadCount { get; set; }
        public string BehindCount { get; set; }

        public string ToDisplay()
        {
            return AheadCount == Gone
                ? AheadBehindData.Gone
                : AheadCount == "0" && string.IsNullOrEmpty(BehindCount)
                ? "0↑↓"
                : (!string.IsNullOrEmpty(AheadCount) && AheadCount != "0"
                    ? AheadCount + "↑" + (!string.IsNullOrEmpty(BehindCount) ? " " : string.Empty)
                    : string.Empty)
                + (!string.IsNullOrEmpty(BehindCount) ? BehindCount + "↓" : string.Empty);
        }
    }
}
