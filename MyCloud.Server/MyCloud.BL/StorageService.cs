
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
            if (!Path.IsPathRooted(storageDirectory))
            {
                throw new ArgumentException(nameof(storageDirectory), "Неверно указан путь к хранилищу");
            }
            Directory.CreateDirectory(storageDirectory);
            _storageDirectory = storageDirectory;
        }

        public bool IsValidAbsoluteFilePath(string absoluteFilePath)
        {
            return absoluteFilePath.StartsWith(_storageDirectory);
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            var absoluteFilePath = GetAbsoluteFilePath(filePath);
            
            if (!IsValidAbsoluteFilePath(absoluteFilePath))
            {
                throw new InvalidOperationException("Нельзя получить файл вне хранилища");
            }
            if (!File.Exists(absoluteFilePath))
            {
                throw new FileNotFoundException($"Файл $\\{filePath} не найден");
            }

            return await File.ReadAllBytesAsync(absoluteFilePath);
        }

        public async Task SaveFileToAsync(Stream stream, string filePath, bool overwrite = false)
        {
            var absoluteFilePath = GetAbsoluteFilePath(filePath);

            if (!IsValidAbsoluteFilePath(absoluteFilePath))
            {
                throw new InvalidOperationException("Нельзя положить файл вне хранилища");
            }

            if (!overwrite && File.Exists(absoluteFilePath))
            {
                throw new InvalidOperationException("Файл уже существует");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(absoluteFilePath));

            stream.Seek(0, SeekOrigin.Begin);

            using var fileStream = File.Create(absoluteFilePath);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
        }

        public async Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath), "Путь к файлу некорректен");
            }
            var absoluteFilePath = GetAbsoluteFilePath(filePath);

            if (!IsValidAbsoluteFilePath(absoluteFilePath))
            {
                throw new InvalidOperationException("Нельзя удалить файл вне хранилища");
            }
            if (!File.Exists(absoluteFilePath))
            {
                throw new FileNotFoundException($"Файл $\\{filePath} не найден");
            }

            try
            {
                File.Delete(absoluteFilePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось удалить файл: {ex.Message}", ex);
            }
        }

        private string GetAbsoluteFilePath(string relativePath) => Path.GetFullPath(Path.Combine(_storageDirectory, relativePath));
    }
}