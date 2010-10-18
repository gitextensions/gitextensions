namespace GitCommands
{
    public class ShortDiffDto
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Result { get; set; }

        public ShortDiffDto(string from, string to)
        {
            From = from;
            To = to;
        }
    }

    public class ShortDiff
    {
        public ShortDiffDto Dto { get; set; }
        public ShortDiff(ShortDiffDto dto)
        {
            Dto = dto;
        }

        public void Execute()
        {
            Dto.Result = GitCommandHelpers.RunCmd(Settings.GitCommand, "diff -z " + Dto.From + " " + Dto.To + " --shortstat");
        }
    }
}
