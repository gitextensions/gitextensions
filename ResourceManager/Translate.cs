namespace ResourceManager
{
    /// <summary>Provides translation capabilities.</summary>
    public class Translate : ITranslate
    {
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
