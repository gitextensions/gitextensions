using System;

namespace ResourceManager
{
    /// <summary>Provides translation capabilities.</summary>
    public class Translate : ITranslate
    {
        void IDisposable.Dispose()
        {
        }

        public virtual void AddTranslationItems(ITranslation translation)
        {
            TranslationUtils.AddTranslationItemsFromFields(GetType().Name, this, translation);
        }

        public virtual void TranslateItems(ITranslation translation)
        {
            TranslationUtils.TranslateItemsFromFields(GetType().Name, this, translation);
        }
    }
}
