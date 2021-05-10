using System.IO;
using System.Text;

namespace GitCommands.Utils
{
    public static class JsonSerializer
    {
        public static string Serialize<T>(T? myObject) where T : class
        {
            var json = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
            MemoryStream stream = new();
            json.WriteObject(stream, myObject);
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static T? Deserialize<T>(string myString) where T : class
        {
            var json = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
            MemoryStream stream = new(Encoding.UTF8.GetBytes(myString));
            return (T?)json.ReadObject(stream);
        }
    }
}
