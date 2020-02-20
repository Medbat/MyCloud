using MyCloud.BL;
using System;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using System.Reflection;
using System.IO;

namespace MyCloud.UnitTests
{


	public class UnitTest1
	{
		private readonly StorageService _storageService;
		private readonly string _storagePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "cloud");
		public UnitTest1()
        {
			_storageService = new StorageService(_storagePath);
		}

		[Fact]
		public async Task CreateStorageWithRelativeTest()
		{
			Should.Throw<ArgumentException>(() => new StorageService("relative/Path"));
		}

		[Fact]
		public async Task CreateStorageWithAbsolutePathTest()
		{
			var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "cloud", "huyaud", "asdasd");
			var storage = new StorageService(path);
			Directory.Exists(path).ShouldBeTrue();			
		}

		[Fact]
		public async Task GetFileOutsideOfStorageTest()
		{
			await _storageService.GetFileAsync("..\\file.txt").ShouldThrowAsync<InvalidOperationException>();
		}

		[Fact]
		public async Task DeleteFileOutsideOfStorageTest()
		{
			await _storageService.DeleteFileAsync("..\\file.txt").ShouldThrowAsync<InvalidOperationException>();
		}

		[Fact]
		public async Task GetNotExistingFileTest()
		{
			await _storageService.GetFileAsync("file.txt").ShouldThrowAsync<FileNotFoundException>();
		}

		[Fact]
		public async Task DeleteNotExistingFileTest()
		{
			await _storageService.DeleteFileAsync("file.txt").ShouldThrowAsync<FileNotFoundException>();
		}

		[Fact]
		public async Task UploadFileOutsideOfStorageTest()
		{
			using var fileStream = new MemoryStream(new byte[] { 49, 50, 51 });
			await _storageService.SaveFileToAsync(fileStream, "..\\file.txt").ShouldThrowAsync<InvalidOperationException>();
		}

		[Theory]
		[InlineData("file.txt")]
		[InlineData("folder\\file.byte")]
		public async Task UploadAndDownloadFileTest(string filePath)
		{
			try
			{
				var uploadedFile = new byte[] { 49, 50, 51 };
				using var fileStream = new MemoryStream(uploadedFile);
				await _storageService.SaveFileToAsync(fileStream, filePath);
				var downloadedFile = await _storageService.GetFileAsync(filePath);
				downloadedFile.ShouldBe(uploadedFile);
			}
			finally
			{
				var fullPath = Path.GetFullPath(Path.Combine(_storagePath, filePath));
				if (File.Exists(fullPath))
				{
					File.Delete(fullPath);
				}
			}
		}
		[Theory]
		[InlineData("HAHAHAHHAAH.txt")]
		[InlineData(@"somefolder\hahahahah.txt")]
        public async Task UploadAndDeleteFileTest(string filePath)
        {
            var uploadedFile = new byte[] { 49, 50, 51 };
            using var fileStream = new MemoryStream(uploadedFile);
            await _storageService.SaveFileToAsync(fileStream, filePath);
			await _storageService.DeleteFileAsync(filePath);

			Directory.Exists(filePath).ShouldBeFalse();
        }

		[Theory]
        [InlineData("path/here")]
		[InlineData("path/must/be/here")]
        public async Task CreateFolder(string path)
        {
            try
            {
                await _storageService.CreateFolder(path);
				Directory.Exists(Path.Combine("cloud", path)).ShouldBeTrue();
            }
            finally
            {
				Directory.Delete(Path.Combine("cloud", "path"), true);
            }
		}

		[Fact]
        public async Task DeleteFolder()
        {
            Directory.CreateDirectory("path/here");
            Directory.CreateDirectory("path/here2");
            File.Create("path/file.txt").Close();
            File.Create("path/here/file2.txt").Close();

			await _storageService.DeleteFolder("path");
			Directory.Exists(Path.Combine("cloud", "path")).ShouldBeFalse();
        }
	}
}
