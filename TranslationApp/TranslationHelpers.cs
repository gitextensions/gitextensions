using GitExtensions.Extensibility.Translations;
using GitExtensions.Extensibility.Translations.Xliff;

namespace TranslationApp
{
    internal static class TranslationHelpers
    {
        public static IDictionary<string, List<TranslationItemWithCategory>> LoadNeutralItems()
        {
            IDictionary<string, TranslationFile> neutralTranslation = new Dictionary<string, TranslationFile>();
            try
            {
                // Set language to neutral to get neutral translations
                GitCommands.AppSettings.CurrentTranslation = "";

                Dictionary<string, List<Type>> translatableTypes = TranslationUtil.GetTranslatableTypes();
                foreach ((string key, List<Type> types) in translatableTypes)
                {
                    TranslationFile translation = new();
                    try
                    {
                        foreach (Type type in types)
                        {
                            try
                            {
                                if (TranslationUtil.CreateInstanceOfClass(type) is ITranslate obj)
                                {
                                    obj.AddTranslationItems(translation);
                                    if (obj is IDisposable disposable)
                                    {
                                        disposable.Dispose();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"ERROR instantiating type {type.FullName}:");
                                Console.WriteLine(ex);
                            }
                        }
                    }
                    finally
                    {
                        translation.Sort();
                        neutralTranslation[key] = translation;
                    }
                }
            }
            finally
            {
                // Restore translation
                GitCommands.AppSettings.CurrentTranslation = null;
            }

            return GetItemsDictionary(neutralTranslation);
        }

        public static IDictionary<string, List<TranslationItemWithCategory>> GetItemsDictionary(IDictionary<string, TranslationFile> translations)
        {
            Dictionary<string, List<TranslationItemWithCategory>> items = [];
            foreach ((string key, TranslationFile file) in translations)
            {
                IEnumerable<TranslationItemWithCategory> list = from item in file.TranslationCategories
                           from translationItem in item.Body.TranslationItems
                           select new TranslationItemWithCategory(item.Name, translationItem);
                items.Add(key, list.ToList());
            }

            return items;
        }

        private static List<T> Find<T>(this IDictionary<string, List<T>> dictionary, string key)
        {
            if (!dictionary.TryGetValue(key, out List<T> list))
            {
                list = [];
                dictionary.Add(key, list);
            }

            return list;
        }

        public static IDictionary<string, List<TranslationItemWithCategory>> LoadTranslation(
            IDictionary<string, TranslationFile> translation, IDictionary<string, List<TranslationItemWithCategory>> neutralItems)
        {
            Dictionary<string, List<TranslationItemWithCategory>> translateItems = [];

            IDictionary<string, List<TranslationItemWithCategory>> oldTranslationItems = GetItemsDictionary(translation);

            foreach ((string key, List<TranslationItemWithCategory> items) in neutralItems)
            {
                List<TranslationItemWithCategory> oldItems = oldTranslationItems.Find(key);
                List<TranslationItemWithCategory> transItems = translateItems.Find(key);
                Dictionary<string, string> dict = [];
                foreach (TranslationItemWithCategory item in items)
                {
                    IEnumerable<TranslationItemWithCategory> curItems = oldItems.Where(
                        oldItem => oldItem.Category.TrimStart('_') == item.Category.TrimStart('_') &&
                                  oldItem.Name.TrimStart('_') == item.Name.TrimStart('_') &&
                                  oldItem.Property == item.Property);
                    TranslationItemWithCategory curItem = curItems.FirstOrDefault();

                    if (curItem is null)
                    {
                        curItem = item.Clone();
                        transItems.Add(curItem);
                        continue;
                    }

                    oldItems.Remove(curItem);
                    curItem.Category = item.Category;
                    curItem.Name = item.Name;

                    string source = curItem.NeutralValue ?? item.NeutralValue;
                    if (!string.IsNullOrEmpty(curItem.TranslatedValue) && !dict.ContainsKey(source))
                    {
                        dict.Add(source, curItem.TranslatedValue);
                    }

                    // Source text changed
                    if (!string.IsNullOrEmpty(curItem.TranslatedValue) && !curItem.IsSourceEqual(item.NeutralValue))
                    {
                        curItem.TranslatedValue = "";
                    }

                    curItem.NeutralValue = item.NeutralValue;
                    transItems.Add(curItem);
                }

                foreach (TranslationItemWithCategory item in oldItems)
                {
                    // Obsolete should be added only to dictionary
                    if (!string.IsNullOrEmpty(item.TranslatedValue) &&
                        item.NeutralValue is not null && !dict.ContainsKey(item.NeutralValue))
                    {
                        dict.Add(item.NeutralValue, item.TranslatedValue);
                    }
                }

                // update untranslated items
                IEnumerable<TranslationItemWithCategory> untranslatedItems =
                    from transItem in transItems
                    where string.IsNullOrEmpty(transItem.TranslatedValue) && dict.ContainsKey(transItem.NeutralValue)
                    select transItem;

                foreach (TranslationItemWithCategory untranslatedItem in untranslatedItems)
                {
                    untranslatedItem.TranslatedValue = dict[untranslatedItem.NeutralValue];
                }
            }

            return translateItems;
        }

        public static void SaveTranslation(string targetLanguageCode,
            IDictionary<string, List<TranslationItemWithCategory>> items, string filename)
        {
            string ext = Path.GetExtension(filename);

            foreach ((string key, List<TranslationItemWithCategory> translateItems) in items)
            {
                TranslationFile foreignTranslation = new(GitCommands.AppSettings.ProductVersion, "en", targetLanguageCode);
                foreach (TranslationItemWithCategory translateItem in translateItems)
                {
                    TranslationItem item = translateItem.GetTranslationItem();

                    TranslationItem ti = new(item.Name, item.Property, item.Source, item.Value);
                    ti.Value ??= string.Empty;
                    foreignTranslation.FindOrAddTranslationCategory(translateItem.Category)
                        .Body.AddTranslationItem(ti);
                }

                string newFileName = Path.ChangeExtension(filename, key + ext);
                TranslationSerializer.Serialize(foreignTranslation, newFileName);
            }
        }
    }
}
