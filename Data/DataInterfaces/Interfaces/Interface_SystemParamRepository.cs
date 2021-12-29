using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterfaces.Interfaces
{
    public interface Interface_SystemParamRepository
    {
        String GetValue(String ParamType, String ParamName);
        List<String> GetValues(String ParamType, String ParamName);
        Int32 UpdateValue(String ParamType, String ParamName, String ParamValue);
        DateTime GetSystemDateTime();
    }
}
