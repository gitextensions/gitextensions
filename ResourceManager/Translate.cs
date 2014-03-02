namespace ResourceManager
{
    /// <summary>Provides translation capabilities.</summary>
    public class Translate : ITranslate
    {
        public virtual void AddTranslationItems(ITranslation translation)
        {
            TranslationUtl.AddTranslationItemsFromFields(GetType().Name, this, translation);
        }

        public virtual void TranslateItems(ITranslation translation)
        {
            TranslationUtl.TranslateItemsFromFields(GetType().Name, this, translation);
        }
    }
}
