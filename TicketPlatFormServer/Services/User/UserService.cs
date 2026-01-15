using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.User;
using TicketPlatFormServer.Enum;
using TicketPlatFormServer.Repository.Token;
using TicketPlatFormServer.Repository.Users;
using TicketPlatFormServer.Services.FileUpload;
using TicketPlatFormServer.Services.Token;

namespace TicketPlatFormServer.Services.User;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IFileUploadService _fileUploadService;

    public UserService(
        IUserRepository repo,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepo,
        IFileUploadService fileUploadService)
    {
        _repo = repo;
        _tokenService = tokenService;
        _refreshTokenRepo = refreshTokenRepo;
        _fileUploadService = fileUploadService;
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
        // dto.Provider값을 대소문자 구분 없이(ignoreCase) 비교후, 찾아진다면 해당 값을 providerEnum에 담도록.
        if (!System.Enum.TryParse<UserRegisterProviderEnum>(dto.Provider, true, out var providerEnum))
        {
            throw new AppException(message: "허용되지 않은 가입 유형 입니다.", statusCode: HttpStatusCode.BadRequest);
        }
        
        // Provider Code를 소문자로 변환
        // 2번 검증 시 true가 되었다면 providerEnum에 해당 값이 담긴다. ex) email,kakao 등등 
        string providerCode = providerEnum.ToString().ToLower();
        
        // provider 값이 정말 있는지 테이블 조회 
        var provider = await _repo.GetProviderByCode(providerCode);
        if (provider == null)
        {
            throw new AppException(message: "허용되지 않은 가입 유형 입니다.", statusCode: HttpStatusCode.BadRequest);
        }
        
        // 3. Role 조회
        
        string roleCode = dto.Role.ToLower(); 
        // role 값이 정말 값이 있는지 테이블 조회
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

        // 6. 랜덤 닉네임 생성 (중복되지 않을 때까지)
        var randomNickname = await NicknameGenerator.GenerateUniqueAsync(
            async (nickname) => await _repo.IsNicknameExistsAsync(nickname)
        );

        // 7. UserProfile 기본값 생성 (UserId는 Repo에서 세팅)
        var userProfile = new DBModel.UserProfile
        {
            Nickname = randomNickname,        // 랜덤 닉네임 자동 생성 (형용사 + 명사)
            ProfileImageUrl = null,           // null이면 Supabase 업로드 안 함
            Bio = null,
            MannerTemperature = 36.5f,
            TotalTradeCount = 0
        };

        // 8. User + UserProfile 저장 (원자적 트랜잭션)
        var saved = await _repo.CreateUserWithProfile(reqEntity, userProfile);

        // 9. 저장 후 Provider와 Role을 다시 로드하기 위해 조회
        var savedWithRelations = await _repo.GetByEmail(saved.Email);
        if (savedWithRelations == null)
        {
            throw new AppException(message: "회원가입 후 사용자 정보를 조회할 수 없습니다.", statusCode: HttpStatusCode.InternalServerError);
        }

        // 10. Entity -> Dto (DB 코드 값을 그대로 내려줌: role -> "user", provider -> "email")
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

        // 4. JWT Token 생성
        var tokenResponse = await _tokenService.GenerateTokensAsync(user, 7);

        // 5. Refresh Token DB 저장
        var refreshToken = new DBModel.RefreshToken
        {
            UserId = user.Id,
            Token = tokenResponse.RefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        };
        await _refreshTokenRepo.SaveRefreshTokenAsync(refreshToken);

        // 6. Entity -> Dto (DB 코드 값 + Token 정보 반환)
        return new LoginUserRespDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone ?? "",
            Role = user.Role.Code,                            // 예: "user"
            Provider = user.Provider.Code,                    // 예: "email"
            LastLoginAt = DateTime.UtcNow,
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = tokenResponse.ExpiresIn,
            TokenType = tokenResponse.TokenType,
            ExpiresAt = tokenResponse.ExpiresAt
        };
    }

    public async Task<UserProfileDto> GetMyProfileAsync(int userId)
    {
        // 1. User 조회 (이메일 정보 포함)
        var user = await _repo.GetByIdAsync(userId);
        if (user == null)
        {
            throw new AppException(message: "사용자를 찾을 수 없습니다.", statusCode: HttpStatusCode.NotFound);
        }

        // 2. 프로필 조회
        var profile = await _repo.GetUserProfileByIdAsync(userId);
        if (profile == null)
        {
            throw new AppException(message: "프로필을 찾을 수 없습니다.", statusCode: HttpStatusCode.NotFound);
        }

        // 3. ProfileImageUrl 처리
        string? profileImageUrl = profile.ProfileImageUrl;

        // Supabase object key인 경우에만 Signed URL로 변환
        // object key는 http(s)로 시작하지 않음 (예: "profiles/user_12/image.jpg")
        if (!string.IsNullOrWhiteSpace(profile.ProfileImageUrl) &&
            !profile.ProfileImageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !profile.ProfileImageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            var signedUrlResult = await _fileUploadService.RefreshSignedUrlAsync(profile.ProfileImageUrl);
            profileImageUrl = signedUrlResult.SignedUrl;
        }

        // 4. Entity -> Dto
        return new UserProfileDto
        {
            UserId = profile.UserId,
            Email = user.Email,
            Nickname = profile.Nickname ?? "",
            ProfileImageUrl = profileImageUrl,
            Bio = profile.Bio,
            MannerTemperature = profile.MannerTemperature,
            TotalTradeCount = profile.TotalTradeCount
        };
    }

    public async Task<UserProfileDto> GetUserProfileAsync(int userId)
    {
        // 내 프로필 조회와 동일 (공개 프로필이므로 같은 정보 반환)
        return await GetMyProfileAsync(userId);
    }

    public async Task<UserProfileDto> UpdateMyProfileAsync(int userId, string? nickname, string? bio, IFormFile? profileImage, bool removeProfileImage)
    {
        // 1. 프로필 조회
        var profile = await _repo.GetUserProfileByIdAsync(userId);
        if (profile == null)
        {
            throw new AppException(message: "프로필을 찾을 수 없습니다.", statusCode: HttpStatusCode.NotFound);
        }

        // 2. 닉네임 검증 및 변경
        if (!string.IsNullOrEmpty(nickname))
        {
            // 길이 검증
            if (nickname.Length > 50)
            {
                throw new AppException(message: "닉네임은 최대 50자까지 입력 가능합니다.", statusCode: HttpStatusCode.BadRequest);
            }

            // 중복 체크 (다른 닉네임으로 변경하는 경우만)
            if (nickname != profile.Nickname)
            {
                var nicknameExists = await _repo.IsNicknameExistsAsync(nickname);
                if (nicknameExists)
                {
                    throw new AppException(message: "이미 사용 중인 닉네임입니다.", statusCode: HttpStatusCode.Conflict);
                }
                profile.Nickname = nickname;
            }
        }

        // 3. Bio 검증 및 업데이트
        if (bio != null)
        {
            // 길이 검증
            if (bio.Length > 500)
            {
                throw new AppException(message: "자기소개는 최대 500자까지 입력 가능합니다.", statusCode: HttpStatusCode.BadRequest);
            }
            profile.Bio = bio;
        }

        // 4. 프로필 이미지 처리
        string? oldImageKey = null;
        string? newImageKey = null;

        if (removeProfileImage)
        {
            // 4-1. 이미지 삭제 요청 (우선순위 높음: profileImage가 함께 오더라도 삭제 우선)
            if (!string.IsNullOrEmpty(profile.ProfileImageUrl))
            {
                // Supabase object key인 경우에만 삭제 예약
                if (!profile.ProfileImageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !profile.ProfileImageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    oldImageKey = profile.ProfileImageUrl;
                }
                profile.ProfileImageUrl = null;
            }
        }
        else if (profileImage != null)
        {
            // 4-2. 새 이미지 업로드 (기존 이미지 삭제는 성공 후)
            // 기존 이미지 키 백업
            if (!string.IsNullOrEmpty(profile.ProfileImageUrl))
            {
                if (!profile.ProfileImageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !profile.ProfileImageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    oldImageKey = profile.ProfileImageUrl;
                }
            }

            // 새 이미지 업로드
            var uploadResult = await _fileUploadService.UploadUserProfileImageAsync(profileImage, userId);
            newImageKey = uploadResult.ObjectKey;
            profile.ProfileImageUrl = newImageKey; // object key를 DB에 저장
        }

        // 5. 프로필 저장 (트랜잭션)
        try
        {
            await _repo.UpdateUserProfileAsync(profile);

            // 6. DB 저장 성공 후 기존 이미지 삭제
            if (oldImageKey != null)
            {
                try
                {
                    await _fileUploadService.DeleteFileAsync(oldImageKey);
                }
                catch (Exception ex)
                {
                    // 이미지 삭제 실패는 로그만 남기고 계속 진행 (DB는 이미 업데이트됨)
                    // TODO: 주기적 정리 작업으로 고아 파일 제거 필요
                    Console.WriteLine($"[Warning] Failed to delete old profile image: {oldImageKey}, Error: {ex.Message}");
                }
            }
        }
        catch
        {
            // 7. DB 저장 실패 시 보상 처리: 새로 업로드한 이미지 삭제
            if (newImageKey != null)
            {
                try
                {
                    await _fileUploadService.DeleteFileAsync(newImageKey);
                }
                catch (Exception ex)
                {
                    // 보상 처리 실패도 로그만 남김
                    Console.WriteLine($"[Warning] Failed to rollback uploaded image: {newImageKey}, Error: {ex.Message}");
                }
            }
            throw;
        }

        // 8. 업데이트된 프로필 반환
        return await GetMyProfileAsync(userId);
    }

    /// <summary>
    /// 프로필 이미지 URL 갱신 (Signed URL 재발급)
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <returns>새로 발급된 Signed URL (이미지 없으면 null)</returns>
    public async Task<string?> RefreshProfileImageUrlAsync(int userId)
    {
        // 1. 프로필 조회
        var profile = await _repo.GetUserProfileByIdAsync(userId);
        if (profile == null)
        {
            throw new AppException(message: "사용자를 찾을 수 없습니다.", statusCode: HttpStatusCode.NotFound);
        }

        // 2. 프로필 이미지가 없는 경우
        if (string.IsNullOrWhiteSpace(profile.ProfileImageUrl))
        {
            return null;
        }

        // 3. 이미 URL인 경우 (http로 시작) 그대로 반환
        if (profile.ProfileImageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            profile.ProfileImageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return profile.ProfileImageUrl;
        }

        // 4. Object Key인 경우: 새 Signed URL 발급
        var signedUrlResult = await _fileUploadService.RefreshSignedUrlAsync(profile.ProfileImageUrl);
        return signedUrlResult.SignedUrl;
    }
}