#nullable enable

using System.ComponentModel;
using System.Linq;
using GitUI.Shells;

namespace GitUI
{
    internal sealed class ResourceImagesTypeConverter : StringConverter
    {
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var value = (string)context.PropertyDescriptor.GetValue(context.Instance);

            return new StandardValuesCollection(ResourceImagesProvider.Images.Select(x => x.Key).ToArray());
        }
    }
}
