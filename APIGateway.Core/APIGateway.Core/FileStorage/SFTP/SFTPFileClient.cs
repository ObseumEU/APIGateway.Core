using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Renci.SshNet;

namespace APIGateway.Core.FileStorage.SFTP
{
    public class SFTPFileClient : IStorageClient
    {
        private readonly ConnectionInfo connectionInfo;
        private readonly SFTPCredentials credentials;

        public SFTPFileClient(SFTPCredentials credentials)
        {
            this.credentials = credentials;
            connectionInfo = new ConnectionInfo(credentials.Endpoint, credentials.Port,
                credentials.Username,
                new PasswordAuthenticationMethod(credentials.Username, credentials.Password));
        }

        public async Task<List<FileInfo>> AllFiles(string folder = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FileExist(string filePath)
        {
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                return Task.FromResult(client.Exists(filePath));
            }
        }

        public async Task<string> GetFileContentAsync(string filePath)
        {
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                var mem = new MemoryStream();
                var sw = new StreamReader(mem);
                client.DownloadFile(filePath, sw.BaseStream);
                return await sw.ReadToEndAsync();
            }
        }

        public string Name => credentials?.Name;

        public void UploadFile(Stream stream, string sourceFile, string targetPath)
        {
            //TODO test on production sftp
            //using (var client = new SftpClient(connectionInfo))
            //{
            //    client.Connect();
            //    using (var uplfileStream = File.Open(sourceFile, FileMode.Open, FileAccess.Write, FileShare.Read))
            //    {
            //        client.UploadFile(uplfileStream, targetPath, true);
            //    }

            //    client.Disconnect();
            //}
        }
    }
}