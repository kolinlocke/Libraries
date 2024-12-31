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
        String mCredentialsData = "";
        String mTempPath = "";

        GoogleCredential? mCred = null;
        DriveService? mDriveSvc = null;

        public void Setup_ConnectionData(string ConnectionData)
        {
            //this.mConnectionData = ConnectionData;
            this.mCredentialsData = ConnectionData;

            //this.mCred = GoogleCredential.FromFile(this.mCredentialsFile).CreateScoped(DriveService.ScopeConstants.Drive);
            this.mCred = GoogleCredential.FromJson(this.mCredentialsData).CreateScoped(DriveService.ScopeConstants.Drive);

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

                String Parent_ID = "";
                String Source_File = "";

                var FilePath_Dirs = FilePath.Split('\\');
                if (FilePath_Dirs.Length != 1)
                {
                    String Parsed_TargetPath = String.Join('\\', FilePath_Dirs.Take(FilePath_Dirs.Length - 1));
                    var Parent_Tuple = this.Get_LastParent(Svc, Parsed_TargetPath, true);
                    Parent_ID = Parent_Tuple.Parent_ID;
                }

                Source_File = Path.GetFileName(FilePath);

                String Query_Parents = "";
                if (!String.IsNullOrEmpty(Parent_ID))
                { Query_Parents = $" and '{Parent_ID}' in parents "; }

                var ListReq = Svc.Files.List();
                ListReq.Q = $"name = '{Source_File}' {Query_Parents}";

                var FileList = ListReq.Execute();
                var FoundFile = FileList.Files.FirstOrDefault(O => O.Name == Source_File && O.MimeType != "application/vnd.google-apps.folder");

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

        GetFileResult Get_FileByFileData(FileData Fd)
        {
            GetFileResult Result = new GetFileResult();

            try
            {
                var Svc = this.mDriveSvc;
                if (Svc == null)
                { throw new Exception("DriveService is not initialized."); }

                String FilePath = Fd.FileName;
                String Parent_ID = Fd.Metadata["Parent_ID"];

                String DownloadPath = Path.Combine(this.mTempPath, Path.GetFileName(FilePath));

                if (!Directory.Exists(this.mTempPath))
                { Directory.CreateDirectory(this.mTempPath); }

                String Query_Parents = "";
                if (!String.IsNullOrEmpty(Parent_ID))
                { Query_Parents = $" and '{Parent_ID}' in parents "; }

                var ListReq = Svc.Files.List();
                ListReq.Q = $"name = '{FilePath}' {Query_Parents}";

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

            if (DirectoryPath.ToLower() == "all")
            {
                this.Get_Files_Recursive(Svc, "all", "", Returned_FileData, Is_Recursive);
            }
            else
            {
                (String Parent_ID, String Parent_Name) Parent_Tuple = (Parent_ID: "", Parent_Name: "");
                if (!String.IsNullOrEmpty(DirectoryPath))
                { Parent_Tuple = this.Get_LastParent(Svc, DirectoryPath, false); }

                this.Get_Files_Recursive(Svc, Parent_Tuple.Parent_ID, Parent_Tuple.Parent_Name, Returned_FileData, Is_Recursive);
            }

            return Returned_FileData;
        }

        void Get_Files_Recursive(DriveService Svc, String Parent_ID, String Parent_Name, List<FileData> List_FileData, Boolean Is_Recursive)
        {
            //If Parent_ID is Null/Empty, designated it as root
            //Known Issue, when in root mode, list does not include shared folders that are not part of the root
            //Keyword "all" added, this will get all folders and files

            String Query_Parents = "";

            if (String.IsNullOrEmpty(Parent_ID))
            {
                Parent_ID = "root";
                Query_Parents = $" '{Parent_ID}' in parents ";
            }
            else if (Parent_ID.ToLower() != "all")
            {
                Query_Parents = $" '{Parent_ID}' in parents ";
            }

            var ListReq = Svc.Files.List();
            ListReq.Q = $"{Query_Parents}";
            ListReq.SupportsAllDrives = true;
            ListReq.IncludeItemsFromAllDrives = true;

            var List = ListReq.Execute();

            List.Files.ToList().ForEach(O_File =>
            {
                var Fd = new FileData(this.Get_FileByFileData) { FileName = O_File.Name, FullName = O_File.Name };
                Fd.Metadata = new Dictionary<string, string>
                {
                    { "MimeType", O_File.MimeType },
                    { "Id", O_File.Id },
                    { "Parents", Parent_Name },
                    { "Parent_ID", Parent_ID  }
                };

                if (O_File.MimeType == "application/vnd.google-apps.folder")
                { Fd.Is_Directory = true; }

                List_FileData.Add(Fd);

                if (Fd.Is_Directory && Is_Recursive)
                { this.Get_Files_Recursive(Svc, O_File.Id, $@"{Parent_Name}\{O_File.Name}", List_FileData, Is_Recursive); }
            });
        }

        public List<FileData> Get_Files_Old(string DirectoryPath, bool Is_Recursive = false)
        {
            List<FileData> Returned_FileData = new List<FileData>();

            var Svc = this.mDriveSvc;
            if (Svc == null)
            { throw new Exception("DriveService is not initialized."); }

            //[To Do]
            //Implement Is_Recursive Option

            //Get Parents 
            //To Do: Change this to find parents recursively

            String Q_FileParents = "";
            if (!String.IsNullOrEmpty(DirectoryPath))
            {
                var Dir_Parents = DirectoryPath.Split('\\');
                String Q_Parents = String.Join(" Or ", Dir_Parents.Select(O => $" name = '{O}' "));
                if (!String.IsNullOrEmpty(Q_Parents))
                { Q_Parents = $" and ({Q_Parents})"; }

                var DirReq = Svc.Files.List();
                DirReq.Q = @$"mimeType = 'application/vnd.google-apps.folder' {Q_Parents}";
                var DirList = DirReq.Execute();
                if (DirList.Files.Any())
                {
                    Q_FileParents = String.Join(" Or ", DirList.Files.Select(O_File => $" '{O_File.Id}' in parents "));
                    Q_FileParents = $" {Q_FileParents} ";
                }
            }

            var ListReq = Svc.Files.List();
            ListReq.Q = Q_FileParents;
            var List = ListReq.Execute();

            List.Files.ToList().ForEach(O_File =>
            {
                var Fd = new FileData(this.Get_File) { FileName = O_File.Name };
                Fd.Metadata = new Dictionary<string, string>
                {
                    { "MimeType", O_File.MimeType },
                    { "Id", O_File.Id },
                    { "Parents", O_File.Parents != null ? String.Join(',', O_File.Parents) : "" }
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

            String Parent_ID = "";
            String Target_File = "";

            if (!String.IsNullOrEmpty(TargetPath))
            {
                var Targets = TargetPath.Split('\\');
                if (Targets.Length != 1)
                {
                    String Parsed_TargetPath = String.Join('\\', Targets.Take(Targets.Length - 1));
                    var Parent_Tuple = this.Get_LastParent(Svc, Parsed_TargetPath, true);
                    Parent_ID = Parent_Tuple.Parent_ID;
                }

                Target_File = Path.GetFileName(TargetPath);
            }
            else
            { Target_File = Path.GetFileName(SourcePath); }

            //Check File if it already exists on GDrive
            //If files exits, delete the file

            String Query_Parents = "";
            if (!String.IsNullOrEmpty(Parent_ID))
            { Query_Parents = $" and '{Parent_ID}' in parents "; }

            var FilesReq = Svc.Files.List();
            FilesReq.Q = @$"name = '{Target_File}' {Query_Parents}";
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
            if (!String.IsNullOrEmpty(Parent_ID))
            { Parents = new List<string> { Parent_ID }; }

            using (FileStream Fs_Source = new FileStream(SourcePath, FileMode.Open))
            {
                var CreateReq =
                    Svc.Files.Create(new GDrive_File()
                    {
                        Name = Target_File,
                        Parents = Parents
                        //, WritersCanShare = true
                    }
                    , Fs_Source
                    , "Text/Plain");

                var Up = CreateReq.Upload();
                if (Up.Exception != null)
                {
                    throw Up.Exception;
                }
                Fs_Source.Close();
            }
        }

        public void Write_File_Old(string SourcePath, string TargetPath)
        {
            var Svc = this.mDriveSvc;
            if (Svc == null)
            { throw new Exception("DriveService is not initialized."); }

            /*
            If TargetPath is empty, file will be written in the root
            Get only the immediate Parent of the File if TargetPath has value
            */

            String Target_Parent = "";
            String Target_File = "";
            GDrive_File? Target_Parent_Dir = null;

            if (!String.IsNullOrEmpty(TargetPath))
            {
                var Targets = TargetPath.Split('\\');
                if (Targets.Length >= 2)
                {
                    //Get 2nd to Last Element to get the immediate Parent
                    Target_Parent = Targets[Targets.Length - 2];
                }

                Target_File = Targets.Last();
            }
            else
            { Target_File = Path.GetFileName(SourcePath); }

            if (!String.IsNullOrEmpty(Target_Parent))
            {
                var DirReq = Svc.Files.List();
                DirReq.Q = @$"name = '{Target_Parent}' and mimeType = 'application/vnd.google-apps.folder'";
                var DirList = DirReq.Execute();
                if (DirList.Files.Any())
                { Target_Parent_Dir = DirList.Files.First(); }
                else
                {
                    //Create Parent_Dir on GDrive

                    GDrive_File New_Dir = new GDrive_File();
                    New_Dir.MimeType = "application/vnd.google-apps.folder";
                    New_Dir.Name = Target_Parent;

                    var Dir_CreateReq = Svc.Files.Create(New_Dir);
                    Target_Parent_Dir = Dir_CreateReq.Execute();
                }
            }

            //Check File if it already exists on GDrive
            //If files exits, delete the file
            var FilesReq = Svc.Files.List();
            FilesReq.Q = @$"name = '{Target_File}'";
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
            if (Target_Parent_Dir != null)
            { Parents = new List<string> { Target_Parent_Dir.Id }; }

            using (FileStream Fs_Source = new FileStream(SourcePath, FileMode.Open))
            {
                var CreateReq =
                    Svc.Files.Create(new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = Target_File,
                        Parents = Parents,
                        WritersCanShare = true
                    }
                    , Fs_Source
                    , "Text/Plain");

                CreateReq.Upload();
                Fs_Source.Close();
            }

            //[-]

            /*
            FileInfo Trg_File = new FileInfo(TargetPath);
            
            //Get TargetPath Parent Dir
            if (Trg_File.Directory != null)
            {
                var Target_Dir = Trg_File.Directory.Name;

                var DirReq = Svc.Files.List();
                DirReq.Q = @$"name = '{Target_Dir}' and mimeType = 'application/vnd.google-apps.folder'";
                var DirList = DirReq.Execute();
                if (DirList.Files.Any())
                { Target_Parent_Dir = DirList.Files.First(); }
                else
                {
                    //Create Parent_Dir on GDrive

                    GDrive_File New_Dir = new GDrive_File();
                    New_Dir.MimeType = "application/vnd.google-apps.folder";
                    New_Dir.Name = Target_Dir;

                    var Dir_CreateReq = Svc.Files.Create(New_Dir);
                    Target_Parent_Dir = Dir_CreateReq.Execute();
                }
            }
            */

            /*
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
            */

            /*
            //Upload the Source File to GDrive
            List<String> Parents = new List<String>();
            if (Target_Parent_Dir != null)
            { Parents = new List<string> { Target_Parent_Dir.Id }; }

            using (FileStream Fs_Source = new FileStream(SourcePath, FileMode.Open))
            {
                var CreateReq =
                    Svc.Files.Create(
                        new Google.Apis.Drive.v3.Data.File() { Name = Trg_File.Name, Parents = Parents }
                        , Fs_Source
                        , "Text/Plain");

                CreateReq.Upload();

                Fs_Source.Close();
            }
            */
        }

        public void Delete_File(string TargetPath)
        {
            var Svc = this.mDriveSvc;
            if (Svc == null)
            { throw new Exception("DriveService is not initialized."); }

            String Parent_ID = "";
            String Target_File = "";

            if (!String.IsNullOrEmpty(TargetPath))
            {
                var Targets = TargetPath.Split('\\');
                if (Targets.Length != 1)
                {
                    String Parsed_TargetPath = String.Join('\\', Targets.Take(Targets.Length - 1));
                    var Parent_Tuple = this.Get_LastParent(Svc, Parsed_TargetPath, true);
                    Parent_ID = Parent_Tuple.Parent_ID;
                }

                Target_File = Path.GetFileName(TargetPath);
            }

            String Query_Parents = "";
            if (!String.IsNullOrEmpty(Parent_ID))
            { Query_Parents = $" and '{Parent_ID}' in parents "; }

            var ListReq = Svc.Files.List();
            ListReq.Q = $"name = '{Target_File}' {Query_Parents}";
            var FileList = ListReq.Execute();
            FileList.Files.ToList().ForEach(O_File =>
            {
                var DeleteReq = Svc.Files.Delete(O_File.Id);
                DeleteReq.Execute();
            });
        }

        public void Dispose()
        {
            var Svc = this.mDriveSvc;
            if (Svc != null)
            {
                Svc.Dispose();
                this.mDriveSvc = null;
            }
        }

        (String Parent_ID, String Parent_Name) Get_LastParent(DriveService Svc, String DirectoryPath, Boolean Is_CreateNewParent)
        {
            var Dir_Parents = DirectoryPath.Split('\\');
            var Result = this.Get_ParentID(Svc, Dir_Parents, "", "", 0, Is_CreateNewParent);
            return Result;
        }

        (String Parent_ID, String Parent_Name) Get_ParentID(DriveService Svc, String[] Dir_Parents, String Parent_ID, String Parent_Name, Int32 Ct_Current, Boolean Is_CreateNewParent)
        {
            //Check if Ct_Current is not out of Bounds of Parents
            if (Dir_Parents.Length > Ct_Current)
            {
                //Get Parent
                var Cur_Parent = Dir_Parents[Ct_Current];

                //If Parent_ID is Not Null/Empty, invclude it in the query
                String Query_Parents = "";
                if (!String.IsNullOrEmpty(Parent_ID))
                { Query_Parents = $" and '{Parent_ID}' in parents "; }

                //Get Parent ID
                var DirReq = Svc.Files.List();
                DirReq.Q = @$"mimeType = 'application/vnd.google-apps.folder' and name = '{Cur_Parent}' {Query_Parents}";
                var DirList = DirReq.Execute();
                if (DirList.Files.Any())
                {
                    var Dir = DirList.Files.First();
                    var Cur_Parent_ID = Dir.Id;
                    var Cur_Parent_Name = Dir.Name;

                    return Get_ParentID(Svc, Dir_Parents, Cur_Parent_ID, Cur_Parent_Name, Ct_Current + 1, Is_CreateNewParent);
                }
                else
                {
                    if (Is_CreateNewParent)
                    {
                        GDrive_File New_Dir = new GDrive_File();
                        New_Dir.MimeType = "application/vnd.google-apps.folder";
                        New_Dir.Name = Cur_Parent;
                        New_Dir.Parents = new String[] { Parent_ID };

                        var Dir_CreateReq = Svc.Files.Create(New_Dir);
                        var New_Parent_Dir = Dir_CreateReq.Execute();

                        return Get_ParentID(Svc, Dir_Parents, New_Parent_Dir.Id, Cur_Parent, Ct_Current + 1, Is_CreateNewParent);
                    }
                    else
                    { throw new Exception($"{String.Join('\\', Dir_Parents)} Path is invalid."); }
                }
            }

            return (Parent_ID: Parent_ID, Parent_Name: Parent_Name);
        }


    }
}
