using OpenAC.Net.DFe.Core.Storage;
using OpenAC.Net.DFe.Core.Tests.Http;

namespace OpenAC.Net.DFe.Core.Tests.Storage;

public class DFeFileSystemDocumentStoreTests
{
    [Test]
    public async Task ConcurrentWritesPreserveEveryDocumentWithoutBom()
    {
        using var directory = new TestDirectory();
        await Task.WhenAll(Enumerable.Range(0, 16).Select(index => DFeFileSystemDocumentStore.Instance.SaveAsync(
            new DFeDocument(System.IO.Path.Combine(directory.Path, "nota.xml"), $"ação {index}"))));
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
        var document = new DFeDocument(System.IO.Path.Combine(directory.Path, "dps.xml"), "original",
            DFeArquivoExiste.LancarErro);
        await DFeFileSystemDocumentStore.Instance.SaveAsync(document);
        await Assert.That(async () => await DFeFileSystemDocumentStore.Instance.SaveAsync(document))
            .Throws<IOException>();
        await Assert.That(await File.ReadAllTextAsync(System.IO.Path.Combine(directory.Path, "dps.xml"))).IsEqualTo("original");
    }

    [Test]
    public async Task CancelledWriteDoesNotCreateDirectories()
    {
        using var directory = new TestDirectory();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.That(async () => await DFeFileSystemDocumentStore.Instance.SaveAsync(
            new DFeDocument(System.IO.Path.Combine(directory.Path, "nota.xml"), "nota"), cancellation.Token))
            .Throws<OperationCanceledException>();
        await Assert.That(Directory.Exists(directory.Path)).IsFalse();
    }

    [Test]
    [Arguments("")]
    [Arguments("nota.xml")]
    [Arguments("../nota.xml")]
    [Arguments("/")]
    public async Task RejectsIncompleteFilePaths(string caminho)
    {
        await Assert.That(() => new DFeDocument(caminho, "nota")).Throws<ArgumentException>();
    }

    [Test]
    public async Task OverwriteReplacesAndTruncatesExistingContent()
    {
        using var directory = new TestDirectory();
        var caminho = System.IO.Path.Combine(directory.Path, "dps.xml");
        await DFeFileSystemDocumentStore.Instance.SaveAsync(new DFeDocument(caminho, "conteúdo original longo"));
        await DFeFileSystemDocumentStore.Instance.SaveAsync(new DFeDocument(caminho, "novo",
            DFeArquivoExiste.Sobrescrever));
        await Assert.That(await File.ReadAllTextAsync(caminho)).IsEqualTo("novo");
        await Assert.That(Directory.GetFiles(directory.Path).Length).IsEqualTo(1);
    }

    [Test]
    public async Task AcceptsCustomDocumentImplementation()
    {
        using var directory = new TestDirectory();
        var caminho = System.IO.Path.Combine(directory.Path, "custom.xml");
        await DFeFileSystemDocumentStore.Instance.SaveAsync(new CustomDocument(caminho));
        await Assert.That(await File.ReadAllTextAsync(caminho)).IsEqualTo("customizado");
    }

    [Test]
    public async Task InvalidCollisionBehaviorDoesNotCreateDirectory()
    {
        using var directory = new TestDirectory();
        await Assert.That(async () => await DFeFileSystemDocumentStore.Instance.SaveAsync(
            new CustomDocument(System.IO.Path.Combine(directory.Path, "custom.xml"),
                (DFeArquivoExiste)99))).Throws<ArgumentOutOfRangeException>();
        await Assert.That(Directory.Exists(directory.Path)).IsFalse();
    }

    private sealed class CustomDocument(string caminho,
        DFeArquivoExiste comportamento = DFeArquivoExiste.AdicionarSufixo) : IDFeDocument
    {
        public string Caminho => caminho;
        public string Conteudo => "customizado";
        public DFeArquivoExiste ArquivoExiste => comportamento;
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
        var diretorio = config.GetDocumentPath(CategoriasTeste.Dps,
            "12.345.678/0001-90", new DateTime(2026, 10, 28));
        await DFeFileSystemDocumentStore.Instance.SaveAsync(
            new DFeDocument(System.IO.Path.Combine(diretorio, "dps.xml"), "dps"));
        var expected = System.IO.Path.Combine(directory.Path, "12345678000190", "2026", "10", CategoriasTeste.Dps, "dps.xml");
        await Assert.That(File.Exists(expected)).IsTrue();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    [Arguments(".")]
    [Arguments("..")]
    [Arguments("../DPS")]
    [Arguments("DPS/Envio")]
    [Arguments("DPS\\Envio")]
    [Arguments("C:DPS")]
    [Arguments("/DPS")]
    public async Task InvalidCategoryDoesNotCreateDirectory(string categoria)
    {
        using var directory = new TestDirectory();
        var config = new HttpTestFiles { PathSalvar = directory.Path, AdicionarLiteral = true };
        await Assert.That(() => config.GetDocumentPath(categoria)).Throws<ArgumentException>();
        await Assert.That(Directory.Exists(directory.Path)).IsFalse();
    }

    [Test]
    public async Task NullCategoryDoesNotCreateDirectory()
    {
        using var directory = new TestDirectory();
        var config = new HttpTestFiles { PathSalvar = directory.Path };
        await Assert.That(() => config.GetDocumentPath(null!)).Throws<ArgumentException>();
        await Assert.That(Directory.Exists(directory.Path)).IsFalse();
    }

    private static class CategoriasTeste
    {
        internal const string Dps = "DPS";
    }

    private sealed class TestDirectory : IDisposable
    {
        internal string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dfe-store-" + Guid.NewGuid());
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, true); }
    }
}
