using Microsoft.AspNetCore.Http;

namespace Ustagram.Application.Abstractions;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string subDirectory);
    FileStream GetFile(string filePath);
    bool DeleteFile(string filePath);
    public string GetFileUrl(string filePath);
}