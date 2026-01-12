using Microsoft.AspNetCore.Http;

namespace TicketPlatFormServer.Services.FileUpload;

public interface IFileUploadService
{
    Task<string> UploadChatImageAsync(IFormFile file, long userId, long roomId);
    Task<bool> DeleteFileAsync(string fileUrl);
}
