﻿using System.ComponentModel;

namespace GitUI.Design
{
    internal class PropertySorter : ExpandableObjectConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(value, attributes);
            List<(string, int)> orderedProperties = new();
            foreach (PropertyDescriptor pd in pdc)
            {
                Attribute attribute = pd.Attributes[typeof(PropertyOrderAttribute)];
                if (attribute is not null)
                {
                    PropertyOrderAttribute poa = (PropertyOrderAttribute)attribute;
                    orderedProperties.Add((pd.Name, poa.Order));
                }
                else
                {
                    orderedProperties.Add((pd.Name, 100));
                }
            }

            return pdc.Sort(orderedProperties.OrderBy(p => p.Item2).Select(p => p.Item1).ToArray());
        }
    }
}
