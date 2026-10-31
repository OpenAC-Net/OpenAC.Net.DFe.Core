using System.Threading;
using System.Threading.Tasks;

namespace OpenAC.Net.DFe.Core.Storage;

/// <summary>Persistência substituível de documentos fiscais e mensagens técnicas.</summary>
public interface IDFeDocumentStore
{
    /// <summary>Persiste o conteúdo no caminho definido pelo documento.</summary>
    Task SaveAsync(IDFeDocument document,
        CancellationToken cancellationToken = default);
}
