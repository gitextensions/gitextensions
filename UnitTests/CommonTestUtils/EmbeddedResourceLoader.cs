using System.IO;
using System.Reflection;

namespace CommonTestUtils
{
    public static class EmbeddedResourceLoader
    {
        public static string Load(Assembly asm, string fullResourceName)
        {
            using var stream = asm.GetManifestResourceStream(fullResourceName);
            using StreamReader reader = new(stream);
            string result = reader.ReadToEnd();
            return result;
        }
    }
}
