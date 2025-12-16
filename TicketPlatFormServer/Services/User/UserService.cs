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

    /// <summary>
    /// Provider Code를 Enum 이름으로 변환
    /// DB의 소문자 code (email, google, kakao, apple)를 Enum 이름으로 변환
    /// </summary>
    private string ConvertProviderCodeToEnumName(string code)
    {
        return code.ToLower() switch
        {
            "email" => "Email",
            "google" => "Google",
            "kakao" => "KaKao", // Enum의 정확한 이름
            "apple" => "Apple",
            _ => code
        };
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
        
        // 2. 가입 유형 검증 및 Provider 조회
        if (!System.Enum.TryParse<UserRegisterProviderEnum>(dto.Provider, true, out var providerEnum))
        {
            throw new AppException(message: "허용되지 않은 가입 유형 입니다.", statusCode: HttpStatusCode.BadRequest);
        }
        
        // Provider Code를 소문자로 변환 (DB에 소문자로 저장됨: email, google, kakao, apple)
        // Enum의 "KaKao"는 ToLower()로 "kakao"가 됨
        string providerCode = providerEnum.ToString().ToLower();
        
        var provider = await _repo.GetProviderByCode(providerCode);
        if (provider == null)
        {
            throw new AppException(message: "허용되지 않은 가입 유형 입니다.", statusCode: HttpStatusCode.BadRequest);
        }
        
        // 3. Role 조회
        
        string roleCode = dto.Role.ToUpper(); 
        var role = await _repo.GetRoleByCode(roleCode);
        if (role == null)
        {
            throw new AppException(message: "허용되지 않은 역할 입니다.", statusCode: HttpStatusCode.BadRequest);
        }
        
        // 4. 비밀번호 암호화
        // providerEnum은 이미 위에서 TryParse 된 Enum 값이므로, 가입 유형이 Email인지만 비교하면 됨
        string passwordHash = (providerEnum == UserRegisterProviderEnum.email
            ? BCrypt.Net.BCrypt.HashPassword(dto.Password)
            : null)!;
         
        // 5. Dto -> Entity 
        var reqEntity = new DBModel.User
        {
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = passwordHash,
            ProviderId = provider.Id,
            RoleId = role.Id
        };

        // 6. DB에 저장 
        var saved = await _repo.Sign(reqEntity);
        
        // 저장 후 Provider와 Role을 다시 로드하기 위해 조회
        var savedWithRelations = await _repo.GetByEmail(saved.Email);
        if (savedWithRelations == null)
        {
            throw new AppException(message: "회원가입 후 사용자 정보를 조회할 수 없습니다.", statusCode: HttpStatusCode.InternalServerError);
        }
        
        // 7. Entity -> Dto (DB 코드 값을 그대로 내려줌: role -> "user", provider -> "email")
        return new RegisterUserRespDto
        { 
            Email = savedWithRelations.Email,
            Phone = savedWithRelations.Phone ?? "",
            Role = savedWithRelations.Role.Code,              // 예: "user"
            Provider = savedWithRelations.Provider.Code       // 예: "email"
        };
    }

    public async Task<LoginUserRespDto> LoginUser(LoginUserReqDto dto)
    {
        // 1. 이메일로 사용자 조회 (Provider와 Role 포함)
        var user = await _repo.GetByEmail(dto.Email);
        if (user == null)
        {
            throw new AppException(message: "이메일 또는 비밀번호가 올바르지 않습니다.", statusCode: HttpStatusCode.Unauthorized);
        }

        // 2. 이메일 가입 유형인 경우 비밀번호 검증
        if (user.Provider.Code.ToLower() == "email")
        {
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new AppException(message: "비밀번호가 설정되지 않았습니다.", statusCode: HttpStatusCode.Unauthorized);
            }

            // BCrypt로 비밀번호 검증
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new AppException(message: "이메일 또는 비밀번호가 올바르지 않습니다.", statusCode: HttpStatusCode.Unauthorized);
            }
        }
        else
        {
            // 소셜 로그인은 비밀번호 검증 불필요
            throw new AppException(message: "소셜 로그인은 해당 제공자를 통해 로그인해주세요.", statusCode: HttpStatusCode.BadRequest);
        }

        // 3. 마지막 로그인 시간 업데이트
        await _repo.UpdateLastLoginAt(user.Id);

        // 4. Entity -> Dto (DB 코드 값을 그대로 내려줌)
        return new LoginUserRespDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone ?? "",
            Role = user.Role.Code,                            // 예: "user"
            Provider = user.Provider.Code,                    // 예: "email"
            LastLoginAt = DateTime.UtcNow
        };
    }
}