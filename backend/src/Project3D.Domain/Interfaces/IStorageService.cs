namespace Project3D.Domain.Interfaces;

/// <summary>
/// Serviço de armazenamento de arquivos.
/// Implementado localmente em dev e via Cloudinary em produção.
/// </summary>
public interface IStorageService
{
    Task<StorageResult> UploadAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task DeleteAsync(string publicId, CancellationToken cancellationToken = default);
    string GetUrl(string publicId);
}

public record StorageResult(string Url, string PublicId, string FileName);
