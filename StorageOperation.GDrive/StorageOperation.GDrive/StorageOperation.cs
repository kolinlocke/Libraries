using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using StorageOperation.Entities;
using StorageOperation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using GDrive_File = Google.Apis.Drive.v3.Data.File;


namespace StorageOperation.GDrive
{
    public class StorageOperation : Interface_StorageOperation
    {
        //String mConnectionData = "";
        String mCredentialsFile = "";
        String mTempPath = "";

        GoogleCredential? mCred = null;
        DriveService? mDriveSvc = null;

        public void Setup_ConnectionData(string ConnectionData)
        {
            //this.mConnectionData = ConnectionData;
            this.mCredentialsFile = ConnectionData;

            this.mCred = GoogleCredential.FromFile(this.mCredentialsFile).CreateScoped(DriveService.ScopeConstants.Drive);
            this.mDriveSvc = new DriveService(new BaseClientService.Initializer() { HttpClientInitializer = this.mCred });
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
                var Svc = this.mDriveSvc;
                if (Svc == null)
                { throw new Exception("DriveService is not initialized."); }

                String DownloadPath = Path.Combine(this.mTempPath, Path.GetFileName(FilePath));

                if (!Directory.Exists(this.mTempPath))
                { Directory.CreateDirectory(this.mTempPath); }

                //[To Do]
                //  Check for Parent Directory(ies) in FilePath
                //  and include them in the Parents Info

                var ListReq = Svc.Files.List();
                ListReq.Q = $"name = '{FilePath}'";
                var FileList = ListReq.Execute();
                var FoundFile = FileList.Files.FirstOrDefault(O => O.Name == FilePath);
                if (FoundFile == null)
                { throw new Exception("File not found"); }

                var FileReq = Svc.Files.Get(FoundFile.Id);
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
            List<FileData> Returned_FileData = new List<FileData>();

            var Svc = this.mDriveSvc;
            if (Svc == null)
            { throw new Exception("DriveService is not initialized."); }

            var ListReq = Svc.Files.List();
            var List = ListReq.Execute();

            List.Files.ToList().ForEach(O_File =>
            {
                var Fd = new FileData(this.Get_File) { FileName = O_File.Name };
                Fd.Metadata = new Dictionary<string, string>
                {
                    { "MimeType", O_File.MimeType },
                    { "Id", O_File.Id },
                    { "Parents", String.Join(',', O_File.Parents) }
                };

                if (O_File.MimeType == "application/vnd.google-apps.folder")
                { Fd.Is_Directory = true; }
                
                Returned_FileData.Add(Fd);
            });

            return Returned_FileData;
        }

        public void Write_File(string SourcePath, string TargetPath)
        {
            var Svc = this.mDriveSvc;
            if (Svc == null)
            { throw new Exception("DriveService is not initialized."); }

            FileInfo Trg_File = new FileInfo(TargetPath);
            GDrive_File? Parent_Dir = null;

            //Get TargetPath Parent Dir
            if (Trg_File.Directory != null)
            {
                var Target_Dir = Trg_File.Directory.Name;

                var DirReq = Svc.Files.List();
                DirReq.Q = @$"name = '{Target_Dir}' and mimeType = 'application/vnd.google-apps.folder'";
                var DirList = DirReq.Execute();
                if (DirList.Files.Any())
                { Parent_Dir = DirList.Files.First(); }
                else
                {
                    //Create Parent_Dir on GDrive

                    GDrive_File New_Dir = new GDrive_File();
                    New_Dir.MimeType = "application/vnd.google-apps.folder";
                    New_Dir.Name = Target_Dir;

                    var Dir_CreateReq = Svc.Files.Create(New_Dir);
                    Parent_Dir = Dir_CreateReq.Execute();
                }
            }

            //Check File if it already exists on GDrive
            //If files exits, delete the file
            var FilesReq = Svc.Files.List();
            FilesReq.Q = @$"name = '{Trg_File.Name}'";
            var FilesList = FilesReq.Execute();
            if (FilesList.Files.Any())
            {
                FilesList.Files.ToList().ForEach(O_File =>
                {
                    var DeleteReq = Svc.Files.Delete(O_File.Id);
                    DeleteReq.Execute();
                });
            }

            //Upload the Source File to GDrive
            List<String> Parents = new List<String>();
            if (Parent_Dir != null)
            { Parents = new List<string> { Parent_Dir.Id }; }

            var CreateReq =
                Svc.Files.Create(
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
            FileList.Files.ToList().ForEach(O_File =>
            {
                var DeleteReq = service.Files.Delete(O_File.Id);
                DeleteReq.Execute();
            });
        }

        public void Dispose()
        {

        }
    }
}
