using StorageOperation.Entities;
using StorageOperation.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageOperation.WinDir
{
    public class StorageOperation : Interface_StorageOperation
    {
        public GetFileResult Get_File(string FilePath)
        {
            GetFileResult Result = new GetFileResult();

            Result.File = new FileInfo(FilePath);
            Result.Result = Result.File.Exists;

            return Result;
        }

        public List<FileData> Get_Files(string DirectoryPath, bool Is_Recursive = false)
        {
            var Files = Directory.GetFiles(DirectoryPath, "*", Is_Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            List<FileData> Fds =
                Files.Select(O_File => new FileData(this.Get_File) { FileName = O_File }).ToList();

            return Fds;
        }

        public void Setup_ConnectionData(string ConnectionData) { }

        public void Setup_TempPath(string TempPath) { }

        public void Write_File(string SourcePath, string TargetPath)
        {
            FileInfo Target = new FileInfo(TargetPath);
            if (!Target.Directory.Exists)
            { Target.Directory.Create(); }

            FileInfo Source = new FileInfo(SourcePath);
            if (Target.Exists)
            { Target.Delete(); }
            Source.CopyTo(TargetPath);
        }

        public void Delete_File(string TargetPath)
        {
            File.Delete(TargetPath);
        }
    }
}
