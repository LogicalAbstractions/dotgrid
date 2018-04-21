using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DotGrid.Core.Json
{
    public static class JTokenExtensions
    {
        public static bool IsStructurallyEqual(this JToken a, JToken b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            
            if (a is JValue v1 && b is JValue v2)
            {
                return v1.Equals(v2);
            }

            if (a is JArray a1 && b is JArray a2)
            {
                if (a1.Count == a2.Count)
                {
                    for (int i = 0; i < a1.Count; ++i)
                    {
                        var t1 = a1.ElementAt(i);
                        var t2 = a2.ElementAt(i);

                        if (!IsStructurallyEqual(t1, t2))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            if (a is JObject o1 && b is JObject o2)
            {
                if (o1.Properties().All(p => o2.ContainsKey(p.Name)) &&
                    o2.Properties().All(p => o1.ContainsKey(p.Name)))
                {
                    foreach (var p1 in o1.Properties())
                    {
                        var p2 = o2.Property(p1.Name);

                        if (!IsStructurallyEqual(p1.Value, p2.Value))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }
}