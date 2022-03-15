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

        public FileData(Func<String, GetFileResult> Func_GetFile)
        {
            this.mFunc_GetFile = Func_GetFile;
        }

        public Boolean Is_Directory { get; set; }

        public String FileName { get; set; }

        Exception mEx;
        public Exception Ex { get { return this.mEx; } }

        FileInfo mFile;
        public FileInfo File
        {
            get
            {
                if (this.mFile == null)
                {
                    //this.mFile = this.mFunc_GetFile(this.FileName);
                    var GetFileResult = this.mFunc_GetFile(this.FileName);
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
