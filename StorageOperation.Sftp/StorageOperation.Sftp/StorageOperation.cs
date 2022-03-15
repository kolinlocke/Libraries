using Renci.SshNet;
using StorageOperation.Entities;
using StorageOperation.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageOperation.Sftp
{
    public class StorageOperation : Interface_StorageOperation
    {
        String mConnectionData = "";
        String mHost;
        String mUserID;
        String mPassword;
        String mTempPath;
        List<String> mOpenedFiles = new List<String>();

        public StorageOperation() { }

        ~StorageOperation()
        {
            this.mOpenedFiles.ForEach(O_File =>
            {
                try { File.Delete(O_File); }
                catch { }
            });
        }

        public void Setup_ConnectionData(string ConnectionData)
        {
            this.mConnectionData = ConnectionData;
            Dictionary<String, String> Data =
                this.mConnectionData
                .Split(',')
                .Select(O => O.Split(':'))
                .ToDictionary(O => O[0], O => O[1]);

            this.mHost = Data["Host"];
            this.mUserID = Data["UserID"];
            this.mPassword = Data["Password"];
        }

        public void Setup_TempPath(string TempPath)
        {
            this.mTempPath = TempPath;
        }

        public GetFileResult Get_File(string FilePath)
        {
            GetFileResult Result = new GetFileResult();

            using (SftpClient Client = new SftpClient(this.mHost, this.mUserID, this.mPassword))
            {

                try
                {
                    Client.Connect();

                    String DownloadPath = Path.Combine(this.mTempPath, Path.GetFileName(FilePath));

                    using (FileStream Fs = File.OpenWrite(DownloadPath))
                    { Client.DownloadFile(FilePath, Fs); }

                    this.mOpenedFiles.Add(DownloadPath);
                    Result.File = new FileInfo(DownloadPath);
                }
                catch (Exception)
                { throw; }
                finally
                { Client.Disconnect(); }

                return Result;
            }
        }

        public List<FileData> Get_Files(string DirectoryPath, bool Is_Recursive = false)
        {
            using (SftpClient Client = new SftpClient(this.mHost, this.mUserID, this.mPassword))
            {
                try
                {
                    Client.Connect();

                    List<String> ForSkip = new List<String>();
                    ForSkip.Add(".");
                    ForSkip.Add("..");

                    var SftpFiles = Client.ListDirectory(DirectoryPath);

                    List<FileData> Files = new List<FileData>();
                    Files =
                        SftpFiles
                        .Where(O_Sf => !ForSkip.Contains(O_Sf.Name))
                        .Select(O_Sf =>
                            {
                                Boolean Is_Directory = false;
                                String FileName = Path.GetFileName(O_Sf.FullName);

                                if (O_Sf.IsDirectory)
                                {
                                    Is_Directory = true;
                                    FileName = $"[{ FileName }]";
                                }

                                return
                                    new FileData(this.Get_File)
                                    {
                                        Is_Directory = Is_Directory,
                                        FileName = FileName
                                    };
                            })
                        .ToList();

                    return Files;
                }
                catch (Exception)
                { throw; }
            }
        }

        public void Write_File(string SourcePath, string TargetPath)
        {
            using (SftpClient Client = new SftpClient(this.mHost, this.mUserID, this.mPassword))
            {
                try
                {
                    Client.Connect();

                    using (FileStream Fs = File.OpenRead(SourcePath))
                    { Client.UploadFile(Fs, TargetPath); }
                }
                catch (Exception)
                { throw; }
                finally
                { Client.Disconnect(); }
            }
        }

        public void Delete_File(string TargetPath)
        {
            using (SftpClient Client = new SftpClient(this.mHost, this.mUserID, this.mPassword))
            {
                try
                {
                    Client.Connect();
                    Client.DeleteFile(TargetPath);
                }
                catch (Exception Ex)
                { throw Ex; }
                finally
                { Client.Disconnect(); }
            }
        }
    }
}
