using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO;

public class ApiResponse<T>
{
    public string Message { get; set; }
    public T? Data { get; set; }
    public int StatusCode { get; set; }

    // 성공 여부 (읽기 전용)
    public bool Success => StatusCode is >= 200 and < 300;
                                    
    public ApiResponse(string message, T? data, int statusCode)
    {
        Message = message;
        Data = data;
        StatusCode = statusCode;
    }
}