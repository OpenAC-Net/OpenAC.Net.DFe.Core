# OpenAC.Net.DFe.Generator

**OpenAC.Net.DFe.Generator** é um **Incremental Source Generator (Roslyn)** para C# e .NET que substitui integralmente a reflexão (*Reflection*) na serialização e deserialização de documentos XML do padrão OpenAC.Net DFe (DF-e, NF-e, NFC-e, CT-e, MDF-e, SAT/CF-e, NFS-e, etc.).

O gerador executa em tempo de compilação (*compile-time*), inspecionando classes parciais anotadas com os atributos do OpenAC.Net e gerando código C# nativo, altamente performático, seguro contra tipos nulos e 100% compatível com **AOT (*Ahead-of-Time*)** e **Trimming**.

---

## 🚀 Principais Vantagens

- **Zero Reflection em Runtime**: Elimina o uso de `PropertyInfo`, `GetValue`, `SetValue` e construtores dinâmicos durante a execução.
- **Altíssimo Desempenho**: Leitura e gravação de XML com acesso direto às propriedades e conversões de alta performance centralizadas no `DFeSerializerHelper`.
- **Compatível com Native AOT**: Adequado para aplicações compiladas nativamente no .NET 8, .NET 9 e superiores.
- **Sem Dependência em Deploy**: O gerador é executado exclusivamente pelo compilador C# (`csc`/`dotnet build`). Nenhuma DLL adicional de análise é necessária no ambiente de produção.
- **Detecção Automática de Mudanças**: Utiliza a API incremental do Roslyn (`IIncrementalGenerator`), recalculando apenas as classes modificadas para compilações ultra-rápidas na IDE.

---

## 📦 Como o Gerador é Distribuído

O gerador já vem **embutido diretamente no pacote NuGet `OpenAC.Net.DFe.Core`** sob a pasta especial `analyzers/dotnet/cs/`.

Ao instalar o `OpenAC.Net.DFe.Core` em qualquer projeto:
```bash
dotnet add package OpenAC.Net.DFe.Core
```
O compilador Roslyn ativa o gerador automaticamente, sem necessidade de instalar pacotes NuGet secundários.

---

## 🛠️ Como Utilizar

Para que uma classe tenha os métodos de serialização e deserialização gerados automaticamente, declare-a como `partial` e utilize os atributos do OpenAC.Net.

### 1. Declaração do Modelo

```csharp
using System;
using System.Collections.Generic;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;

namespace MeuProjeto.Modelos;

[DFeRoot("NFe", Namespace = "http://www.portalfiscal.inf.br/nfe")]
public partial class NotaFiscal : DFeDocument<NotaFiscal>
{
    [DFeAttribute(TipoCampo.Str, "Id")]
    public string Id { get; set; } = string.Empty;

    [DFeElement(TipoCampo.DatHorTz, "dhEmi", Ocorrencia = Ocorrencia.Obrigatoria)]
    public DateTimeOffset DataEmissao { get; set; }

    [DFeElement(TipoCampo.De2, "vNF", Ocorrencia = Ocorrencia.Obrigatoria)]
    public decimal ValorTotal { get; set; }

    [DFeElement(TipoCampo.De2, "vDesconto", Ocorrencia = Ocorrencia.MaiorQueZero)]
    public decimal ValorDesconto { get; set; }

    [DFeCollection("itens")]
    [DFeItem(typeof(ItemNota), "det")]
    public List<ItemNota> Itens { get; set; } = new();

    // Controle condicional de serialização:
    public bool ShouldSerializeValorDesconto() => ValorDesconto > 0;
}

public partial class ItemNota
{
    [DFeAttribute(TipoCampo.Int, "nItem")]
    public int NumeroItem { get; set; }

    [DFeElement(TipoCampo.Str, "cProd", Ocorrencia = Ocorrencia.Obrigatoria)]
    public string CodigoProduto { get; set; } = string.Empty;

    [DFeElement(TipoCampo.De4, "vUnCom", Ocorrencia = Ocorrencia.Obrigatoria)]
    public decimal ValorUnitario { get; set; }
}
```

### 2. Serializando para XML

```csharp
var nfe = new NotaFiscal
{
    Id = "NFe3526...",
    DataEmissao = DateTimeOffset.Now,
    ValorTotal = 150.50m,
    Itens =
    {
        new ItemNota { NumeroItem = 1, CodigoProduto = "PROD01", ValorUnitario = 150.50m }
    }
};

// Método de extensão gerado em tempo de compilação
string xml = nfe.WriteToXml();
Console.WriteLine(xml);
```

### 3. Deserializando a partir de XML

```csharp
// Método estático gerado em tempo de compilação
NotaFiscal nfe = NotaFiscal.ReadFromXml(xml);
Console.WriteLine($"Nota carregada: {nfe.Id} - Total: {nfe.ValorTotal:C2}");
```

---

## 🏷️ Atributos e Recursos Suportados

| Atributo / Recurso | Descrição |
| :--- | :--- |
| `[DFeRoot]` | Define o elemento raiz do XML, namespace padrão e nome da tag. |
| `[DFeElement]` | Mapeia propriedades para elementos XML com formatação via `TipoCampo` (strings, decimais com precisão configurável, datas, etc.). |
| `[DFeAttribute]` | Mapeia propriedades para atributos do elemento XML. |
| `[DFeCollection]` e `[DFeItem]` | Mapeia coleções (`List<T>`, `DFeCollection<T>`, `T[]`) e itens polimórficos (`IXmlItem`). |
| `[DFeDictionary]` | Mapeia dicionários (`IDictionary<TKey, TValue>`) gerando chaves como atributos ou elementos filhos. |
| `[DFeEnum]` | Converte enums fortemente tipados para strings específicas da documentação técnica da SEFAZ. |
| `[DFeSignInfoElement]` | Suporte à assinatura digital XML-DSig padrão SEFAZ. |
| `[DFeIgnore]` | Ignora a propriedade na serialização e deserialização. |
| `Ocorrencia` | Suporte a `Obrigatoria`, `NaoObrigatoria` e `MaiorQueZero`. |
| `ShouldSerialize[NomePropriedade]()` | Métodos condicionais dinâmicos para inclusão/omissão de tags no XML. |
| `UseCData` | Envolve valores textuais em blocos `<![CDATA[...]]>`. |

---

## 🔍 Inspecionando o Código Gerado

Para visualizar os arquivos `.DFe.g.cs` gerados pelo compilador na pasta do seu projeto, adicione ao seu arquivo `.csproj`:

```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Os arquivos gerados ficarão disponíveis em `obj/Generated/OpenAC.Net.DFe.Generator/...`.

---

## 🧪 Testes Unitários

O projeto conta com uma suíte abrangente de testes unitários utilizando o **TUnit**, cobrindo:
- Serialização e deserialização de todos os formatos de `TipoCampo` (primitivos, decimais com 2 a 10 casas decimais, datas ISO, SAT/CF-e, TimeZone).
- Enums mapeados e nulos.
- Coleções genéricas, arrays e coleções aninhadas pai-filho (`DFeParentCollection`).
- Polimorfismo e interfaces.
- Dicionários com chave em atributo/elemento.
- CDATA, namespaces customizados e validações de ocorrência.
- Testes diretos do analisador Roslyn `IIncrementalGenerator`.

Para executar a suíte de testes:
```bash
dotnet test
```

---

## 📄 Licença

Distribuído sob a licença **MIT**. Veja o arquivo [LICENSE](../../LICENSE) para mais detalhes.