using Api.Models.Attach;

namespace Api.Services
{
    public class AttachService
	{
		private async Task<MetadataModel> UploadFile(IFormFile file)
		{
			var tempPath = Path.GetTempPath();
			var meta = new MetadataModel
			{
				TempId = Guid.NewGuid(),
				Name = file.FileName,
				MimeType = file.ContentType,
				Size = file.Length
			};

			var newPath = Path.Combine(tempPath, meta.TempId.ToString());

			var fileinfo = new FileInfo(newPath);
			if (fileinfo.Exists)
			{
				throw new Exception("file exist");
			}
			else
			{
				using (var stream = File.Create(newPath))
				{
					await file.CopyToAsync(stream);
				}
				return meta;
			}
		}

		public async Task<List<MetadataModel>> UploadFiles(List<IFormFile> files)
		{
			var result = new List<MetadataModel>();
			foreach (var file in files)
			{
				result.Add(await UploadFile(file));
			}
			return result;
		}

		public string SaveMetaToAttaches(MetadataModel meta)
		{
			var tempFI = new FileInfo(Path.Combine(Path.GetTempPath(), meta.TempId.ToString()));
			if (!tempFI.Exists)
				throw new Exception("file not found");

			var path = Path.Combine(Directory.GetCurrentDirectory(), "Attaches", meta.TempId.ToString());
			var destFI = new FileInfo(path);
			if (destFI.Directory != null && !destFI.Directory.Exists)
				destFI.Directory.Create();
			File.Copy(tempFI.FullName, path, true);
			return path;
		}
	}
}
