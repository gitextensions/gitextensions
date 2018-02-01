using System.IO;
using System.Reflection;

namespace CommonTestUtils
{
    public static class EmbeddedResourceLoader
    {
        public static string Load(Assembly asm, string fullResourceName)
        {
            using (var stream = asm.GetManifestResourceStream(fullResourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    return result;
                }
            }
        }
    }
}