namespace GitCommands.Git
{
    public struct AheadBehindData
    {
        public string Branch { get; set; }
        public string AheadCount { get; set; }
        public string BehindCount { get; set; }

        public string ToDisplay()
        {
            return (!string.IsNullOrEmpty(AheadCount) ? AheadCount + "↑" : string.Empty)
                   + (!string.IsNullOrEmpty(AheadCount) && !string.IsNullOrEmpty(BehindCount) ? " " : string.Empty)
                   + (!string.IsNullOrEmpty(BehindCount) ? BehindCount + "↓" : string.Empty);
        }
    }
}