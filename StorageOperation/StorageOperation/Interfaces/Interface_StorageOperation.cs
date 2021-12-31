using StorageOperation.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageOperation.Interfaces
{
    public interface Interface_StorageOperation
    {        
        void Setup_ConnectionData(String ConnectionData);
        void Setup_TempPath(String TempPath);
        List<FileData> Get_Files(String DirectoryPath, Boolean Is_Recursive = false);
        GetFileResult Get_File(String FilePath);
        void Write_File(String SourcePath, String TargetPath);
        void Delete_File(String TargetPath);
    }
}
