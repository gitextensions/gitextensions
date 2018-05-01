using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ResourceManager.Xliff
{
    public static class TranslationUtl
    {
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

            foreach (FieldInfo fieldInfo in obj.GetType().GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.SetField))
            {
                if (fieldInfo.IsPublic && !fieldInfo.IsInitOnly)
                {
                    // if public AND modifiable (NOT readonly)
                    Trace.WriteLine(string.Format("Skip field {0}.{1} [{2}]", obj.GetType().Name, fieldInfo.Name, fieldInfo.GetValue(obj)), "Translation");
                    continue;
                }

                yield return (fieldInfo.Name, fieldInfo.GetValue(obj));
            }
        }

        public static void AddTranslationItem(string category, object obj, string propName, ITranslation translation)
        {
            var propertyInfo = obj?.GetType().GetProperty(
                propName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.NonPublic | BindingFlags.SetProperty);

            if (propertyInfo?.GetValue(obj, null) is string value && AllowTranslateProperty(value))
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

            Func<PropertyInfo, bool> isTranslatableItem;
            if (item is DataGridViewColumn c)
            {
                isTranslatableItem = propertyInfo => IsTranslatableItemInDataGridViewColumn(propertyInfo, c);
            }
            else if (item is ComboBox || item is ListBox)
            {
                isTranslatableItem = propertyInfo => IsTranslatableItemInBox(propertyInfo, item);
            }
            else
            {
                isTranslatableItem = IsTranslatableItemInComponent;
            }

            foreach (var propertyInfo in item.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static |
                               BindingFlags.NonPublic | BindingFlags.SetProperty)
                .Where(isTranslatableItem))
            {
                yield return propertyInfo;
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
                    string propertyName = property.Name;
                    if (propertyName == "Items" && typeof(IList).IsAssignableFrom(property.PropertyType))
                    {
                        var list = (IList)property.GetValue(itemObj, null);
                        for (int index = 0; index < list.Count; index++)
                        {
                            propertyName = "Item" + index;

                            if (list[index] is string listValue)
                            {
                                string ProvideDefaultValue() => listValue;
                                string value = translation.TranslateItem(category, itemName, propertyName,
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
                        string value = translation.TranslateItem(category, itemName, propertyName, ProvideDefaultValue);

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

            var propertyInfo = obj.GetType().GetProperty(propName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.NonPublic | BindingFlags.SetProperty);
            if (propertyInfo == null)
            {
                return;
            }

            string ProvideDefaultValue() => "";
            string value = translation.TranslateItem(category, propName, "Text", ProvideDefaultValue);
            if (!string.IsNullOrEmpty(value))
            {
                if (propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(obj, value, null);
                }
            }
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

        private static bool IsTranslatableItemInBox(PropertyInfo propertyInfo, object itemObj)
        {
            if (IsTranslatableItemInComponent(propertyInfo))
            {
                return true;
            }

            if (propertyInfo.Name.Equals("Items", StringComparison.CurrentCulture))
            {
                if (propertyInfo.GetValue(itemObj, null) is IList items && items.Count != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTranslatableItemInComponent(PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType != typeof(string))
            {
                return false;
            }

            if (propertyInfo.Name.Equals("Caption", StringComparison.CurrentCulture))
            {
                return true;
            }

            if (propertyInfo.Name.Equals("Text", StringComparison.CurrentCulture))
            {
                return true;
            }

            if (propertyInfo.Name.Equals("ToolTipText", StringComparison.CurrentCulture))
            {
                return true;
            }

            if (propertyInfo.Name.Equals("Title", StringComparison.CurrentCulture))
            {
                return true;
            }

            return false;
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
        };

        /// <summary>true if the specified <see cref="Assembly"/> may be translatable.</summary>
        private static bool IsTranslatable(this Assembly assembly)
        {
            bool isInvalid = UnTranslatableDLLs.Any(
                asm => assembly.FullName.StartsWith(asm, StringComparison.OrdinalIgnoreCase));

            return !isInvalid;
        }

        /// <summary>true if the specified <see cref="Assembly"/> may be translatable.</summary>
        private static bool IsPlugin(this Assembly assembly)
        {
            return assembly.CodeBase.IndexOf("Plugins/", StringComparison.OrdinalIgnoreCase) > 0;
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
                        var val = !assembly.IsPlugin() ? "" : ".Plugins";
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
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            object obj = null;

            // try to find parameter less constructor first
            foreach (ConstructorInfo constructor in type.GetConstructors(flags))
            {
                if (constructor.GetParameters().Length == 0)
                {
                    obj = Activator.CreateInstance(type, true);
                }
            }

            if (obj == null && type.GetConstructors().Length > 0)
            {
                ConstructorInfo parameterConstructor = type.GetConstructors(flags)[0];
                var parameters = new List<object>(parameterConstructor.GetParameters().Length);
                for (int i = 0; i < parameterConstructor.GetParameters().Length; i++)
                {
                    parameters.Add(null);
                }

                obj = parameterConstructor.Invoke(parameters.ToArray());
            }

            Debug.Assert(obj != null, "obj != null");
            return obj;
        }
    }
}
