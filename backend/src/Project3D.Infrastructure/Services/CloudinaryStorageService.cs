using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Project3D.Domain.Interfaces;

namespace Project3D.Infrastructure.Services;

/// <summary>
/// Serviço de armazenamento Cloudinary para produção.
/// Ativar configurando Upload:UseLocal=false e preenchendo Cloudinary:* no appsettings.
/// </summary>
public class CloudinaryStorageService : IStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryStorageService> _logger;

    public CloudinaryStorageService(IConfiguration config, ILogger<CloudinaryStorageService> logger)
    {
        _logger = logger;
        var account = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<StorageResult> UploadAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder,
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false,
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (result.Error is not null)
            throw new Exception($"Erro no upload Cloudinary: {result.Error.Message}");

        _logger.LogInformation("Upload Cloudinary concluído: {PublicId}", result.PublicId);

        return new StorageResult(result.SecureUrl.ToString(), result.PublicId, fileName);
    }

    public async Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var deleteParams = new DeletionParams(publicId);
        await _cloudinary.DestroyAsync(deleteParams);
        _logger.LogInformation("Arquivo removido do Cloudinary: {PublicId}", publicId);
    }

    public string GetUrl(string publicId)
        => _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
}
