namespace Gerrit.Server
{
    public class CommandBuilderWithPrivateSupport : CommandBuilder
    {
        private string _publishType;

        public override CommandBuilder WithPublishType(string publishType)
        {
            _publishType = publishType?.Trim();

            return this;
        }

        public override string Build(string branch)
        {
            if (!string.IsNullOrEmpty(_publishType))
            {
                CommandArguments.Add(_publishType);
            }

            return base.Build(branch);
        }
    }
}
