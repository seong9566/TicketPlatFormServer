using TicketPlatFormServer.DTO;

namespace TicketPlatFormServer.Common;

/// <summary>
/// 역할
/// 0. 미들웨어 -> Controller -> Service -> Repository 를 실행 하고,중간에 발생한 에러를 여기서 처리.
/// 1. API 예외 처리
/// 2. AppException은 비즈니스 에러 처리 ( 내가 직접 message를 작성 해서 return )
/// 3. 일반 Exception은 서버에러(500)으로 처리
/// </summary>
public class GlobalExceptionMiddleware
{
    // 의존성 주입
    private readonly RequestDelegate _next;
    
    // 생성자 
    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    
    public async Task Invoke(HttpContext context)
    {
        try
        {
            // Controller -> Service -> Repository 로직을 실행 
            await _next(context);
        }
        // 에러는 커스텀 에러로 처리 
        catch (AppException e)
        {
            // AppException이 갖고 있는 StatusCode를 미들웨어에서 그대로 사용
            context.Response.StatusCode = (int)e.StatusCode;

            await context.Response.WriteAsJsonAsync(new ApiResponse<object>(
                message: e.Message,
                data: null,
                statusCode: context.Response.StatusCode
            ));
        }
        catch (Exception e)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new ApiResponse<object>(
                message: "서버 내부 오류 발생",
                data: null,
                statusCode: context.Response.StatusCode
                ));
        }
    }
    
}