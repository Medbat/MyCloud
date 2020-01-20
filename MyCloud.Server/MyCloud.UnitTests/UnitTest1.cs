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
				var uplodedFile = new byte[] { 49, 50, 51 };
				using var fileStream = new MemoryStream(uplodedFile);
				await _storageService.SaveFileToAsync(fileStream, filePath);
				var downloadedFile = await _storageService.GetFileAsync(filePath);
				downloadedFile.ShouldBe(uplodedFile);
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
	}
}
