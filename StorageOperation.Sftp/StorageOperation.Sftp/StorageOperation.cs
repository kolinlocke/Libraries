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
		String mKeyPath;
		String mTempPath;
        List<String> mOpenedFiles = new List<String>();

        public StorageOperation() { }

        ~StorageOperation()
        {
            this.Dispose();
        }

		public void Setup_ConnectionData(string ConnectionData)
		{
			this.mConnectionData = ConnectionData;
			Dictionary<String, String> Data =
				this.mConnectionData
				.Split(',')
				.Select(O => O.Split('='))
				.ToDictionary(O => O[0], O => O[1]);

			this.mHost = this.Get_DictionaryValue(Data, "Host"); //Data["Host"];
			this.mUserID = this.Get_DictionaryValue(Data, "UserID"); //Data["UserID"];
			this.mPassword = this.Get_DictionaryValue(Data, "Password"); //Data["Password"];
			this.mKeyPath = this.Get_DictionaryValue(Data, "Key"); //Data["Key"];
		}

		public void Setup_TempPath(string TempPath)
		{
			this.mTempPath = TempPath;
		}

		public GetFileResult Get_File(string FilePath)
		{
			GetFileResult Result = new GetFileResult();

			using (SftpClient Client = this.Create_Client())
			{

				try
				{
					Client.Connect();

					var SftpFile = Client.Get(FilePath);

					String DownloadPath = Path.Combine(this.mTempPath, Path.GetFileName(FilePath));

					using (FileStream Fs = File.OpenWrite(DownloadPath))
					{ Client.DownloadFile(FilePath, Fs); }

					this.mOpenedFiles.Add(DownloadPath);
					Result.File = new FileInfo(DownloadPath);

					Result.File.LastWriteTime = SftpFile.LastWriteTime;
					Result.File.LastWriteTimeUtc = SftpFile.LastWriteTimeUtc;
					Result.File.LastAccessTimeUtc = SftpFile.LastAccessTimeUtc;
					Result.File.LastAccessTime = SftpFile.LastAccessTime;

					Result.Result = true;
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
			//using (SftpClient Client = new SftpClient(this.mHost, this.mUserID, this.mPassword))
			using (SftpClient Client = this.Create_Client())
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
							{ Is_Directory = true; }

							return
								new FileData(this.Get_File)
								{
									Is_Directory = Is_Directory,
									FileName = FileName,
									FullName = O_Sf.FullName
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
			using (SftpClient Client = this.Create_Client())
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
			using (SftpClient Client = this.Create_Client())
			{
				try
				{
					Client.Connect();
					Client.DeleteFile(TargetPath);
				}
				catch (Exception)
				{ throw; }
				finally
				{ Client.Disconnect(); }
			}
		}

		SftpClient Create_Client()
		{
			SftpClient Client = null;

			String Host = this.mHost;
			String UserID = this.mUserID;
			String Password = this.mPassword;
			String KeyPath = this.mKeyPath;

			if (!String.IsNullOrEmpty(Password))
			{ Client = new SftpClient(Host, UserID, Password); }
			else if (!String.IsNullOrEmpty(KeyPath))
			{
				PrivateKeyFile KeyFile = new PrivateKeyFile(KeyPath);
				Client = new SftpClient(Host, UserID, KeyFile);
			}
			else
			{ throw new Exception("Connection Data not set."); }

			return Client;
		}

		String Get_DictionaryValue(Dictionary<String,String> Data, String Key)
		{
			String Value;
			Data.TryGetValue(Key, out Value);
			return Value;
		}

		public void Dispose()
		{
			this.mOpenedFiles.ForEach(O_File =>
			{
				try { File.Delete(O_File); }
				catch { }
			});
		}
	}
}
