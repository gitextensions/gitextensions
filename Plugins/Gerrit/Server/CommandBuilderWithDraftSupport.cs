namespace Gerrit.Server
{
    public class CommandBuilderWithDraftSupport : CommandBuilder
    {
        public const string DraftsPublishType = @"drafts";

        public override CommandBuilder WithPublishType(string publishType)
        {
            if (publishType == DraftsPublishType)
            {
                TargetRef = DraftsPublishType;
            }

            return this;
        }
    }
}
