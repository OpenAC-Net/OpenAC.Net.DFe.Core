using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAC.Net.DFe.Core.Http;

/// <summary>Leitura e decodificação compartilhadas das respostas HTTP dos componentes fiscais.</summary>
public static class DFeHttpResponse
{
    /// <summary>Lê o conteúdo com cancelamento e preserva os cabeçalhos ao disponibilizá-lo em memória.</summary>
    public static async Task BufferAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        if (response == null) throw new ArgumentNullException(nameof(response));
        cancellationToken.ThrowIfCancellationRequested();
        if (response.Content == null) return;
        var original = response.Content;
        var bytes = await ReadAsByteArrayAsync(original, cancellationToken).ConfigureAwait(false);
        var buffered = new ByteArrayContent(bytes);
        foreach (var header in original.Headers)
            buffered.Headers.TryAddWithoutValidation(header.Key, header.Value);
        response.Content = buffered;
        original.Dispose();
    }

    /// <summary>Lê todos os bytes do conteúdo respeitando o cancelamento nos frameworks suportados.</summary>
    public static async Task<byte[]> ReadAsByteArrayAsync(HttpContent content,
        CancellationToken cancellationToken = default)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        cancellationToken.ThrowIfCancellationRequested();
#if NET5_0_OR_GREATER
        var bytes = await content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
        using var output = new System.IO.MemoryStream();
        var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
        if (stream.CanSeek) stream.Position = 0;
        await stream.CopyToAsync(output, 81920, cancellationToken).ConfigureAwait(false);
        if (stream.CanSeek) stream.Position = 0;
        var bytes = output.ToArray();
#endif
        cancellationToken.ThrowIfCancellationRequested();
        return bytes;
    }

    /// <summary>Lê texto usando BOM, UTF-8 válido e fallback para o charset declarado ou Latin-1.</summary>
    public static async Task<string> ReadAsStringAsync(HttpContent content,
        CancellationToken cancellationToken = default)
    {
        var bytes = await ReadAsByteArrayAsync(content, cancellationToken).ConfigureAwait(false);
        return Decode(bytes, content.Headers.ContentType?.CharSet);
    }

    /// <summary>Decodifica texto fiscal, tolerando cabeçalhos de charset ausentes ou incorretos.</summary>
    public static string Decode(byte[] bytes, string? headerCharset = null)
    {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        switch (bytes.Length)
        {
            case >= 4 when bytes[0] == 0xff && bytes[1] == 0xfe && bytes[2] == 0 && bytes[3] == 0:
                return Encoding.UTF32.GetString(bytes, 4, bytes.Length - 4);

            case >= 4 when bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0xfe && bytes[3] == 0xff:
                return new UTF32Encoding(true, false).GetString(bytes, 4, bytes.Length - 4);

            case >= 2 when bytes[0] == 0xff && bytes[1] == 0xfe:
                return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);

            case >= 2 when bytes[0] == 0xfe && bytes[1] == 0xff:
                return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);

            default:
                try
                {
                    return new UTF8Encoding(false, true).GetString(bytes).TrimStart('\uFEFF');
                }
                catch (DecoderFallbackException)
                {
                    var encoding = Encoding.GetEncoding("ISO-8859-1");
                    if (!string.IsNullOrWhiteSpace(headerCharset))
                    {
                        try
                        {
                            encoding = Encoding.GetEncoding(headerCharset.Trim('"'));
                        }
                        catch (ArgumentException)
                        {
                        }
                        catch (NotSupportedException)
                        {
                        }
                    }

                    return encoding.GetString(bytes).TrimStart('\uFEFF');
                }

        }
    }
}