using System;
using System.Security.Cryptography;
using System.Text;

namespace TicketPlatFormServer.Services.Common;

/// <summary>
/// AES-256-GCM 암호화 서비스
/// 민감한 결제 정보 (secret, refund_receive_account, toss_response 등) 암호화용
/// </summary>
public class EncryptionService
{
    private readonly byte[] _key;
    private const int NonceSize = 12; // GCM 표준 Nonce 크기
    private const int TagSize = 16;   // GCM 표준 Tag 크기

    public EncryptionService(string encryptionKey)
    {
        if (string.IsNullOrWhiteSpace(encryptionKey))
        {
            throw new ArgumentException("Encryption key cannot be null or empty", nameof(encryptionKey));
        }

        // 키를 32바이트 (256비트)로 변환
        _key = DeriveKey(encryptionKey);
    }

    /// <summary>
    /// 문자열을 AES-256-GCM으로 암호화하고 Base64 인코딩하여 반환
    /// </summary>
    /// <param name="plainText">암호화할 평문</param>
    /// <returns>Base64 인코딩된 암호문 (Nonce + Tag + Ciphertext)</returns>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));
        }

        // 평문을 바이트 배열로 변환
        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        // Nonce 생성 (12바이트)
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        // Tag 버퍼 (16바이트)
        var tag = new byte[TagSize];

        // 암호문 버퍼
        var cipherBytes = new byte[plainBytes.Length];

        // AES-GCM 암호화
        using var aesGcm = new AesGcm(_key, TagSize);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Nonce + Tag + Ciphertext 결합
        var result = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, result, NonceSize + TagSize, cipherBytes.Length);

        // Base64 인코딩
        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Base64 인코딩된 암호문을 AES-256-GCM으로 복호화
    /// </summary>
    /// <param name="encryptedBase64">Base64 인코딩된 암호문</param>
    /// <returns>복호화된 평문</returns>
    public string Decrypt(string encryptedBase64)
    {
        if (string.IsNullOrEmpty(encryptedBase64))
        {
            throw new ArgumentException("Encrypted text cannot be null or empty", nameof(encryptedBase64));
        }

        try
        {
            // Base64 디코딩
            var encryptedData = Convert.FromBase64String(encryptedBase64);

            // Nonce, Tag, Ciphertext 분리
            if (encryptedData.Length < NonceSize + TagSize)
            {
                throw new CryptographicException("Invalid encrypted data format");
            }

            var nonce = new byte[NonceSize];
            var tag = new byte[TagSize];
            var cipherBytes = new byte[encryptedData.Length - NonceSize - TagSize];

            Buffer.BlockCopy(encryptedData, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(encryptedData, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(encryptedData, NonceSize + TagSize, cipherBytes, 0, cipherBytes.Length);

            // 복호화
            var plainBytes = new byte[cipherBytes.Length];
            using var aesGcm = new AesGcm(_key, TagSize);
            aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Decryption failed", ex);
        }
    }

    /// <summary>
    /// 암호화 키 파생 (PBKDF2 사용)
    /// </summary>
    private static byte[] DeriveKey(string password)
    {
        // 고정 Salt 사용 (프로덕션에서는 환경변수로 관리)
        const string salt = "TicketPlatform-2026-Encryption-Salt";
        var saltBytes = Encoding.UTF8.GetBytes(salt);

        // PBKDF2를 사용하여 256비트 키 생성
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            saltBytes,
            100000, // 반복 횟수
            HashAlgorithmName.SHA256
        );

        return pbkdf2.GetBytes(32); // 256비트 = 32바이트
    }

    /// <summary>
    /// nullable 문자열 암호화 (null이면 null 반환)
    /// </summary>
    public string? EncryptNullable(string? plainText)
    {
        return string.IsNullOrEmpty(plainText) ? null : Encrypt(plainText);
    }

    /// <summary>
    /// nullable 문자열 복호화 (null이면 null 반환)
    /// </summary>
    public string? DecryptNullable(string? encryptedBase64)
    {
        return string.IsNullOrEmpty(encryptedBase64) ? null : Decrypt(encryptedBase64);
    }
}
