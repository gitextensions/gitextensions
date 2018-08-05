using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ResourceManager.Xliff
{
    public static class TranslationUtil
    {
        private const BindingFlags _propertyFlags
            = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static |
              BindingFlags.NonPublic | BindingFlags.SetProperty;
        private const BindingFlags _fieldFlags
            = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static |
              BindingFlags.NonPublic | BindingFlags.SetField;

        private static bool AllowTranslateProperty(string text)
        {
            if (text == null)
            {
                return false;
            }

            return text.Any(char.IsLetter);
        }

        public static IEnumerable<(string name, object item)> GetObjFields(object obj, string objName)
        {
            if (objName != null)
            {
                yield return (objName, obj);
            }

            foreach (var field in obj.GetType().GetFields(_fieldFlags))
            {
                if (field.IsPublic && !field.IsInitOnly)
                {
                    // if public AND modifiable (NOT readonly)
                    Trace.WriteLine(string.Format("Skip field {0}.{1} [{2}]", obj.GetType().Name, field.Name, field.GetValue(obj)), "Translation");
                    continue;
                }

                yield return (field.Name, field.GetValue(obj));
            }
        }

        public static void AddTranslationItem(string category, object obj, string propName, ITranslation translation)
        {
            var property = obj?.GetType().GetProperty(propName, _propertyFlags);

            if (property?.GetValue(obj, null) is string value && AllowTranslateProperty(value))
            {
                translation.AddTranslationItem(category, propName, "Text", value);
            }
        }

        public static void AddTranslationItemsFromFields(string category, object obj, ITranslation translation)
        {
            if (obj == null)
            {
                return;
            }

            AddTranslationItemsFromList(category, translation, GetObjFields(obj, "$this"));
        }

        private static IEnumerable<PropertyInfo> GetItemPropertiesEnumerator(string name, object item)
        {
            if (item == null)
            {
                yield break;
            }

            // Skip controls with a name started with "_NO_TRANSLATE_"
            // this is a naming convention, these are not translated
            if (name.StartsWith("_NO_TRANSLATE_"))
            {
                yield break;
            }

            Func<PropertyInfo, bool> isTranslatable;
            if (item is DataGridViewColumn c)
            {
                isTranslatable = property => IsTranslatableItemInDataGridViewColumn(property, c);
            }
            else if (item is ComboBox || item is ListBox)
            {
                isTranslatable = property => IsTranslatableItemInBox(property, item);
            }
            else
            {
                isTranslatable = IsTranslatableItemInComponent;
            }

            foreach (var property in item.GetType().GetProperties(_fieldFlags).Where(isTranslatable))
            {
                yield return property;
            }
        }

        public static void AddTranslationItemsFromList(string category, ITranslation translation, IEnumerable<(string name, object item)> items)
        {
            foreach (var (itemName, itemObj) in items)
            {
                foreach (var property in GetItemPropertiesEnumerator(itemName, itemObj))
                {
                    var value = property.GetValue(itemObj, null);

                    if (value == null)
                    {
                        continue;
                    }

                    if (value is string valueStr)
                    {
                        if (AllowTranslateProperty(valueStr))
                        {
                            translation.AddTranslationItem(category, itemName, property.Name, valueStr);
                        }

                        continue;
                    }

                    if (value is IList listItems)
                    {
                        for (int index = 0; index < listItems.Count; index++)
                        {
                            var listItem = listItems[index] as string;

                            if (AllowTranslateProperty(listItem))
                            {
                                translation.AddTranslationItem(category, itemName, "Item" + index, listItem);
                            }
                        }
                    }
                }
            }
        }

        public static void TranslateItemsFromList(string category, ITranslation translation, IEnumerable<(string name, object item)> items)
        {
            foreach (var (itemName, itemObj) in items)
            {
                foreach (var propertyInfo in GetItemPropertiesEnumerator(itemName, itemObj))
                {
                    var property = propertyInfo; // copy for lambda

                    if (property.Name == "Items" && typeof(IList).IsAssignableFrom(property.PropertyType))
                    {
                        var list = (IList)property.GetValue(itemObj, null);
                        for (int index = 0; index < list.Count; index++)
                        {
                            if (list[index] is string listValue)
                            {
                                string ProvideDefaultValue() => listValue;
                                string value = translation.TranslateItem(category, itemName, "Item" + index,
                                    ProvideDefaultValue);

                                if (!string.IsNullOrEmpty(value))
                                {
                                    list[index] = value;
                                }
                            }
                        }
                    }
                    else if (property.PropertyType.IsEquivalentTo(typeof(string)))
                    {
                        string ProvideDefaultValue() => (string)property.GetValue(itemObj, null);
                        string value = translation.TranslateItem(category, itemName, property.Name, ProvideDefaultValue);

                        if (!string.IsNullOrEmpty(value))
                        {
                            if (property.CanWrite)
                            {
                                property.SetValue(itemObj, value, null);
                            }
                        }
                        else if (property.Name == "ToolTipText" &&
                                 !string.IsNullOrEmpty((string)property.GetValue(itemObj, null)))
                        {
                            value = translation.TranslateItem(category, itemName, "Text", ProvideDefaultValue);
                            if (!string.IsNullOrEmpty(value))
                            {
                                if (property.CanWrite)
                                {
                                    property.SetValue(itemObj, value, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void TranslateProperty(string category, object obj, string propName, ITranslation translation)
        {
            if (obj == null)
            {
                return;
            }

            var property = obj.GetType().GetProperty(propName, _propertyFlags);

            if (property == null)
            {
                return;
            }

            string value = translation.TranslateItem(category, propName, "Text", ProvideDefaultValue);

            if (!string.IsNullOrEmpty(value) && property.CanWrite)
            {
                property.SetValue(obj, value, null);
            }

            string ProvideDefaultValue() => "";
        }

        public static void TranslateItemsFromFields(string category, object obj, ITranslation translation)
        {
            if (obj == null)
            {
                return;
            }

            TranslateItemsFromList(category, translation, GetObjFields(obj, "$this"));
        }

        private static bool IsTranslatableItemInDataGridViewColumn(PropertyInfo propertyInfo, DataGridViewColumn viewCol)
        {
            return propertyInfo.Name.Equals("HeaderText", StringComparison.CurrentCulture) && viewCol.Visible;
        }

        private static bool IsTranslatableItemInBox(PropertyInfo property, object itemObj)
        {
            if (IsTranslatableItemInComponent(property))
            {
                return true;
            }

            return property.Name.Equals("Items", StringComparison.Ordinal) &&
                   property.GetValue(itemObj, null) is IList items &&
                   items.Count != 0;
        }

        private static readonly HashSet<string> _translatableItemInComponentNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "Caption",
            "Text",
            "ToolTipText",
            "Title"
        };

        private static bool IsTranslatableItemInComponent(PropertyInfo property)
        {
            return property.PropertyType == typeof(string) &&
                   _translatableItemInComponentNames.Contains(property.Name);
        }

        private static readonly string[] UnTranslatableDLLs =
        {
            "mscorlib",
            "Microsoft",
            "Presentation",
            "WindowsBase",
            "ICSharpCode",
            "access",
            "SMDiag",
            "System",
            "vshost",
            "Atlassian",
        };

        /// <summary>true if the specified <see cref="Assembly"/> may be translatable.</summary>
        private static bool IsTranslatable(this Assembly assembly)
        {
            bool isInvalid = UnTranslatableDLLs.Any(
                asm => assembly.FullName.StartsWith(asm, StringComparison.OrdinalIgnoreCase));

            return !isInvalid;
        }

        public static Dictionary<string, List<Type>> GetTranslatableTypes()
        {
            var dictionary = new Dictionary<string, List<Type>>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsTranslatable())
                {
                    continue;
                }

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && typeof(ITranslate).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        var isPlugin = assembly.CodeBase.IndexOf("Plugins/", StringComparison.OrdinalIgnoreCase) != -1;
                        var val = isPlugin ? ".Plugins" : "";

                        if (!dictionary.TryGetValue(val, out var list))
                        {
                            list = new List<Type>();
                            dictionary.Add(val, list);
                        }

                        list.Add(type);
                    }
                }
            }

            return dictionary;
        }

        public static object CreateInstanceOfClass(Type type)
        {
            const BindingFlags constructorFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            object obj = null;

            var constructors = type.GetConstructors(constructorFlags);

            // try to find parameterless constructor first
            foreach (var constructor in constructors)
            {
                if (constructor.GetParameters().Length == 0)
                {
                    obj = Activator.CreateInstance(type, true);
                }
            }

            if (obj == null && constructors.Length != 0)
            {
                var parameterConstructor = constructors[0];
                var parameters = parameterConstructor.GetParameters();
                obj = parameterConstructor.Invoke(new object[parameters.Length]);
            }

            Debug.Assert(obj != null, "obj != null");
            return obj;
        }
    }
}
