using System;

namespace GitUI.Design
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class PropertyOrderAttribute : Attribute
    {
        public PropertyOrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }
    }
}
