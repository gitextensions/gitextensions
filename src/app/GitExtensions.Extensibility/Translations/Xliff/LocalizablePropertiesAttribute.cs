namespace GitExtensions.Extensibility.Translations.Xliff;

/// <summary>
///  When placed on a control, overrides the list of properties <see cref="TranslationUtil._translatableItemInComponentNames"/> included in localization.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class LocalizablePropertiesAttribute : Attribute
{
    public string[] TranslatableProperties { get; }

    public LocalizablePropertiesAttribute(params string[] translatableProperties)
    {
        TranslatableProperties = translatableProperties;
    }
}
