using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Commons.Serializers
{
    public class Serializer_Json<T_Obj> : Interface_Serializer<T_Obj>
    {
        public void SerializeToFile(T_Obj Source, string TargetPath)
        {
            FileInfo Fi = new FileInfo(TargetPath);
            if (!Fi.Directory.Exists)
            { Fi.Directory.Create(); }

            using (StreamWriter Sw = new StreamWriter(TargetPath))
            {
                using (JsonTextWriter Jtw = new JsonTextWriter(Sw))
                {
                    JsonSerializer Js = new JsonSerializer();
                    Js.Serialize(Jtw, Source, typeof(T_Obj));
                }
            }
        }

        public string SerializeToString(T_Obj Source)
        {
            String SerializedData = JsonConvert.SerializeObject(Source);
            return SerializedData;
        }

        public T_Obj DeserializeFromFile(string SourcePath)
        {
            if (!File.Exists(SourcePath))
            { return default(T_Obj); }

            using (StreamReader Sr = new StreamReader(SourcePath))
            {
                using (JsonTextReader Jtr = new JsonTextReader(Sr))
                {
                    JsonSerializer Js = new JsonSerializer();
                    T_Obj Deserialized = (T_Obj)Js.Deserialize(Jtr, typeof(T_Obj));
                    return Deserialized;
                }
            }
        }

        public T_Obj DeserializeFromString(string Source)
        {
            T_Obj DeserializedObject = (T_Obj)JsonConvert.DeserializeObject(Source, typeof(T_Obj));
            return DeserializedObject;
        }
    }
}
