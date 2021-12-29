using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Commons.Serializers
{
    public interface Interface_Serializer<T_Obj>
    {
        void SerializeToFile(T_Obj Source, String Target_FilePath);
        String SerializeToString(T_Obj Source);
        T_Obj DeserializeFromFile(String Source_FilePath);
        T_Obj DeserializeFromString(String Source);
    }
}
