namespace TicketPlatFormServer.Services.FileUpload;

public static class MagicBytesValidator
{
    // Magic bytes를 확장자별로 매핑
    private static readonly Dictionary<string, byte[][]> MagicBytes = new()
    {
        { ".jpg", [new byte[] { 0xFF, 0xD8, 0xFF }] },
        { ".jpeg", [new byte[] { 0xFF, 0xD8, 0xFF }] },
        { ".png", [new byte[] { 0x89, 0x50, 0x4E, 0x47 }] },
        { ".gif", [new byte[] { 0x47, 0x49, 0x46, 0x38 }] },
        { ".webp", [new byte[] { 0x52, 0x49, 0x46, 0x46 }] },
        { ".bmp", [new byte[] { 0x42, 0x4D }] },
        { ".svg", [
            new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C }, // <?xml
            new byte[] { 0x3C, 0x73, 0x76, 0x67 }       // <svg
        ] },
        { ".heic", [new byte[] { 0x00, 0x00, 0x00 }] },  // ftyp 체크는 복잡하므로 간단히 처리
        { ".heif", [new byte[] { 0x00, 0x00, 0x00 }] },
        { ".avif", [new byte[] { 0x00, 0x00, 0x00 }] }
    };

    /// <summary>
    /// 파일의 magic bytes가 확장자와 일치하는지 검증
    /// </summary>
    public static async Task<(bool IsValid, string DebugInfo)> ValidateAsync(Stream stream, string extension)
    {
        var ext = extension.ToLowerInvariant();

        if (!MagicBytes.TryGetValue(ext, out var expectedMagicBytes))
        {
            return (false, $"Extension {ext} not supported");
        }

        var maxLength = expectedMagicBytes.Max(mb => mb.Length);
        var buffer = new byte[Math.Max(maxLength, 12)]; // HEIC/HEIF/AVIF 검증을 위해 최소 12바이트

        var originalPosition = stream.Position;
        stream.Position = 0;

        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
        stream.Position = originalPosition;

        var actualBytes = BitConverter.ToString(buffer.Take(Math.Min(bytesRead, 16)).ToArray());
        var expectedBytesStr = string.Join(" | ", expectedMagicBytes.Select(mb => BitConverter.ToString(mb)));

        if (bytesRead < maxLength)
        {
            return (false, $"Not enough bytes: read {bytesRead}, need {maxLength}. Actual: {actualBytes}");
        }

        // 특수 파일 형식 검증
        bool isValid;
        if (ext == ".webp")
        {
            isValid = await ValidateWebpAsync(stream, buffer);
        }
        else if (ext is ".heic" or ".heif" or ".avif")
        {
            isValid = ValidateHeicHeifAvif(buffer, ext);
        }
        else
        {
            isValid = expectedMagicBytes.Any(magic =>
                buffer.Take(magic.Length).SequenceEqual(magic));
        }

        var debugInfo = $"Extension: {ext}, BytesRead: {bytesRead}, Expected: {expectedBytesStr}, Actual: {actualBytes}, Valid: {isValid}";
        return (isValid, debugInfo);
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

    /// <summary>
    /// HEIC/HEIF/AVIF 파일 검증 (ISO Base Media File Format)
    /// </summary>
    private static bool ValidateHeicHeifAvif(byte[] buffer, string extension)
    {
        // 최소 12바이트 필요 (ftyp box 확인)
        if (buffer.Length < 12)
        {
            return false;
        }

        // ftyp box 확인 (4-7 바이트가 'ftyp'여야 함)
        if (!buffer.Skip(4).Take(4).SequenceEqual(new byte[] { 0x66, 0x74, 0x79, 0x70 }))
        {
            return false;
        }

        // Major brand 확인 (8-11 바이트)
        var majorBrand = System.Text.Encoding.ASCII.GetString(buffer, 8, 4);

        return extension switch
        {
            ".heic" => majorBrand.StartsWith("heic") || majorBrand.StartsWith("mif1"),
            ".heif" => majorBrand.StartsWith("heif") || majorBrand.StartsWith("mif1"),
            ".avif" => majorBrand.StartsWith("avif"),
            _ => false
        };
    }

    /// <summary>
    /// 파일의 실제 타입을 magic bytes로 감지
    /// </summary>
    public static async Task<string?> DetectFileTypeAsync(Stream stream)
    {
        var buffer = new byte[12]; // 대부분의 magic bytes 확인에 충분

        var originalPosition = stream.Position;
        stream.Position = 0;

        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
        stream.Position = originalPosition;

        if (bytesRead < 4)
        {
            return null;
        }

        // PNG 체크 (89 50 4E 47)
        if (buffer.Take(4).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }))
        {
            return ".png";
        }

        // JPEG 체크 (FF D8 FF)
        if (buffer.Take(3).SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }))
        {
            return ".jpg";
        }

        // GIF 체크 (47 49 46 38)
        if (buffer.Take(4).SequenceEqual(new byte[] { 0x47, 0x49, 0x46, 0x38 }))
        {
            return ".gif";
        }

        // BMP 체크 (42 4D)
        if (buffer.Take(2).SequenceEqual(new byte[] { 0x42, 0x4D }))
        {
            return ".bmp";
        }

        // WebP 체크 (52 49 46 46 ... 57 45 42 50)
        if (bytesRead >= 12 &&
            buffer.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 }) &&
            buffer.Skip(8).Take(4).SequenceEqual(new byte[] { 0x57, 0x45, 0x42, 0x50 }))
        {
            return ".webp";
        }

        // SVG 체크 (3C 3F 78 6D 6C = <?xml 또는 3C 73 76 67 = <svg)
        if (bytesRead >= 5)
        {
            if (buffer.Take(5).SequenceEqual(new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C }) ||
                buffer.Take(4).SequenceEqual(new byte[] { 0x3C, 0x73, 0x76, 0x67 }))
            {
                return ".svg";
            }
        }

        // HEIC/HEIF/AVIF 체크 (ftyp box)
        if (bytesRead >= 12 && buffer.Skip(4).Take(4).SequenceEqual(new byte[] { 0x66, 0x74, 0x79, 0x70 }))
        {
            var majorBrand = System.Text.Encoding.ASCII.GetString(buffer, 8, 4);

            if (majorBrand.StartsWith("heic") || majorBrand.StartsWith("mif1"))
            {
                return ".heic";
            }
            if (majorBrand.StartsWith("heif"))
            {
                return ".heif";
            }
            if (majorBrand.StartsWith("avif"))
            {
                return ".avif";
            }
        }

        return null;
    }
}
