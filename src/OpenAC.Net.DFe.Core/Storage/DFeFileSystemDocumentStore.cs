using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAC.Net.DFe.Core.Storage;

/// <summary>Grava UTF-8 sem BOM conforme o comportamento definido pelo documento.</summary>
public sealed class DFeFileSystemDocumentStore : IDFeDocumentStore
{
    /// <summary>Instância compartilhada para consumidores sem injeção de dependências.</summary>
    public static DFeFileSystemDocumentStore Instance { get; } = new();

    /// <inheritdoc />
    public async Task SaveAsync(IDFeDocument document, CancellationToken cancellationToken = default)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        cancellationToken.ThrowIfCancellationRequested();
        // Valida também documentos fornecidos por outras implementações da interface.
        var arquivo = new DFeDocument(document.Caminho, document.Conteudo, document.ArquivoExiste);
        var directory = Path.GetDirectoryName(arquivo.Caminho)!;
        var fileName = Path.GetFileName(arquivo.Caminho);
        var name = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var overwrite = arquivo.ArquivoExiste == DFeArquivoExiste.Sobrescrever;
        var bytes = new UTF8Encoding(false).GetBytes(arquivo.Conteudo);
        Directory.CreateDirectory(directory);
        for (var index = 0;; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(directory, index == 0 ? fileName : $"{name}_{index}{extension}");
            FileStream stream;
            try
            {
                stream = new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.Read, 4096, true);
            }
            catch (IOException) when (arquivo.ArquivoExiste == DFeArquivoExiste.AdicionarSufixo && File.Exists(path))
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
                // Remove somente arquivos criados exclusivamente por este envio, preservando o erro original.
                try
                {
                    if (!overwrite) File.Delete(path);
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