using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Ustagram.Application.Abstractions;

namespace Ustagram.Application.Services;

public class FileService : IFileService
{
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public FileService(IHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }
    
    public async Task<string> SaveFileAsync(IFormFile file, string subDirectory)
    {
        try
        {
            if (file == null || file.Length == 0)
                return null;
            
            var uploadsPath = Path.Combine(_environment.ContentRootPath, subDirectory);
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);
            
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine(subDirectory, fileName).Replace("\\", "/");
        }
        catch (Exception ex)
        {
            throw new Exception("File upload failed", ex);
        }
    }
    
    public string GetFileUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) 
            return $"{_configuration["BaseUrl"]}/assets/default-profile.png";
        
        return $"{_configuration["BaseUrl"]}/{filePath.Replace("\\", "/")}";
    }

    public FileStream GetFile(string filePath)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
        if (!System.IO.File.Exists(fullPath))
            return null;

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
    }

    public bool DeleteFile(string filePath)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
        if (!System.IO.File.Exists(fullPath))
            return false;

        System.IO.File.Delete(fullPath);
        return true;
    }
    
}