﻿using ResourceManager;
using ResourceManager.Xliff;

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

                var translatableTypes = TranslationUtil.GetTranslatableTypes();
                foreach (var (key, types) in translatableTypes)
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
                            catch
                            {
                                // no-op
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
            Dictionary<string, List<TranslationItemWithCategory>> items = new();
            foreach (var (key, file) in translations)
            {
                var list = from item in file.TranslationCategories
                           from translationItem in item.Body.TranslationItems
                           select new TranslationItemWithCategory(item.Name, translationItem);
                items.Add(key, list.ToList());
            }

            return items;
        }

        private static List<T> Find<T>(this IDictionary<string, List<T>> dictionary, string key)
        {
            if (!dictionary.TryGetValue(key, out var list))
            {
                list = new List<T>();
                dictionary.Add(key, list);
            }

            return list;
        }

        public static IDictionary<string, List<TranslationItemWithCategory>> LoadTranslation(
            IDictionary<string, TranslationFile> translation, IDictionary<string, List<TranslationItemWithCategory>> neutralItems)
        {
            Dictionary<string, List<TranslationItemWithCategory>> translateItems = new();

            var oldTranslationItems = GetItemsDictionary(translation);

            foreach (var (key, items) in neutralItems)
            {
                var oldItems = oldTranslationItems.Find(key);
                var transItems = translateItems.Find(key);
                Dictionary<string, string> dict = new();
                foreach (var item in items)
                {
                    var curItems = oldItems.Where(
                        oldItem => oldItem.Category.TrimStart('_') == item.Category.TrimStart('_') &&
                                  oldItem.Name.TrimStart('_') == item.Name.TrimStart('_') &&
                                  oldItem.Property == item.Property);
                    var curItem = curItems.FirstOrDefault();

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

                foreach (var item in oldItems)
                {
                    // Obsolete should be added only to dictionary
                    if (!string.IsNullOrEmpty(item.TranslatedValue) &&
                        item.NeutralValue is not null && !dict.ContainsKey(item.NeutralValue))
                    {
                        dict.Add(item.NeutralValue, item.TranslatedValue);
                    }
                }

                // update untranslated items
                var untranslatedItems =
                    from transItem in transItems
                    where string.IsNullOrEmpty(transItem.TranslatedValue) && dict.ContainsKey(transItem.NeutralValue)
                    select transItem;

                foreach (var untranslatedItem in untranslatedItems)
                {
                    untranslatedItem.TranslatedValue = dict[untranslatedItem.NeutralValue];
                }
            }

            return translateItems;
        }

        public static void SaveTranslation(string targetLanguageCode,
            IDictionary<string, List<TranslationItemWithCategory>> items, string filename)
        {
            var ext = Path.GetExtension(filename);

            foreach (var (key, translateItems) in items)
            {
                TranslationFile foreignTranslation = new(GitCommands.AppSettings.ProductVersion, "en", targetLanguageCode);
                foreach (var translateItem in translateItems)
                {
                    var item = translateItem.GetTranslationItem();

                    TranslationItem ti = new(item.Name, item.Property, item.Source, item.Value);
                    ti.Value ??= string.Empty;
                    foreignTranslation.FindOrAddTranslationCategory(translateItem.Category)
                        .Body.AddTranslationItem(ti);
                }

                var newFileName = Path.ChangeExtension(filename, key + ext);
                TranslationSerializer.Serialize(foreignTranslation, newFileName);
            }
        }
    }
}
