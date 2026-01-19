namespace TicketPlatFormServer.Services.FileUpload;

public static class MagicBytesValidator
{
    private static readonly Dictionary<string, byte[][]> MagicBytes = new()
    {
        { ".jpg", [new byte[] { 0xFF, 0xD8, 0xFF }] },
        { ".jpeg", [new byte[] { 0xFF, 0xD8, 0xFF }] },
        { ".png", [new byte[] { 0x89, 0x50, 0x4E, 0x47 }] },
        { ".gif", [new byte[] { 0x47, 0x49, 0x46, 0x38 }] },
        { ".webp", [new byte[] { 0x52, 0x49, 0x46, 0x46 }] }
    };

    /// <summary>
    /// 파일의 magic bytes가 확장자와 일치하는지 검증
    /// </summary>
    public static async Task<bool> ValidateAsync(Stream stream, string extension)
    {
        if (!MagicBytes.TryGetValue(extension.ToLowerInvariant(), out var expectedMagicBytes))
        {
            return false;
        }

        var maxLength = expectedMagicBytes.Max(mb => mb.Length);
        var buffer = new byte[maxLength];

        var originalPosition = stream.Position;
        stream.Position = 0;

        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, maxLength));
        stream.Position = originalPosition;

        if (bytesRead < maxLength)
        {
            return false;
        }

        if (extension.ToLowerInvariant() == ".webp")
        {
            return await ValidateWebpAsync(stream, buffer);
        }

        return expectedMagicBytes.Any(magic =>
            buffer.Take(magic.Length).SequenceEqual(magic));
    }

    private static async Task<bool> ValidateWebpAsync(Stream stream, byte[] riffHeader)
    {
        if (!riffHeader.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 }))
        {
            return false;
        }

        var webpSignature = new byte[4];
        var originalPosition = stream.Position;
        stream.Position = 8;
        var bytesRead = await stream.ReadAsync(webpSignature.AsMemory(0, 4));
        stream.Position = originalPosition;

        if (bytesRead < webpSignature.Length)
        {
            return false;
        }

        return webpSignature.SequenceEqual(new byte[] { 0x57, 0x45, 0x42, 0x50 });
    }
}
