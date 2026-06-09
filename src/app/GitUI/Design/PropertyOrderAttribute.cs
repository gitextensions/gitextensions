namespace GitUI.Design;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class PropertyOrderAttribute : Attribute
{
    public PropertyOrderAttribute(int order)
    {
        Order = order;
    }

    public int Order { get; }
}
