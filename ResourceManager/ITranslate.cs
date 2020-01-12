using System;

namespace ResourceManager
{
    /// <summary>Provides translation capabilities.</summary>
    public interface ITranslate : IDisposable
    {
        void AddTranslationItems(ITranslation translation);

        /// <summary>Translates all (translatable) items.</summary>
        void TranslateItems(ITranslation translation);
    }
}
