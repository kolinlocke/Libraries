using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Commons.Serializers
{
    public class Serializer
    {
        public enum SerializerType
        {
            Json = 1,
            Xml = 2,
            Binary = 3
        }

        public static void SerializeToFile<T_Obj>(SerializerType SerializerType, T_Obj Source, String TargetPath)
        {
            var Serializer = Create_Instance<T_Obj>(SerializerType);
            Serializer.SerializeToFile(Source, TargetPath);
        }

        public static String SerializeToString<T_Obj>(SerializerType SerializerType, T_Obj Source)
        {
            var Serializer = Create_Instance<T_Obj>(SerializerType);
            var Serialized = Serializer.SerializeToString(Source);
            return Serialized;
        }

        public static String SerializeListToString<T_Obj>(SerializerType SerializerType, List<T_Obj> Source)
        {
            var Serializer = Create_Instance<List<T_Obj>>(SerializerType);
            var Serialized = Serializer.SerializeToString(Source);
            return Serialized;
        }

        public static String SerializeObjectToString(Type T_Obj, SerializerType SerializerType, Object Source)
        {
            var Method_SerializeToString =
                typeof(Serializer)
                .GetMethod("SerializeToString")
                .MakeGenericMethod(T_Obj);

            var Result =  (String)Method_SerializeToString.Invoke(null, new Object[] { SerializerType, Source });
            
            return Result;
        }

        public static String SerializeObjectListToString(Type T_Obj, SerializerType SerializerType, Object Source)
        {
            var Method_SerializeListToString =
               typeof(Serializer)
               .GetMethod("SerializeListToString")
               .MakeGenericMethod(T_Obj);

            var Result = (String)Method_SerializeListToString.Invoke(null, new Object[] { SerializerType, Source });

            return Result;
        }

        public static T_Obj DeserializeFromFile<T_Obj>(SerializerType SerializerType, String SourcePath)
        {
            var Serializer = Create_Instance<T_Obj>(SerializerType);
            return Serializer.DeserializeFromFile(SourcePath);
        }

        public static T_Obj DeserializeFromString<T_Obj>(SerializerType SerializerType, String Source)
        {
            var Serializer = Create_Instance<T_Obj>(SerializerType);
            return Serializer.DeserializeFromString(Source);
        }

        public static List<T_Obj> DeserializeListFromString<T_Obj>(SerializerType SerializerType, String Source)
        {
            var Serializer = Create_Instance<List<T_Obj>>(SerializerType);
            return Serializer.DeserializeFromString(Source);
        }

        public static Object DeserializeObjectFromString(Type T_Obj, SerializerType SerializerType, String Source)
        {
            var Method_DeserializeToString =
                typeof(Serializer)
                .GetMethod("DeserializeFromString")
                .MakeGenericMethod(T_Obj);

            var Result = Method_DeserializeToString.Invoke(null, new Object[] { SerializerType, Source });

            return Result;
        }

        public static Object DeserializeObjectListFromString(Type T_Obj, SerializerType SerializerType, String Source)
        {
            var Method_DeserializeListToString =
                typeof(Serializer)
                .GetMethod("DeserializeListFromString")
                .MakeGenericMethod(T_Obj);

            var Result = Method_DeserializeListToString.Invoke(null, new Object[] { SerializerType, Source });

            return Result;
        }

        static Interface_Serializer<T_Obj> Create_Instance<T_Obj>(SerializerType SerializerType)
        {
            Interface_Serializer<T_Obj> Serializer = null;
            switch (SerializerType)
            {
                case SerializerType.Json:
                    Serializer = new Serializer_Json<T_Obj>();
                    break;
                case SerializerType.Xml:
                    Serializer = new Serializer_Xml<T_Obj>();
                    break;
                case SerializerType.Binary:
                    Serializer = new Serializer_Binary<T_Obj>();
                    break;
            }

            return Serializer;
        }
    }
}
