using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace DotGrid.TestHelpers.Json
{
    public class JsonTestDocumentCollection : IEnumerable<JsonTestDocument>
    {
        private readonly IReadOnlyDictionary<string, JsonTestDocument> documents;

        public JsonTestDocumentCollection(IReadOnlyDictionary<string, JsonTestDocument> documents)
        {
            this.documents = documents;
        }

        public IEnumerable<string> Ids => documents.Keys;

        public JsonTestDocument this[string id] => documents[id];

        public IEnumerator<JsonTestDocument> GetEnumerator()
        {
            return documents.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public static JsonTestDocumentCollection FromDirectory(string path,Func<JsonReader,int,byte[]> binaryConverter)
        {
            var jsonFiles = Directory.EnumerateFiles(path, "*.json", SearchOption.TopDirectoryOnly);
            
            return new JsonTestDocumentCollection(jsonFiles.Select(f => JsonTestDocument.FromFile(f,binaryConverter)).ToDictionary(k => k.Id,k => k));
        }
    }
}