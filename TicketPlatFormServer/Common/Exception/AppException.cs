using System.Net;

namespace TicketPlatFormServer.Common;

public class AppException : Exception
{
    // StatusCode는 내가 직접 셋팅 하는게 아니므로 get
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// 기본 생성자 (InnerException 없음)
    /// </summary>
    public AppException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// InnerException을 포함하는 생성자
    /// 예: DB 예외, 외부 API 호출 실패 등 원본 예외 정보를 보존할 때 사용
    /// </summary>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     await externalApiClient.CallAsync();
    /// }
    /// catch (HttpRequestException ex)
    /// {
    ///     throw new AppException("외부 API 호출에 실패했습니다.", HttpStatusCode.BadGateway, ex);
    /// }
    /// </code>
    /// </example>
    public AppException(string message, HttpStatusCode statusCode, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}