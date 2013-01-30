using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

        public static void AddTranslationItemsFromFields(string category, object obj, Translation translation)
        {
            if (obj == null)
                return;

            Action<string, object, PropertyInfo> action = (item, itemObj, propertyInfo) =>
            {
                var value = (string)propertyInfo.GetValue(itemObj, null);
                if (AllowTranslateProperty(value))
                {
                    translation.AddTranslationItem(category, item, propertyInfo.Name, value);
                }
            };
            ForEachField(obj, action);
        }

        public static void TranslateItemsFromFields(string category, object obj, Translation translation)
        {
            if (obj == null)
                return;

            Action<string, object, PropertyInfo> action = (item, itemObj, propertyInfo) =>
            {
                string value = translation.TranslateItem(category, item, propertyInfo.Name, null);
                if (!String.IsNullOrEmpty(value))
                {
                    propertyInfo.SetValue(itemObj, value, null);
                }
                else if (propertyInfo.Name == "ToolTipText" &&
                         !String.IsNullOrEmpty((string)propertyInfo.GetValue(itemObj, null)))
                {
                    value = translation.TranslateItem(category, item, "Text", null);
                    if (!String.IsNullOrEmpty(value))
                    {
                        propertyInfo.SetValue(itemObj, value, null);
                    }
                }
            };
            ForEachField(obj, action);
        }

        public static void ForEachField(object obj, Action<string, object, PropertyInfo> action)
        {
            if (obj == null)
                return;
            foreach (FieldInfo fieldInfo in obj.GetType().GetFields(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                if (fieldInfo.IsPublic && !fieldInfo.IsInitOnly)
                {// if public AND modifiable (not readonly)
                    continue;
                }// ... else: nonPublic OR readonly
                Action<PropertyInfo> paction =
                    propertyInfo => action(fieldInfo.Name, fieldInfo.GetValue(obj), propertyInfo);

                //Skip controls with a name started with "_NO_TRANSLATE_"
                //this is a naming convention, these are not translated
                if (fieldInfo.Name.StartsWith("_NO_TRANSLATE_"))
                    continue;
                if (fieldInfo.FieldType.IsSubclassOf(typeof(Component)))
                {
                    Component c = fieldInfo.GetValue(obj) as Component;
                    ForEachProperty(c, paction, IsTranslatableItemInComponent);
                }
                else if (fieldInfo.FieldType.IsSubclassOf(typeof(DataGridViewColumn)))
                {
                    DataGridViewColumn c = fieldInfo.GetValue(obj) as DataGridViewColumn;

                    Func<PropertyInfo, bool> IsTranslatableItem =
                        propertyInfo => IsTranslatableItemInDataGridViewColumn(propertyInfo, c);

                    ForEachProperty(c, paction, IsTranslatableItem);
                }
            }
        }

        public static void ForEachProperty(object obj, Action<PropertyInfo> action, Func<PropertyInfo, bool> IsTranslatableItem)
        {
            if (obj == null)
                return;

            foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic))
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

        public static bool IsAssemblyTranslatable(Assembly assembly)
        {
            if ((assembly.FullName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("Presentation", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("ICSharpCode", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("access", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("SMDiag", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("vshost", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            return true;
        }

        public static List<Type> GetTranslatableTypes()
        {
            List<Type> translatableTypes = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (IsAssemblyTranslatable(assembly))
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.IsClass && typeof(ITranslate).IsAssignableFrom(type))
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
