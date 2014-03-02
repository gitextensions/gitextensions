using System;
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
                return false;
            return text.Any(Char.IsLetter);
        }

        public static IEnumerable<Tuple<string, object>> GetObjProperties(object obj, string objName)
        {
            if (objName != null)
                yield return new Tuple<string, object>(objName, obj);

            foreach (FieldInfo fieldInfo in obj.GetType().GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.SetField))
            {
                if (fieldInfo.IsPublic && !fieldInfo.IsInitOnly)
                {// if public AND modifiable (NOT readonly)
                    continue;
                }
                yield return new Tuple<string, object>(fieldInfo.Name, fieldInfo.GetValue(obj));
            }
        }

        public static void AddTranslationItemsFromFields(string category, object obj, ITranslation translation)
        {
            if (obj == null)
                return;

            AddTranslationItemsFromList(category, translation, GetObjProperties(obj, "$this"));
        }

        public static void AddTranslationItemsFromList(string category, ITranslation translation, IEnumerable<Tuple<string, object>> items)
        {
            Action<string, object, PropertyInfo> action = delegate(string item, object itemObj, PropertyInfo propertyInfo)
            {
                var value = (string)propertyInfo.GetValue(itemObj, null);
                if (AllowTranslateProperty(value))
                {
                    translation.AddTranslationItem(category, item, propertyInfo.Name, value);
                }
            };
            ForEachItem(items, action);
        }

        public static void ForEachItem(IEnumerable<Tuple<string, object>> items, Action<string, object, PropertyInfo> action)
        {
            foreach (var item in items)
            {
                string itemName = item.Item1;
                object itemObj = item.Item2;

                //Skip controls with a name started with "_NO_TRANSLATE_"
                //this is a naming convention, these are not translated
                if (itemName.StartsWith("_NO_TRANSLATE_"))
                    continue;

                Func<PropertyInfo, bool> isTranslatableItem;
                if (itemObj is DataGridViewColumn)
                {
                    var c = itemObj as DataGridViewColumn;

                    isTranslatableItem = propertyInfo => IsTranslatableItemInDataGridViewColumn(propertyInfo, c);
                }
                else
                {
                    isTranslatableItem = IsTranslatableItemInComponent;
                }

                Action<PropertyInfo> paction = propertyInfo => action(itemName, itemObj, propertyInfo);

                ForEachProperty(itemObj, paction, isTranslatableItem);
            }
        }

        public static void TranslateItemsFromList(string category, ITranslation translation, IEnumerable<Tuple<string, object>> items) 
        {
            Action<string, object, PropertyInfo> action = delegate(string item, object itemObj, PropertyInfo propertyInfo)
            {
                string value = translation.TranslateItem(category, item, propertyInfo.Name, null);
                if (!String.IsNullOrEmpty(value))
                {
                    if (propertyInfo.CanWrite)
                        propertyInfo.SetValue(itemObj, value, null);
                }
                else if (propertyInfo.Name == "ToolTipText" && !String.IsNullOrEmpty((string)propertyInfo.GetValue(itemObj, null)))
                {
                    value = translation.TranslateItem(category, item, "Text", null);
                    if (!String.IsNullOrEmpty(value))
                    {
                        if (propertyInfo.CanWrite)
                            propertyInfo.SetValue(itemObj, value, null);
                    }
                }
            };
            ForEachItem(items, action);
        }

        public static void TranslateItemsFromFields(string category, object obj, ITranslation translation)
        {
            if (obj == null)
                return;

            TranslateItemsFromList(category, translation, GetObjProperties(obj, "$this"));
        }

        public static void ForEachProperty(object obj, Action<PropertyInfo> action, Func<PropertyInfo, bool> IsTranslatableItem)
        {
            if (obj == null)
                return;

            foreach (var propertyInfo in obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static |
                               BindingFlags.NonPublic | BindingFlags.SetProperty)
                .Where(IsTranslatableItem))
            {
                action(propertyInfo);
            }
        }

        public static bool IsTranslatableItemInComponent(PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType != typeof(string))
                return false;
            if (propertyInfo.Name.Equals("Caption", StringComparison.CurrentCulture))
                return true;
            if (propertyInfo.Name.Equals("Text", StringComparison.CurrentCulture))
                return true;
            if (propertyInfo.Name.Equals("ToolTipText", StringComparison.CurrentCulture))
                return true;
            if (propertyInfo.Name.Equals("Title", StringComparison.CurrentCulture))
                return true;
            return false;
        }

        public static bool IsTranslatableItemInDataGridViewColumn(PropertyInfo propertyInfo, DataGridViewColumn viewCol)
        {
            return propertyInfo.Name.Equals("HeaderText", StringComparison.CurrentCulture) && viewCol.Visible;
        }

        static readonly string[] UnTranslatableDLLs =
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
        public static bool IsTranslatable(this Assembly assembly)
        {
            bool isInvalid = UnTranslatableDLLs.Any(
                asm => assembly.FullName.StartsWith(asm, StringComparison.OrdinalIgnoreCase));

            return !isInvalid;
        }

        public static List<Type> GetTranslatableTypes()
        {
            var list = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsTranslatable())
                    continue;
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && typeof(ITranslate).IsAssignableFrom(type) && !type.IsAbstract)
                        list.Add(type);
                }
            }
            return list;
        }

        public static object CreateInstanceOfClass(Type type)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            object obj = null;
            // try to find parameter less constructor first
            foreach (ConstructorInfo constructor in type.GetConstructors(flags))
            {
                if (constructor.GetParameters().Length == 0)
                    obj = Activator.CreateInstance(type, true);
            }
            if (obj == null && type.GetConstructors().Length > 0)
            {
                ConstructorInfo parameterConstructor = type.GetConstructors(flags)[0];
                var parameters = new List<object>(parameterConstructor.GetParameters().Length);
                for (int i = 0; i < parameterConstructor.GetParameters().Length; i++)
                    parameters.Add(null);
                obj = parameterConstructor.Invoke(parameters.ToArray());
            }

            Debug.Assert(obj != null);
            return obj;
        }
    }
}
