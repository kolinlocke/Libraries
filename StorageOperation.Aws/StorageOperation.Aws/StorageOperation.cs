using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using StorageOperation.Entities;
using StorageOperation.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageOperation.Aws
{
    public class StorageOperation : Interface_StorageOperation
    {
        String mTempPath;

        String mBucketName;
        String mContainerName;
        String mContentType;
        String mAccessKeyID;
        String mSecretKey;
        RegionEndpoint mRegionEndPoint;
        AmazonS3Client mAws_Client;

        public StorageOperation() { }

        ~StorageOperation() { this.mAws_Client.Dispose(); }

        public void Setup_ConnectionData(string ConnectionData)
        {
            if (String.IsNullOrEmpty(ConnectionData))
            { return; }

            Dictionary<String, String> Data =
                ConnectionData
                .Split(',')
                .Select(O => O.Split(':'))
                .ToDictionary(O => O[0], O => O[1]);

            this.mContainerName = this.GetDictionaryData(Data, "ContainerName"); 
            this.mContentType = this.GetDictionaryData(Data, "ContentType"); 

            this.mBucketName = this.GetDictionaryData(Data, "BucketName"); 
            this.mAccessKeyID = this.GetDictionaryData(Data, "AccessKeyID"); 
            this.mSecretKey = this.GetDictionaryData(Data, "AccessKey"); 

            String AWS_RegionEndpoint = this.GetDictionaryData(Data, "RegionEndPoint"); 
            this.mRegionEndPoint = (RegionEndpoint)typeof(RegionEndpoint).GetField(AWS_RegionEndpoint).GetValue(0);

            var Credentials = new BasicAWSCredentials(this.mAccessKeyID, this.mSecretKey);
            this.mAws_Client = new AmazonS3Client(Credentials, this.mRegionEndPoint);
        }
                
        public void Setup_TempPath(string TempPath)
        {
            this.mTempPath = TempPath;
        }

        public GetFileResult Get_File(string FilePath)
        {
            GetFileResult Result = new GetFileResult();

            FileInfo Downloaded = null;
            String DownloadPath = Path.Combine(this.mTempPath, Path.GetFileName(FilePath));

            try
            {
                Downloaded =
                    DownloadFile(
                        this.mAws_Client
                        , FilePath
                        , DownloadPath
                        , this.mBucketName
                        , this.mContainerName);

                Result.Result = true;
                Result.File = Downloaded;
            }
            catch (Exception Ex)
            {
                Result.Result = false;
                Result.Ex = Ex;
            }

            return Result;
        }

        public List<FileData> Get_Files(string DirectoryPath, bool Is_Recursive = false)
        {
            try
            {
                DirectoryPath = Path.Combine(this.mContainerName, DirectoryPath);
                SearchOption Directory_SearchOption = SearchOption.TopDirectoryOnly;
                if (Is_Recursive)
                { Directory_SearchOption = SearchOption.AllDirectories; }

                List<FileData> Files = new List<FileData>();

                var S3_DirectoryInfo = new S3DirectoryInfo(this.mAws_Client, this.mBucketName, DirectoryPath);
                Files =
                    S3_DirectoryInfo
                        .GetFiles("*.*", Directory_SearchOption)
                        .Select(O_File => new FileData(this.Get_File) { FileName = O_File.FullName })
                        .ToList();

                return Files;
            }
            catch (Exception Ex)
            { throw Ex; }
        }

        public void Write_File(string SourcePath, string TargetPath)
        {
            throw new NotImplementedException();
        }

        public void Delete_File(string TargetPath)
        {
            throw new NotImplementedException();
        }

        String GetDictionaryData(Dictionary<String, String> Dict, String Key)
        { return Dict.ContainsKey(Key) ? Dict[Key] : ""; }

        FileInfo DownloadFile(
         AmazonS3Client Aws_Client
         , String FilePath
         , String DownloadPath
         , String BucketName
         , String ContainerName)
        {
            FileInfo Downloaded;

            try
            {
                //If File to be downloaded already exists, delete local file
                if (File.Exists(DownloadPath))
                { File.Delete(DownloadPath); }

                FilePath = Path.Combine(ContainerName, FilePath);

                var S3_FileInfo = new S3FileInfo(Aws_Client, BucketName, FilePath);

                Downloaded = S3_FileInfo.CopyToLocal(DownloadPath);
            }
            catch (Exception Ex)
            { throw Ex; }

            return Downloaded;
        }

        void UploadFile(
            AmazonS3Client Aws_Client
            , String BucketName
            , String ContainerName
            , String ContentType
            , String FilePath
            , String UploadPath)
        {
            UploadPath = Path.Combine(ContainerName, UploadPath);

            PutObjectRequest Request =
                new PutObjectRequest()
                {
                    BucketName = BucketName,
                    Key = UploadPath,
                    FilePath = FilePath,
                    ContentType = ContentType
                };

            var Response = Aws_Client.PutObject(Request);
            Debugger.Break();
        }

        string UploadFileMultiPartInS3(
            AmazonS3Client Aws_Client
            , FileInfo file
            , string bucketName
            , string containerName
            , string fileName = null)
        {
            string awsFilePath = string.Empty;

            awsFilePath =
                UploadFileMultiPartInS3Async(
                    Aws_Client
                    , file
                    , bucketName
                    , containerName
                    , fileName);

            return awsFilePath;
        }

        String UploadFileMultiPartInS3Async(
            AmazonS3Client s3Client
            , FileInfo file
            , string bucketName
            , string containerName
            , string fileName = null)
        {
            FileStream InputStream = file.Open(FileMode.Open);

            string AWSFilePath = string.Empty;
            string fileFullPath = string.Empty;
            if (!string.IsNullOrEmpty(fileName))
            {
                fileFullPath = fileName;
            }
            else
            {
                fileFullPath = (file != null && !string.IsNullOrEmpty(file.Name)) ? file.Name : string.Empty;
            }
            fileFullPath = !string.IsNullOrEmpty(containerName) ? containerName + "/" + fileFullPath : fileFullPath;
            // Create list to store upload part responses.
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();
            // Setup information required to initiate the multipart upload.
            InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest()
            {
                BucketName = bucketName,
                Key = fileFullPath,
                CannedACL = S3CannedACL.PublicRead
            };
            // Initiate the upload.
            InitiateMultipartUploadResponse initResponse = new InitiateMultipartUploadResponse();
            initResponse = s3Client.InitiateMultipartUploadAsync(initiateRequest).Result;

            // Upload parts.
            long contentLength = InputStream.Length;
            long partSize = 5 * (long)Math.Pow(2, 20); // 5 MB
            InputStream.Position = 0;
            try
            {
                long filePosition = 0;
                for (int i = 1; filePosition < contentLength; i++)
                {
                    UploadPartRequest uploadRequest = new UploadPartRequest
                    {
                        BucketName = bucketName,
                        Key = fileFullPath,
                        UploadId = initResponse.UploadId,
                        PartNumber = i,
                        PartSize = partSize,
                        FilePosition = filePosition,
                        InputStream = InputStream
                    };
                    //// Track upload progress.
                    //uploadRequest.StreamTransferProgress +=
                    //    new EventHandler<StreamTransferProgressArgs>(UploadPartProgressEventCallback);

                    // Upload a part and add the response to our list.
                    uploadResponses.Add(s3Client.UploadPartAsync(uploadRequest).Result);

                    filePosition += partSize;
                }
                // Setup to complete the upload.
                CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = bucketName,
                    Key = fileFullPath,
                    UploadId = initResponse.UploadId
                };

                completeRequest.AddPartETags(uploadResponses);

                // Complete the upload.
                CompleteMultipartUploadResponse completeUploadResponse =
                    s3Client.CompleteMultipartUploadAsync(completeRequest).Result;

                AWSFilePath = (completeUploadResponse != null && !string.IsNullOrEmpty(completeUploadResponse.Location)) ? completeUploadResponse.Location : string.Empty;
            }
            catch (Exception exception)
            {
                // Abort the upload.
                AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                {
                    BucketName = bucketName,
                    Key = fileFullPath,
                    UploadId = initResponse.UploadId
                };
                AbortMultipartUploadResponse abortMultipartUploadResponse = s3Client.AbortMultipartUploadAsync(abortMPURequest).Result;

                throw exception;
            }
            finally
            { InputStream.Close(); }

            return AWSFilePath;
        }

        void DeleteObjectinS3BucketAsync(AmazonS3Client s3Client, string fileName, string bucketName, string containerName)
        {
            try
            {
                fileName = !string.IsNullOrEmpty(fileName) ? fileName : string.Empty;
                fileName = !string.IsNullOrEmpty(containerName) ? containerName + "/" + fileName : fileName;
                DeleteObjectRequest deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName
                };
                //Deleting a file in S3 bucket
                DeleteObjectResponse deleteStatus = s3Client.DeleteObjectAsync(deleteObjectRequest).Result;
            }
            catch (AmazonS3Exception e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
