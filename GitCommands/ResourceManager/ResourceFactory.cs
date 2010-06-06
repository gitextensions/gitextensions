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

    }
}
