using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Project3D.Domain.Interfaces;

namespace Project3D.Infrastructure.Services;

/// <summary>
/// Serviço de armazenamento local para desenvolvimento.
/// Em produção, substituir por CloudinaryStorageService.
/// Arquivos salvos em /uploads/{folder}/ e servidos estaticamente.
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalStorageService> _logger;

    private static readonly HashSet<string> AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".webp"];

    public LocalStorageService(IWebHostEnvironment env, IConfiguration config, ILogger<LocalStorageService> logger)
    {
        _logger = logger;
        var relativePath = config["Upload:LocalPath"] ?? "uploads";
        _basePath = Path.Combine(env.ContentRootPath, relativePath);
        // URL base — em dev será http://localhost:5000/uploads
        _baseUrl = $"/uploads";
    }

    public async Task<StorageResult> UploadAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        ValidateExtension(fileName);

        var safeFileName = SanitizeFileName(fileName);
        var uniqueName = $"{Guid.NewGuid():N}_{safeFileName}";
        var dirPath = Path.Combine(_basePath, folder);

        Directory.CreateDirectory(dirPath);

        var fullPath = Path.Combine(dirPath, uniqueName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken);

        var publicId = $"{folder}/{uniqueName}";
        var url = $"{_baseUrl}/{publicId}";

        _logger.LogInformation("Arquivo salvo localmente: {Path}", fullPath);

        return new StorageResult(url, publicId, uniqueName);
    }

    public Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, publicId.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Arquivo removido: {Path}", fullPath);
        }
        return Task.CompletedTask;
    }

    public string GetUrl(string publicId)
        => $"{_baseUrl}/{publicId}";

    private static void ValidateExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"Extensão de arquivo não permitida: {ext}. Permitidas: {string.Join(", ", AllowedExtensions)}");
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        // Remover caracteres especiais do nome
        var safe = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
        return string.IsNullOrEmpty(safe) ? $"file{ext}" : $"{safe}{ext}";
    }
}
