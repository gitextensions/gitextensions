using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;

namespace GitUI
{
    public class GitExtensionsResourceSet : ResourceSet
    {
        public GitExtensionsResourceSet(ResourceSet resourceSet)
        {
            innerResourceSet = resourceSet;
        }

        private ResourceSet innerResourceSet;

        public override string GetString(string name)
        {
            return "TEST3";
        }

        public override string GetString(string name, bool ignoreCase)
        {
            return "TEST4";
        }

        public override void Close()
        {
            innerResourceSet.Close();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override object GetObject(string name)
        {
            return innerResourceSet.GetObject(name);
        }

        public override object GetObject(string name, bool ignoreCase)
        {
            return innerResourceSet.GetObject(name, ignoreCase);
        }

        public override System.Collections.IDictionaryEnumerator GetEnumerator()
        {
            return innerResourceSet.GetEnumerator();
        }
    }
}
