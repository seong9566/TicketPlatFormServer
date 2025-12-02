using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.Enum;

namespace TicketPlatFormServer.Services.User;

public class UserService : IUserService

{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }
    
    public async Task<RegisterUserRespDto> RegisterUser(RegisterUserReqDto dto)
    {
        // 1. 중복 이메일 체크 
        var exists = await _repo.GetByEmail(dto.Email);
        // 중복
        if (exists != null)
        {
            // 추후 BaseResponseModel로 변경 할 것.
            // code,message,data
            throw new AppException(message:"이미 가입된 계정입니다.",statusCode: HttpStatusCode.AlreadyReported);
        }
        
        // 2. 가입 유형 검증
        var validProvider = System.Enum.GetValues<UserRegisterProviderEnum>();
        if (!System.Enum.TryParse<UserRegisterProviderEnum>(dto.Provider, true, out var providerEnum))
        {
            throw new AppException(message: "허용되지 않은 가입 유형 입니다.", statusCode: HttpStatusCode.BadRequest);
        }
        
        // 3. 비밀번호 암호화
        string passwordHash = (dto.Provider == nameof(UserRegisterProviderEnum.Email)
            ? BCrypt.Net.BCrypt.HashPassword(dto.Password)
            : null)!;
        
        // 4. Dto -> Entity 
        var reqEntity = new DBModel.User
        {
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = passwordHash,
            Role = dto.Role.ToUpper(),
            Provider = dto.Provider
        };

        // 5. DB에 저장 
        var saved = await _repo.Sign(reqEntity);
        
        // Entity -> Dto 
        return new RegisterUserRespDto
        { 
            Email = saved.Email,
            Phone = saved.Phone ?? "",
            // ignoreCase : 대소문자 무시 
            Role = System.Enum.Parse<UserRoleEnum>(saved.Role, ignoreCase: true),
            Provider = System.Enum.Parse<UserRegisterProviderEnum>(saved.Provider, ignoreCase: true)
        };
    }
}