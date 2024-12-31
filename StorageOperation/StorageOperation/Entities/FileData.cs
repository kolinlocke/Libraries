using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageOperation.Entities
{
    public class FileData
    {
        Func<String, GetFileResult> mFunc_GetFile;

        Func<FileData, GetFileResult> mFunc_GetFileByFileData;

        public FileData(Func<String, GetFileResult> Func)
        {
            this.mFunc_GetFile = Func;
        }

        public FileData(Func<FileData, GetFileResult> Func)
        {
            this.mFunc_GetFileByFileData = Func;
        }

        public Boolean Is_Directory { get; set; }

        public String FileName { get; set; }

        public String FullName { get; set; }

        public Dictionary<String, String> Metadata { get; set; }

        Exception mEx;
        public Exception Ex { get { return this.mEx; } }

        FileInfo mFile;
        public FileInfo File
        {
            get
            {
                if (this.mFile == null)
                {
                    GetFileResult GetFileResult = null;

                    if (this.mFunc_GetFile != null)
                    { GetFileResult = this.mFunc_GetFile(this.FullName); }
                    else if (this.mFunc_GetFileByFileData != null)
                    { GetFileResult = this.mFunc_GetFileByFileData(this); }

                    if (GetFileResult.Result)
                    { this.mFile = GetFileResult.File; }
                    else
                    { this.mEx = GetFileResult.Ex; }

                    return this.mFile;                    
                }
                else
                { return this.mFile; }
            }
        }
    }

    public class GetFileResult
    {
        public FileInfo File { get; set; }
        public Boolean Result { get; set; }
        public Exception Ex { get; set; }
    }
}
