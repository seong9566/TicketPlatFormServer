namespace TicketPlatFormServer.Common;

/// <summary>
/// 랜덤 닉네임 생성 유틸리티
/// </summary>
public static class NicknameGenerator
{
    /// <summary>
    /// 형용사 목록
    /// </summary>
    private static readonly string[] Adjectives = new[]
    {
        "활발한", "조용한", "귀여운", "멋진", "친절한",
        "용감한", "똑똑한", "재미있는", "사랑스러운", "신비한",
        "빠른", "느긋한", "행복한", "씩씩한", "상냥한",
        "차분한", "열정적인", "명랑한", "다정한", "당당한",
        "겸손한", "긍정적인", "창의적인", "유쾌한", "온화한",
        "발랄한", "성실한", "진지한", "솔직한", "밝은"
    };

    /// <summary>
    /// 명사 목록
    /// </summary>
    private static readonly string[] Nouns = new[]
    {
        "토끼", "사자", "호랑이", "곰", "여우",
        "고양이", "강아지", "팬더", "코알라", "햄스터",
        "다람쥐", "펭귄", "돌고래", "사슴", "너구리",
        "늑대", "치타", "표범", "기린", "코끼리",
        "원숭이", "앵무새", "독수리", "부엉이", "참새",
        "고래", "물개", "수달", "비버", "알파카"
    };

    /// <summary>
    /// 랜덤 닉네임 생성 (형용사 + 명사)
    /// </summary>
    /// <returns>생성된 닉네임 (예: "활발한토끼")</returns>
    public static string Generate()
    {
        var adjective = Adjectives[Random.Shared.Next(Adjectives.Length)];
        var noun = Nouns[Random.Shared.Next(Nouns.Length)];
        return $"{adjective}{noun}";
    }

    /// <summary>
    /// 랜덤 닉네임 생성 (숫자 포함)
    /// 중복 방지를 위해 뒤에 숫자 추가
    /// </summary>
    /// <returns>생성된 닉네임 (예: "활발한토끼123")</returns>
    public static string GenerateWithNumber()
    {
        var adjective = Adjectives[Random.Shared.Next(Adjectives.Length)];
        var noun = Nouns[Random.Shared.Next(Nouns.Length)];
        var number = Random.Shared.Next(100, 1000); // 100~999 사이 랜덤 숫자
        return $"{adjective}{noun}{number}";
    }

    /// <summary>
    /// 중복되지 않는 닉네임 생성 (최대 시도 횟수 제한)
    /// </summary>
    /// <param name="isExistsFunc">닉네임 존재 여부 확인 함수</param>
    /// <param name="maxAttempts">최대 시도 횟수 (기본 10회)</param>
    /// <returns>중복되지 않는 닉네임</returns>
    /// <exception cref="InvalidOperationException">최대 시도 횟수 초과 시</exception>
    public static async Task<string> GenerateUniqueAsync(
        Func<string, Task<bool>> isExistsFunc,
        int maxAttempts = 10)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            // 처음 5번은 숫자 없이 시도, 이후는 숫자 포함
            var nickname = i < 5 ? Generate() : GenerateWithNumber();

            // 중복 확인
            var exists = await isExistsFunc(nickname);
            if (!exists)
            {
                return nickname;
            }
        }

        // 최대 시도 횟수 초과 시 타임스탬프 포함
        var fallbackNickname = $"{Generate()}{DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 10000}";
        return fallbackNickname;
    }
}
