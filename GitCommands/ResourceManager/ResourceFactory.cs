using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager
{
    public static class ResourceFactory
    {
        public static IResourceManager GetResourceManager(Type resourceType)
        {
            return new ResourceManager(resourceType.Name, new XmlResourceManager(resourceType));
        }

        public static IResourceManager GetResourceManager(string resourceName)
        {
            string resourceLocation = @"C:\Development\GitUI\Translations"/*typeof(ResourceFactory).Assembly.Location.Substring(0, typeof(ResourceFactory).Assembly.Location.LastIndexOf("\\")) + "\\resources"*/;
            return new ResourceManager(resourceName, new XmlResourceManager(resourceLocation, resourceName));
        }

    }
}
