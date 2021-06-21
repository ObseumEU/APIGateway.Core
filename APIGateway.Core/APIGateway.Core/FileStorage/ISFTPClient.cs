using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace APIGateway.Core.FileStorage
{
    public interface IStorageClient
    {
        string Name { get; }
        Task<bool> FileExist(string name);
        void UploadFile(Stream stream, string sourceFile, string targetPath);
        Task<List<FileInfo>> AllFiles(string folder = null);
        Task<string> GetFileContentAsync(string filePath);
    }
}