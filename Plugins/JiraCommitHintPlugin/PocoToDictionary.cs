using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JiraCommitHintPlugin
{
    public static class PocoToDictionary
    {
        private static readonly MethodInfo AddToDicitonaryMethod = typeof(IDictionary<string, object>).GetMethod("Add");
        private static readonly Dictionary<Type, Func<object, IDictionary<string, object>>> Converters = new Dictionary<Type, Func<object, IDictionary<string, object>>>();
        private static readonly ConstructorInfo DictionaryConstructor = typeof(Dictionary<string, object>).GetConstructors().FirstOrDefault(c => c.IsPublic && !c.GetParameters().Any());

        public static IDictionary<string, object> ToDictionary(object obj)
        {
            var dictionary = new Dictionary<string, object>();
            var inputType = obj.GetType();
            foreach (var property in inputType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy))
            {
                if (!property.CanRead || property.GetIndexParameters().Any())
                {
                    continue;
                }

                object value = property.GetValue(obj);

                dictionary[property.Name] = value;
            }

            return dictionary;
        }
    }
}
