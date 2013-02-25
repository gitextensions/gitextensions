namespace ResourceManager.Translation
{
    /// <summary>Provides translation capabilities.</summary>
    public interface ITranslate
    {
        void AddTranslationItems(Translation translation);
        /// <summary>Translates all (translatable) items.</summary>
        void TranslateItems(Translation translation);
    }
}
