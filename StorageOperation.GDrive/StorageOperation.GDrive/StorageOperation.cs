using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using StorageOperation.Entities;
using StorageOperation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDrive_File = Google.Apis.Drive.v3.Data.File;

namespace StorageOperation.GDrive
{
    public class StorageOperation : Interface_StorageOperation
    {
        String mConnectionData = "";
        String mCredentialsFile = "";
        String mTempPath = "";

        public void Setup_ConnectionData(string ConnectionData)
        {
            this.mConnectionData = ConnectionData;
            this.mCredentialsFile = ConnectionData;
        }

        public void Setup_TempPath(string TempPath)
        {
            this.mTempPath = TempPath;
        }

        public GetFileResult Get_File(string FilePath)
        {
            GetFileResult Result = new GetFileResult();

            try
            {
                var credential = GoogleCredential.FromFile(this.mCredentialsFile).CreateScoped(DriveService.ScopeConstants.Drive);
                var service = new DriveService(new BaseClientService.Initializer() { HttpClientInitializer = credential });

                String DownloadPath = Path.Combine(this.mTempPath, Path.GetFileName(FilePath));

                if (!Directory.Exists(this.mTempPath))
                { Directory.CreateDirectory(this.mTempPath); }

                //[To Do]
                //  Check for Parent Directory(ies) in FilePath
                //  and include them in the Parents Info

                var ListReq = service.Files.List();
                ListReq.Q = $"name = '{FilePath}'";
                var FileList = ListReq.Execute();
                var FoundFile = FileList.Files.FirstOrDefault(O => O.Name == FilePath);
                if (FoundFile == null)
                { throw new Exception("File not found"); }

                var FileReq = service.Files.Get(FoundFile.Id);
                FileStream Fs_File = new FileStream(DownloadPath, FileMode.Create);
                FileReq.Download(Fs_File);
                Fs_File.Flush();
                Fs_File.Close();

                Result.File = new FileInfo(DownloadPath);
            }
            catch (Exception)
            { throw; }

            return Result;
        }

        public List<FileData> Get_Files(string DirectoryPath, bool Is_Recursive = false)
        {
            throw new NotImplementedException();
        }

        public void Write_File(string SourcePath, string TargetPath)
        {
            var credential = GoogleCredential.FromFile(this.mCredentialsFile).CreateScoped(DriveService.ScopeConstants.Drive);
            var service = new DriveService(new BaseClientService.Initializer() { HttpClientInitializer = credential });

            FileInfo Trg_File = new FileInfo(TargetPath);
            GDrive_File? Parent_Dir = null;

            //Get TargetPath Parent Dir
            if (Trg_File.Directory != null)
            {
                var Target_Dir = Trg_File.Directory.Name;

                var ListReq = service.Files.List();
                ListReq.Q = @$"name = '{Target_Dir}' and mimeType = 'application/vnd.google-apps.folder'";
                var DirList = ListReq.Execute();
                if (DirList.Files.Any())
                { Parent_Dir = DirList.Files.First(); }
                else
                {
                    //Create Parent_Dir on GDrive
                    GDrive_File New_Dir_Body = new GDrive_File();
                    New_Dir_Body.MimeType = "application/vnd.google-apps.folder";
                    var Dir_CreateReq = service.Files.Create(New_Dir_Body);
                    Parent_Dir = Dir_CreateReq.Execute();
                }
            }

            //Upload the Source File to GDrive
            List<String> Parents = new List<String>();
            if (Parent_Dir != null)
            { Parents = new List<string> { Parent_Dir.Id }; }

            var CreateReq =
                service.Files.Create(
                    new Google.Apis.Drive.v3.Data.File() { Name = Trg_File.Name, Parents = Parents }
                    , new FileStream(SourcePath, FileMode.Open), "Text/Plain");

            CreateReq.Upload();
        }

        public void Delete_File(string TargetPath)
        {
            var credential = GoogleCredential.FromFile(this.mCredentialsFile).CreateScoped(DriveService.ScopeConstants.Drive);
            var service = new DriveService(new BaseClientService.Initializer() { HttpClientInitializer = credential });

            var Trg_FileName = Path.GetFileName(TargetPath);

            var ListReq = service.Files.List();
            ListReq.Q = $"name = '{Trg_FileName}'";
            var FileList = ListReq.Execute();
            FileList.Files.ToList().ForEach(O_File => {
                var DeleteReq = service.Files.Delete(O_File.Id);
                DeleteReq.Execute();
            });
        }

        public void Dispose()
        {
            
        }
    }
}
