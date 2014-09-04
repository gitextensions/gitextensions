using System;

namespace ResourceManager
{
    public interface ITranslation
    {
        void AddTranslationItem(string category, string item, string property, string neutralValue);

        string TranslateItem(string category, string item, string property, Func<string> provideDefaultValue);
    }
}