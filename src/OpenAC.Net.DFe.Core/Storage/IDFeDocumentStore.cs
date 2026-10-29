using System.Threading;
using System.Threading.Tasks;
using OpenAC.Net.DFe.Core.Common;

namespace OpenAC.Net.DFe.Core.Storage;

/// <summary>Persistência substituível de documentos fiscais e mensagens técnicas.</summary>
public interface IDFeDocumentStore
{
    /// <summary>Persiste o documento usando as regras de diretório do componente.</summary>
    Task SaveAsync(DFeDocument document, DFeArquivosConfigBase configuration,
        CancellationToken cancellationToken = default);
}
