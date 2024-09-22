using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Commons.Serializers
{
    public class Serializer_Binary<T_Obj> : Interface_Serializer<T_Obj>
    {
        public void SerializeToFile(T_Obj Source, string Target_FilePath)
        {
            FileInfo Obj_File = new FileInfo(Target_FilePath);
            if (!Obj_File.Directory.Exists)
            { Obj_File.Directory.Create(); }
                        
            FileStream Fs = new FileStream(Target_FilePath, FileMode.Create);            
            JsonSerializer.Serialize(Fs, Source);
            Fs.Flush();
            Fs.Close();
            Fs.Dispose();

            //FileStream Fs = new FileStream(Target_FilePath, FileMode.Create);
            //BinaryFormatter Formatter = new BinaryFormatter();
            //Formatter.Serialize(Fs, Source);
            //Fs.Close();
            //Fs.Dispose();
        }

        public String SerializeToString(T_Obj Source)
        {
            //MemoryStream Stream = new MemoryStream();
            //BinaryFormatter Formatter = new BinaryFormatter();
            //Formatter.Serialize(Stream, Source);

            //var Read = Convert.ToBase64String(Stream.ToArray());

            //Stream.Close();
            //Stream.Dispose();

            //return Read;

            var SerializedBytes = JsonSerializer.SerializeToUtf8Bytes(Source);
            var Read = Convert.ToBase64String(SerializedBytes);
            return Read;
        }

        public T_Obj DeserializeFromFile(string Source_FilePath)
        {
            T_Obj Result = default(T_Obj);

            if (File.Exists(Source_FilePath))
            {
                //FileStream Fs = new FileStream(Source_FilePath, FileMode.Open);
                //BinaryFormatter Formatter = new BinaryFormatter();
                //Result = (T_Obj)Formatter.Deserialize(Fs);
                //Fs.Close();
                //Fs.Dispose();

                var Jti = JsonTypeInfo.CreateJsonTypeInfo<T_Obj>(JsonSerializerOptions.Default);
                FileStream Fs = new FileStream(Source_FilePath, FileMode.Open);
                Result = JsonSerializer.Deserialize(Fs, Jti);

            }
            else
            { throw new Exception("Source_FilePath doesn't exists."); }

            return Result;
        }

        public T_Obj DeserializeFromString(String Source)
        {
            T_Obj Result = default(T_Obj);

            //MemoryStream Stream = new MemoryStream(Convert.FromBase64String(Source));
            //BinaryFormatter Formatter = new BinaryFormatter();
            //Result = (T_Obj)Formatter.Deserialize(Stream);

            //Stream.Close();
            //Stream.Dispose();

            var Jti = JsonTypeInfo.CreateJsonTypeInfo<T_Obj>(JsonSerializerOptions.Default);
            Result = JsonSerializer.Deserialize(Source, Jti);

            return Result;
        }
    }
}
