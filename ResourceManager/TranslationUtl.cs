using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ResourceManager.Translation
{
    public static class TranslationUtl
    {
        private static bool AllowTranslateProperty(string text)
        {
            if (text == null)
                return false;
            foreach (char c in text)
                if (Char.IsLetter(c))
                    return true;
            return false;
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

        public static void AddTranslationItemsFromFields(string category, object obj, Translation translation)
        {
            if (obj == null)
                return;

            AddTranslationItemsFromList(category, translation, GetObjProperties(obj, "$this"));
        }

        public static void AddTranslationItemsFromList(string category, Translation translation, IEnumerable<Tuple<string, object>> items)
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

                Func<PropertyInfo, bool> IsTranslatableItem;
                if (itemObj is DataGridViewColumn)
                {
                    DataGridViewColumn c = itemObj as DataGridViewColumn;

                    IsTranslatableItem = propertyInfo => IsTranslatableItemInDataGridViewColumn(propertyInfo, c);
                }
                else
                {
                    IsTranslatableItem = IsTranslatableItemInComponent;
                }

                Action<PropertyInfo> paction = propertyInfo => action(itemName, itemObj, propertyInfo);
                ForEachProperty(itemObj, paction, IsTranslatableItem);
            }
        }

        public static void TranslateItemsFromList(string category, Translation translation, IEnumerable<Tuple<string, object>> items)
        {
            Action<string, object, PropertyInfo> action = (item, itemObj, propertyInfo) =>
            {
                string value = translation.TranslateItem(category, item, propertyInfo.Name, null);
				if (!String.IsNullOrEmpty(value))
				{
					if (propertyInfo.CanWrite)
						propertyInfo.SetValue(itemObj, value, null);
				}
				else if (propertyInfo.Name == "ToolTipText" && !String.IsNullOrEmpty((string)propertyInfo.GetValue(itemObj, null))
                         && !String.IsNullOrEmpty((string)propertyInfo.GetValue(itemObj, null)))
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

        public static void TranslateItemsFromFields(string category, object obj, Translation translation)
        {
            if (obj == null)
                return;

            TranslateItemsFromList(category, translation, GetObjProperties(obj, "$this"));
        }

        public static void ForEachProperty(object obj, Action<PropertyInfo> action, Func<PropertyInfo, bool> IsTranslatableItem)
        {
            if (obj == null)
                return;

            foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.SetProperty))
                if (IsTranslatableItem(propertyInfo))
                    action(propertyInfo);
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

        static string[] UnTranslatableDLLs = new string[]
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
            List<Type> translatableTypes = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsTranslatable())
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        //TODO: Check if class contain TranslationString but doesn't implement ITranslate
                        if (type.IsClass && typeof(ITranslate).IsAssignableFrom(type) && !type.IsAbstract)
                        {
                            translatableTypes.Add(type);
                        }
                    }
                }
            }
            return translatableTypes;
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
