using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenAC.Net.DFe.Core.Common;

namespace OpenAC.Net.DFe.Core.Storage;

/// <summary>Grava UTF-8 sem BOM, com criação exclusiva e sufixos em caso de colisão.</summary>
public sealed class DFeFileSystemDocumentStore : IDFeDocumentStore
{
    /// <summary>Instância compartilhada para consumidores sem injeção de dependências.</summary>
    public static DFeFileSystemDocumentStore Instance { get; } = new();

    /// <inheritdoc />
    public Task SaveAsync(DFeDocument document, DFeArquivosConfigBase configuration,
        CancellationToken cancellationToken = default)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        cancellationToken.ThrowIfCancellationRequested();
        return SaveToDirectoryAsync(document, configuration.GetDocumentPath(document), cancellationToken);
    }

    /// <summary>Grava em um diretório explícito, permitindo uso por hosts web ou regras próprias.</summary>
    public static async Task SaveToDirectoryAsync(DFeDocument document, string directory,
        CancellationToken cancellationToken = default)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Informe o diretório.", nameof(directory));
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(directory);
        var name = Path.GetFileNameWithoutExtension(document.NomeArquivo);
        var extension = Path.GetExtension(document.NomeArquivo);
        var bytes = new UTF8Encoding(false).GetBytes(document.Conteudo);
        for (var index = 0;; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(directory, index == 0 ? document.NomeArquivo : $"{name}_{index}{extension}");
            FileStream stream;
            try
            {
                stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 4096, true);
            }
            catch (IOException) when (!document.PreservarNomeArquivo && File.Exists(path))
            {
                continue;
            }

            try
            {
                using (stream)
                {
                    await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }

                return;
            }
            catch
            {
                // Remove somente o arquivo criado por este envio, preservando o erro original.
                try
                {
                    File.Delete(path);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }

                throw;
            }
        }
    }
}