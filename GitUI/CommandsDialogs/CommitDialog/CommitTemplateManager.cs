using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GitUI.CommandsDialogs.CommitDialog
{
    internal sealed class CommitTemplateManager
    {
        private static readonly ConcurrentDictionary<string, Func<string>> RegisteredTemplatesDic = new ConcurrentDictionary<string, Func<string>>();
        public IEnumerable<CommitTemplateItem> RegisteredTemplates => RegisteredTemplatesDic.Select(item => new CommitTemplateItem(item.Key, item.Value()));

        public void Register(string templateName, Func<string> templateText)
        {
            if (RegisteredTemplatesDic.ContainsKey(templateName))
                return;

            RegisteredTemplatesDic.TryAdd(templateName, templateText);
        }

        public void Unregister(string templateName)
        {
            Func<string> notUsed;
            RegisteredTemplatesDic.TryRemove(templateName, out notUsed);
        }
    }
}