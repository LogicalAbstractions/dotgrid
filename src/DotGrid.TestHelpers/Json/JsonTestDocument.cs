using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotGrid.TestHelpers.Json
{
    public class JsonTestDocument
    {
        public JsonTestDocument(string id,string jsonText,Func<JsonReader,int,byte[]> binaryConverter)
        {
            Id = id;
            JsonText = jsonText;
            JsonToken = JToken.Parse(jsonText);
            JsonBytes = Encoding.UTF8.GetBytes(JsonToken.ToString(Formatting.None));

            using (var reader = new JTokenReader(JsonToken))
            {
                BinaryBytes = binaryConverter.Invoke(reader, jsonText.Length);
            }
        }
        
        public string Id { get; }
        
        public string JsonText  { get; }
        
        public JToken JsonToken { get; }
        
        public byte[] JsonBytes { get; }
        
        public byte[] BinaryBytes { get; }

        public static JsonTestDocument FromFile(string path,Func<JsonReader,int,byte[]> binaryConverter)
        {
            return new JsonTestDocument(Path.GetFileNameWithoutExtension(path),File.ReadAllText(path),binaryConverter);
        }
    }
}