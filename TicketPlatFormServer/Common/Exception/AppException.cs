using System.Net;

namespace TicketPlatFormServer.Common;

public class AppException : Exception
{
    // StatusCode는 내가 직접 셋팅 하는게 아니므로 get 
    public HttpStatusCode StatusCode { get; }
    
    // 생성자
    public AppException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest): base(message) // Exception의 message를 호출  
    {
        StatusCode = statusCode;
    }
}