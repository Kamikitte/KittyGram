using Api.Models.Attach;

namespace Api.Services;

public sealed class AttachService
{
    private static async Task<MetadataModel> UploadFile(IFormFile file)
    {
        var tempPath = Path.GetTempPath();
        var meta = new MetadataModel
        {
            TempId = Guid.NewGuid(),
            Name = file.FileName,
            MimeType = file.ContentType,
            Size = file.Length,
        };

        var newPath = Path.Combine(tempPath, meta.TempId.ToString());

        var fileInfo = new FileInfo(newPath);
        if (fileInfo.Exists)
        {
            throw new Exception("file exist");
        }

        await using var stream = File.Create(newPath);
        await file.CopyToAsync(stream);

        return meta;
    }

    public static async Task<List<MetadataModel>> UploadFiles(List<IFormFile> files)
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
        var tempFile = new FileInfo(Path.Combine(Path.GetTempPath(), meta.TempId.ToString()));
        if (!tempFile.Exists)
        {
            throw new Exception("file not found");
        }

        var path = Path.Combine(Directory.GetCurrentDirectory(), "Attaches", meta.TempId.ToString());
        var destinationFile = new FileInfo(path);
        if (destinationFile.Directory is { Exists: false })
        {
            destinationFile.Directory.Create();
        }

        File.Copy(tempFile.FullName, path, true);
        return path;
    }
}