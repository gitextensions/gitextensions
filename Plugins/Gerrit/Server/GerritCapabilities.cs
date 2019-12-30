using System;
using System.Collections.Generic;
using ResourceManager;

namespace Gerrit.Server
{
    public class GerritCapabilities : Translate
    {
        #region Translation
        private static readonly TranslationString _publishTypeReview = new TranslationString("For Review");
        private static readonly TranslationString _publishTypeWip = new TranslationString("Work-in-Progress");
        private static readonly TranslationString _publishTypePrivate = new TranslationString("Private");
        private static readonly TranslationString _publishTypeDraft = new TranslationString("Draft");
        #endregion

        private readonly Func<CommandBuilder> _builderFactory;

        private GerritCapabilities(KeyValuePair<string, string>[] publishTypes,
            Func<CommandBuilder> builderFactory)
        {
            PublishTypes = publishTypes;
            _builderFactory = builderFactory;
        }

        public IReadOnlyList<KeyValuePair<string, string>> PublishTypes { get; }

        public CommandBuilder NewBuilder()
            => _builderFactory();

        public static GerritCapabilities Version2_15 { get; }
            = new GerritCapabilities(
                new[]
                {
                    new KeyValuePair<string, string>(_publishTypeReview.Text, ""),
                    new KeyValuePair<string, string>(_publishTypeWip.Text, "wip"),
                    new KeyValuePair<string, string>(_publishTypePrivate.Text, "private"),
                },
                () => new CommandBuilderWithPrivateSupport());

        public static GerritCapabilities OldestVersion { get; }
            = new GerritCapabilities(
                new[]
                {
                    new KeyValuePair<string, string>(_publishTypeReview.Text, ""),
                    new KeyValuePair<string, string>(_publishTypeDraft.Text, CommandBuilderWithDraftSupport.DraftsPublishType),
                },
                () => new CommandBuilderWithDraftSupport());
    }
}
