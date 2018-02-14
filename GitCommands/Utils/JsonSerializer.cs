using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace GitCommands.Utils
{
    public static class JsonSerializer
    {
        public static string Serialize<T>(T myObject)
        {
            var json = new DataContractJsonSerializer(typeof(T));
            var stream = new MemoryStream();
            json.WriteObject(stream, myObject);
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static T Deserialize<T>(string myString)
        {
            var json = new DataContractJsonSerializer(typeof(T));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(myString));
            return (T)json.ReadObject(stream);
        }
    }
}
