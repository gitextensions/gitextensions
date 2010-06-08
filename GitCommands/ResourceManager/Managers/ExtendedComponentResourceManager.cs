using System;
using System.IO;
using System.Resources;
using System.Reflection;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;

namespace ResourceManager
{
    /// <summary>
    /// This class simulates the ComponentResourceManager and includes all of its base constructors
    /// </summary>
    public class ExtendedComponentResourceManager : System.Resources.ResourceManager
    {
        #region Fields
        private Hashtable resourceSets;
        private CultureInfo neutralResourcesCulture;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the neutral resources culture.
        /// </summary>
        /// <value>The neutral resources culture.</value>
        private CultureInfo NeutralResourcesCulture
        {
            get
            {
                if ((this.neutralResourcesCulture == null) && (base.MainAssembly != null))
                {
                    this.neutralResourcesCulture = System.Resources.ResourceManager.GetNeutralResourcesLanguage(base.MainAssembly);
                }
                return this.neutralResourcesCulture;
            }
        }
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedComponentResourceManager"/> class.
        /// </summary>
        protected ExtendedComponentResourceManager()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedComponentResourceManager"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ExtendedComponentResourceManager(Type type)
            : base(type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedComponentResourceManager"/> class.
        /// </summary>
        /// <param name="baseName">the base name for the set of resources.</param>
        /// <param name="assembly">The assembly that hosts the resources.</param>
        public ExtendedComponentResourceManager(string baseName, Assembly assembly)
            : base(baseName, assembly)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedComponentResourceManager"/> class.
        /// </summary>
        /// <param name="baseName">the base name for the set of resources.</param>
        /// <param name="assembly">The assembly that hosts the resources.</param>
        /// <param name="usingResourceSet">The resource set type to use.</param>
        public ExtendedComponentResourceManager(string baseName, Assembly assembly, Type usingResourceSet)
            : base(baseName, assembly, usingResourceSet)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// GetString looks up a named resource by "name" using the current culture
        /// and returns the result as a <see cref="System.String"> string</see>.
        /// </summary>
        /// <param name="name">The name of the resource to get.</param>
        /// <returns>resource string</returns>
        /// <exception cref="System.ArgumentNullException">The resource name is a null reference</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The resource value is a null reference</exception>
        /// <exception cref="System.InvalidOperationException">The resource value is not a string</exception>
        public override string GetString(string name)
        {
            return this.GetString(name, null);
        }

        /// <summary>
        /// Overloaded GetString to look up a named resource by "name" and <see cref="CultureInfo"/>
        /// and returns the result as a <see cref="System.String"> string</see>.
        /// </summary>
        /// <param name="name">The name of the resource to get.</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"></see> object
        /// that represents the culture for which the resource is localized. Note that if the resource is not
        /// localized for this culture, the lookup will fall back using the culture's
        /// <see cref="P:System.Globalization.CultureInfo.Parent"></see> property, stopping after looking in 
        /// the neutral culture.If this value is null, the <see cref="T:System.Globalization.CultureInfo"></see>
        /// is obtained using the culture's <see cref="P:System.Globalization.CultureInfo.CurrentUICulture"></see>
        /// property.</param>
        /// <returns>resource string</returns>
        /// <exception cref="System.ArgumentNullException">The resource name is a null reference</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The resource value is a null reference</exception>
        /// <exception cref="System.InvalidOperationException">The resource value is not a string</exception>
        public override string GetString(string name, CultureInfo culture)
        {
            string resource;
            resource = base.GetString(name, culture);

            //if (resource == null)
            //    throw new ArgumentOutOfRangeException(name);

            return resource;
        }

        /// <summary>
        /// GetObject looks up a named resource by "name" using the current culture
        /// and returns the result as an <see cref="System.Object"> object</see>.
        /// </summary>
        /// <param name="name">The name of the resource to get.</param>
        /// <returns>resource object</returns>
        /// <remarks>It is up to the calling program to determine the underlying resource type</remarks>
        /// <exception cref="System.ArgumentNullException">The resource name is a null reference</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The resource value is a null reference</exception>
        public override object GetObject(string name)
        {
            return this.GetObject(name, null);
        }

        /// <summary>
        /// GetObject looks up a named resource by "name" and <see cref="CultureInfo"/>
        /// and returns the result as an <see cref="System.Object"> object</see>.
        /// </summary>
        /// <param name="name">The name of the resource to get.</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"></see> object that represents
        /// the culture for which the resource is localized. Note that if the resource is not localized for this
        /// culture, the lookup will fall back using the culture's <see cref="P:System.Globalization.CultureInfo.Parent"></see>
        /// property, stopping after checking in the neutral culture.If this value is null, the
        /// <see cref="T:System.Globalization.CultureInfo"></see> is obtained using the culture's
        /// <see cref="P:System.Globalization.CultureInfo.CurrentUICulture"></see> property.</param>
        /// <returns>resource object</returns>
        /// <remarks>It is up to the calling program to determine the underlying resource type</remarks>
        /// <exception cref="System.ArgumentNullException">The resource name is a null reference</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The resource value is a null reference</exception>
        public override object GetObject(string name, CultureInfo culture)
        {
            object resource;
            resource = base.GetObject(name, culture);

            //if (resource == null)
            //   throw new ArgumentOutOfRangeException(name);

            return resource;
        }

        /// <summary>
        /// GetStream looks up a named resource by "name" using the current culture
        /// and returns the result as an <see cref="System.IO.UnmanagedMemoryStream"> object</see>.
        /// </summary>
        /// <param name="name">The name of a resource.</param>
        /// <returns>
        /// 	<see cref="System.IO.UnmanagedMemoryStream"/> object
        /// </returns>
        /// <remarks>
        /// It is up to the calling program to determine the underlying resource type.
        /// The GetStream method is good for resources like audio, documents and unsupported graphical formats
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">The resource name is a null reference</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The resource value is a null reference</exception>
        public new UnmanagedMemoryStream GetStream(string name)
        {
            return this.GetStream(name, null);
        }

        /// <summary>
        /// GetStream looks up a named resource by "name" and <see cref="CultureInfo"/>
        /// and returns the result as an <see cref="System.IO.UnmanagedMemoryStream"> object</see>.
        /// </summary>
        /// <param name="name">The name of a resource.</param>
        /// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"></see> object that specifies
        /// the culture to use for the resource lookup. If culture is null, the culture for the current thread
        /// is used.</param>
        /// <returns>
        /// 	<see cref="System.IO.UnmanagedMemoryStream"/> object
        /// </returns>
        /// <remarks>
        /// It is up to the calling program to determine the underlying resource type.
        /// The GetStream method is good for resources like audio, documents and unsupported graphical formats
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">The resource name is a null reference</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The resource value is a null reference</exception>
        public new UnmanagedMemoryStream GetStream(string name, CultureInfo culture)
        {
            UnmanagedMemoryStream resource;
            resource = base.GetStream(name, culture);

            //if (resource == null)
            //    throw new ArgumentOutOfRangeException(name);
            return resource;
        }

        /// <summary>
        /// Applies resources to the properties of an object.
        /// </summary>
        /// <param name="value">The object itself.</param>
        /// <param name="objectName">the name of the object.</param>
        /// <remarks>this overload will apply resources from the neutral or invariant culture</remarks>
        public void ApplyResources(object value, string objectName)
        {
            this.ApplyResources(value, objectName, null);
        }

        /// <summary>
        /// Applies resources to the properties of an object.
        /// </summary>
        /// <param name="value">The object itself.</param>
        /// <param name="objectName">the name of the object.</param>
        /// <param name="culture">The culture.</param>
        public virtual void ApplyResources(object value, string objectName, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (objectName == null)
                throw new ArgumentNullException("objectName");

            if (culture == null)
                culture = CultureInfo.CurrentUICulture;

            SortedList<string, object> resourceList;
            // fill a resource set with a list of resources for the specified culture
            if (this.resourceSets == null)
                resourceList = FillList(culture);
            else
            {
                resourceList = (SortedList<string, object>)this.resourceSets[culture];
                if ((resourceList == null) || (resourceList.Comparer.Equals(StringComparer.OrdinalIgnoreCase) != this.IgnoreCase))
                    resourceList = FillList(culture);
            }

            // find out if the value is a component and is currently in design mode
            bool isComponentInDesignMode = CheckComponent(value);

            // for each resource find the corresponding property and set its value
            // A resource key will be made up of an object name and a property name
            // in the form objectName.propertyName
            foreach (KeyValuePair<string, object> resource in resourceList)
            {
                if (resource.Key == null)
                    continue;

                // check for the start of the resource key matching the name of the object
                if (!IsObjectNameMatched(resource.Key, objectName, this.IgnoreCase))
                    continue;

                // check to see if the key has a property name
                if ((resource.Key.Length <= objectName.Length) || (resource.Key[objectName.Length] != '.'))
                    continue;

                // get the property name from the suffix of the key
                string propertyName = resource.Key.Substring(objectName.Length + 1);

                if (isComponentInDesignMode)
                    // change the value of a sub component in design mode
                    SetPropertyDescriptor(propertyName, resource.Value, value, this.IgnoreCase);
                else
                    // otherwise, change the value of a property of a component
                    SetProperty(propertyName, resource.Value, value, this.IgnoreCase);
            }
        }

        /// <summary>
        /// Tells the <see cref="T:System.Resources.ResourceManager"></see> to call <see cref="M:System.Resources.ResourceSet.Close"></see> on all <see cref="T:System.Resources.ResourceSet"></see> objects and release all resources.
        /// </summary>
        public override void ReleaseAllResources()
        {
            base.ReleaseAllResources();
            this.resourceSets = null;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Creates a sorted and flattened list of resources in top down hierarchy order so that
        /// resources lower down the hierarchy order supercede higher order resources with the same key.
        /// e.g. culture "en-GB" resource value with key "Fred" will supercede "en" resource value with key Fred
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <param name="resourceSet">The output resource set.</param>
        /// <returns>Sorted list of resources</returns>
        private SortedList<string, object> FillResources(CultureInfo culture, out ResourceSet resourceSet)
        {
            SortedList<string, object> resourceList = null;
            ResourceSet parentResourceSet = null;

            // recurse up through the culture hierarchy until you reach the invariant or neutral culture
            // then initialise the resource list
            if (!culture.Equals(CultureInfo.InvariantCulture) && !culture.Equals(this.NeutralResourcesCulture) &&
                !culture.Parent.Equals(CultureInfo.InvariantCulture) && !culture.Parent.Equals(this.NeutralResourcesCulture))
            {
                // recurse up to the parent culture
                resourceList = this.FillResources(culture.Parent, out parentResourceSet);
            }
            else if (this.IgnoreCase)
            {
                // initialise the sorted list to ignore case
                resourceList = new SortedList<string, object>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                // initialise the sorted list to recognise case
                resourceList = new SortedList<string, object>(StringComparer.Ordinal);
            }

            // go and get the resource set for the specified culture
            resourceSet = this.GetResourceSet(culture, true, true);

            // load the resource set into the sorted list overwriting duplicate keys
            if ((resourceSet != null) && !object.ReferenceEquals(resourceSet, parentResourceSet))
            {
                foreach (DictionaryEntry resource in resourceSet)
                {
                    resourceList[(string)resource.Key] = resource.Value;
                }
            }
            return resourceList;
        }

        /// <summary>
        /// Fills the list.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        private SortedList<string, object> FillList(CultureInfo culture)
        {
            ResourceSet dummySet;
            SortedList<string, object> resourceList = this.FillResources(culture, out dummySet);

            if (this.resourceSets == null)
                this.resourceSets = new Hashtable();
            this.resourceSets[culture] = resourceList;

            return resourceList;
        }

        /// <summary>
        /// Checks the component.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static bool CheckComponent(object value)
        {
            bool isComponentInDesignMode = false;
            IComponent component = value as IComponent;
            if (component != null)
            {
                ISite site = component.Site;
                if ((site != null) && site.DesignMode)
                    isComponentInDesignMode = true;
            }

            return isComponentInDesignMode;
        }

        /// <summary>
        /// Determines whether [is object name matched] [the specified key].
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns>
        /// 	<c>true</c> if [is object name matched] [the specified key]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsObjectNameMatched(string key, string objectName, bool ignoreCase)
        {
            bool objectNameMatched = false;
            if (ignoreCase)
            {
                if (string.Compare(key, 0, objectName, 0, objectName.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    objectNameMatched = true;
            }
            else
            {
                if (string.CompareOrdinal(key, 0, objectName, 0, objectName.Length) == 0)
                    objectNameMatched = true;
            }

            return objectNameMatched;
        }

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="resourceValue">The resource value.</param>
        /// <param name="value">The value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        private static void SetProperty(string propertyName, object resourceValue, object value, bool ignoreCase)
        {
            // set the reflection search binding flags
            BindingFlags bindingFlags = SetBindingFlags(ignoreCase);

            PropertyInfo property = null;
            try
            {
                // search for the component property
                property = value.GetType().GetProperty(propertyName, bindingFlags);
            }
            catch (AmbiguousMatchException)
            {
                // if you get multiple matching properties choose the one closest to the declared type
                Type baseType = value.GetType();
                do
                {
                    property = baseType.GetProperty(propertyName, bindingFlags | BindingFlags.DeclaredOnly);
                    baseType = baseType.BaseType;
                    if ((property != null) || (baseType == null))
                        break;
                }
                while (baseType != typeof(object));
            }

            if (((property != null) && property.CanWrite) && ((resourceValue == null) || property.PropertyType.IsInstanceOfType(resourceValue)))
                property.SetValue(value, resourceValue, null);
        }

        /// <summary>
        /// Sets the property descriptor.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="resourceValue">The resource value.</param>
        /// <param name="value">The value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        private static void SetPropertyDescriptor(string propertyName, object resourceValue, object value, bool ignoreCase)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(value).Find(propertyName, ignoreCase);
            if (((descriptor != null) && !descriptor.IsReadOnly) && ((resourceValue == null) || descriptor.PropertyType.IsInstanceOfType(resourceValue)))
                descriptor.SetValue(value, resourceValue);
        }

        /// <summary>
        /// Sets the binding flags.
        /// </summary>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        private static BindingFlags SetBindingFlags(bool ignoreCase)
        {
            BindingFlags bindingFlags = BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance;
            if (ignoreCase)
                bindingFlags |= BindingFlags.IgnoreCase;

            return bindingFlags;
        }
        #endregion
    }
}
