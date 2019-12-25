
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyCloud.BL
{
    public class StorageService
    {
        private readonly string _storageDirectory;

        public StorageService(string storageDirectory)
        {
            _storageDirectory = storageDirectory;
        }

        public bool IsValidAbsoluteFilePath(string absoluteFilePath)
        {
            return absoluteFilePath.StartsWith(_storageDirectory);
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            var absoluteFilePath = Path.Combine(_storageDirectory, filePath);
            
            if (!IsValidAbsoluteFilePath(absoluteFilePath))
            {
                throw new InvalidOperationException("Нельзя получить файл вне хранилища");
            }

            return await File.ReadAllBytesAsync(absoluteFilePath);
        }

        public async Task SaveFileToAsync(Stream stream, string filePath, bool overwrite = false)
        {
            var absoluteFilePath = Path.Combine(_storageDirectory, filePath);

            if (!IsValidAbsoluteFilePath(absoluteFilePath))
            {
                throw new InvalidOperationException("Нельзя положить файл вне хранилища");
            }

            if (!overwrite && File.Exists(absoluteFilePath))
            {
                throw new InvalidOperationException("Файл уже существует");
            }

            stream.Seek(0, SeekOrigin.Begin);

            using var fileStream = File.Create(absoluteFilePath);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
        }
    }
}