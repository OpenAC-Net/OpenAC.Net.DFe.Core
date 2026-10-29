using OpenAC.Net.DFe.Core.Storage;
using OpenAC.Net.DFe.Core.Tests.Http;

namespace OpenAC.Net.DFe.Core.Tests.Storage;

public class DFeFileSystemDocumentStoreTests
{
    [Test]
    public async Task ConcurrentWritesPreserveEveryDocumentWithoutBom()
    {
        using var directory = new TestDirectory();
        await Task.WhenAll(Enumerable.Range(0, 16).Select(index => DFeFileSystemDocumentStore.SaveToDirectoryAsync(
            new(DFeTipoDocumento.DocumentoFiscal, $"ação {index}", "nota.xml", null, DateTime.Now), directory.Path)));
        var files = Directory.GetFiles(directory.Path);
        var contents = await Task.WhenAll(files.Select(file => File.ReadAllTextAsync(file)));
        await Assert.That(files.Length).IsEqualTo(16);
        await Assert.That(contents.Distinct().Count()).IsEqualTo(16);
        foreach (var file in files)
        {
            var bytes = await File.ReadAllBytesAsync(file);
            await Assert.That(bytes[0]).IsEqualTo((byte)'a');
        }
    }

    [Test]
    public async Task PreservingNameFailsOnCollisionWithoutOverwriting()
    {
        using var directory = new TestDirectory();
        var document = new DFeDocument(DFeTipoDocumento.DocumentoFiscal, "original", "dps.xml", null, DateTime.Now, true);
        await DFeFileSystemDocumentStore.SaveToDirectoryAsync(document, directory.Path);
        await Assert.That(async () => await DFeFileSystemDocumentStore.SaveToDirectoryAsync(document, directory.Path))
            .Throws<IOException>();
        await Assert.That(await File.ReadAllTextAsync(System.IO.Path.Combine(directory.Path, "dps.xml"))).IsEqualTo("original");
    }

    [Test]
    public async Task CancelledWriteDoesNotCreateDirectories()
    {
        using var directory = new TestDirectory();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.That(async () => await DFeFileSystemDocumentStore.SaveToDirectoryAsync(
            new(DFeTipoDocumento.DocumentoFiscal, "nota", "nota.xml", null, DateTime.Now), directory.Path, cancellation.Token))
            .Throws<OperationCanceledException>();
        await Assert.That(Directory.Exists(directory.Path)).IsFalse();
    }

    [Test]
    [Arguments("../nota.xml")]
    [Arguments("..\\nota.xml")]
    [Arguments("/tmp/nota.xml")]
    [Arguments("C:\\nota.xml")]
    public async Task RejectsNamesContainingPaths(string name)
    {
        await Assert.That(() => new DFeDocument(DFeTipoDocumento.DocumentoFiscal, "nota", name, null, DateTime.Now))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task DirectoryIsResolvedUsingTaxpayerDateAndCategory()
    {
        using var directory = new TestDirectory();
        var config = new HttpTestFiles
        {
            PathSalvar = directory.Path, SepararPorCNPJ = true, SepararPorAno = true,
            SepararPorMes = true, AdicionarLiteral = true
        };
        await DFeFileSystemDocumentStore.Instance.SaveAsync(
            new(DFeTipoDocumento.DocumentoFiscal, "dps", "dps.xml", "12.345.678/0001-90", new DateTime(2026, 10, 28)), config);
        var expected = System.IO.Path.Combine(directory.Path, "12345678000190", "2026", "10", "DocumentoFiscal", "dps.xml");
        await Assert.That(File.Exists(expected)).IsTrue();
    }

    private sealed class TestDirectory : IDisposable
    {
        internal string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dfe-store-" + Guid.NewGuid());
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, true); }
    }
}
