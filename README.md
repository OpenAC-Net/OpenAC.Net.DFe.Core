<div align="center">

# OpenAC.Net.DFe.Core

**Núcleo de serialização XML de alta performance, assinatura digital e comunicação com Web Services para Documentos Fiscais Eletrônicos (DF-e).**

[![Nuget version](https://img.shields.io/nuget/v/OpenAC.Net.DFe.Core.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/OpenAC.Net.DFe.Core/)
[![Nuget downloads](https://img.shields.io/nuget/dt/OpenAC.Net.DFe.Core.svg?style=flat-square)](https://www.nuget.org/packages/OpenAC.Net.DFe.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)](LICENSE)
[![Discord](https://img.shields.io/badge/Chat%20on-Discord-7289DA.svg?style=flat-square&logo=discord&logoColor=white)](https://discord.com/invite/brdmJ7Yv6w)

</div>

---

## 📌 Visão Geral

O **OpenAC.Net.DFe.Core** é a biblioteca base do ecossistema [OpenAC .Net](https://openac.net.br/) para desenvolvimento e integração com **Documentos Fiscais Eletrônicos** no Brasil (NF-e, NFC-e, CT-e, MDF-e, SAT/CF-e, NFS-e, BP-e, CIOT, entre outros).

Ele provê a infraestrutura fundamental de:
- **Serialização e Deserialização XML**: Baseada em **Roslyn Source Generator** em tempo de compilação (*Zero Reflection*, alta performance e compatibilidade nativa com AOT/Trimming).
- **Assinatura Digital XML-DSig**: Suporte completo a certificados digitais ICP-Brasil (A1 e A3) com algoritmos SHA-1 e SHA-256.
- **Validação de Schemas XSD**: Validação sintática e estrutural rigorosa contra schemas oficiais da SEFAZ.
- **Clientes de Web Services**: Clientes base para comunicação SOAP e REST com suporte a mTLS, timeout configurável, logs de requisição/resposta e regras de contingência (SVC-AN, SVC-RS, etc.).

---

## 🚀 Principais Recursos

- ⚡ **Source Generator Integrado**: Não utiliza *Reflection* em tempo de execução. O gerador é embutido no próprio pacote e gera métodos C# nativos (`WriteToXml`, `ReadFromXml`) durante a compilação.
- 🔒 **Pronto para Native AOT**: Compatível com compilação Ahead-of-Time (.NET 8, .NET 9 e .NET 10).
- 🏷️ **Anotações Fiscais Especializadas**: Atributos específicos para regras da SEFAZ (`[DFeRoot]`, `[DFeElement]`, `[DFeAttribute]`, `[DFeCollection]`, `[DFeEnum]`, `[DFeDictionary]`).
- 🔢 **Tipagem Fiscal Rigorosa (`TipoCampo`)**: Formatação automática para decimais de 2 a 10 casas (`De2`, `De3`, `De4`, `De6`, `De10`), datas fiscais (`Dat`, `DatCFe`), horas (`Hor`, `HorCFe`) e fusos horários (`DatHorTz`).
- 📁 **Gerenciamento Inteligente de Arquivos**: Organização automática de XMLs por CNPJ, Modelo, Ano e Mês.
- 🎯 **Multi-Targeting**: Suporte a `.NET Standard 2.0`, `.NET 6.0`, `.NET 7.0`, `.NET 8.0+` e `.NET Framework 4.6.2+`.

---

## 📦 Instalação

Adicione o pacote via CLI do .NET:

```bash
dotnet add package OpenAC.Net.DFe.Core
```

Ou através do Gerenciador de Pacotes do Visual Studio:

```powershell
Install-Package OpenAC.Net.DFe.Core
```

*(O Source Generator já está embutido no pacote. Nenhuma instalação adicional é necessária!)*

---

## 💡 Exemplos de Uso

### 1. Criando um Modelo e Serializando para XML

Basta declarar a classe como `partial` e utilizar as anotações do OpenAC.Net:

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

    [DFeCollection("itens")]
    [DFeItem(typeof(ItemNota), "det")]
    public List<ItemNota> Itens { get; set; } = new();
}

public partial class ItemNota
{
    [DFeAttribute(TipoCampo.Int, "nItem")]
    public int NumeroItem { get; set; }

    [DFeElement(TipoCampo.Str, "cProd", Ocorrencia = Ocorrencia.Obrigatoria)]
    public string CodigoProduto { get; set; } = string.Empty;

    [DFeElement(TipoCampo.De2, "vProd", Ocorrencia = Ocorrencia.Obrigatoria)]
    public decimal ValorProduto { get; set; }
}
```

#### Gerando e Lendo o XML:

```csharp
// 1. Instanciar o modelo
var nfe = new NotaFiscal
{
    Id = "NFe35260100000000000191550010000000011000000011",
    DataEmissao = DateTimeOffset.Now,
    ValorTotal = 150.00m,
    Itens =
    {
        new ItemNota { NumeroItem = 1, CodigoProduto = "PROD001", ValorProduto = 150.00m }
    }
};

// 2. Serializar para string XML (gerado pelo Source Generator em tempo de compilação)
string xml = nfe.WriteToXml();
Console.WriteLine(xml);

// 3. Deserializar a partir de XML
NotaFiscal documento = NotaFiscal.Load(xml);
Console.WriteLine($"Carregado: {documento.Id} - Total: {documento.ValorTotal:C2}");
```

---

### 2. Assinando Digitalmente um Documento (XML-DSig)

Para documentos que exigem assinatura digital, herde de `DFeSignDocument<T>`:

```csharp
using System.Security.Cryptography.X509Certificates;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.DFe.Core.Document;

public partial class MinhaNFeAssinada : DFeSignDocument<MinhaNFeAssinada>
{
    // Propriedades do documento...
}

// Assinando o XML:
var cert = new X509Certificate2("certificado.pfx", "senha123");
minhaNFe.Assinar(cert, TipoAssinatura.RsaSha1, "infNFe", "NFe");

// Obtendo o XML assinado completo com a tag <Signature>
string xmlAssinado = minhaNFe.WriteToXml();
```

---

### 3. Validando XML contra Schemas XSD

```csharp
using System.Xml.Schema;
using OpenAC.Net.DFe.Core;

var validador = new XmlSchemaValidation();
validador.AddSchema("http://www.portalfiscal.inf.br/nfe", @"C:\Schemas\nfe_v4.00.xsd");

bool valido = validador.Validar(xmlString, out string erros);

if (!valido)
{
    Console.WriteLine($"Erros de validação:\n{erros}");
}
```

---

## 🏛️ Estrutura da Solução

```text
OpenAC.Net.DFe.Core/
├── src/
│   ├── OpenAC.Net.DFe.Core/            # Biblioteca principal (Modelos, Assinatura, Serviços)
│   ├── OpenAC.Net.DFe.Generator/       # Roslyn Incremental Source Generator
│   ├── OpenAC.Net.DFe.Generator.Sample/# Projeto de demonstração
│   └── OpenAC.Net.DFe.Core.Tests/      # Suíte unificada de testes unitários (TUnit)
```

---

## 🧪 Executando os Testes

Os testes unitários cobrem todos os tipos de campos, coleções, dicionários, CDATA, enums, interfaces polimórficas e o próprio Source Generator.

Para executar os testes via linha de comando:

```bash
dotnet test
```

---

## 🤝 Como Contribuir

Contribuições da comunidade são muito bem-vindas!

1. Faça um **Fork** do repositório.
2. Crie uma **Branch** para sua funcionalidade ou correção:
   ```bash
   git checkout -b feature/minha-melhoria
   ```
3. Garanta que todos os testes passem:
   ```bash
   dotnet test
   ```
4. Envie seus commits com mensagens descritivas:
   ```bash
   git commit -m "feat: adiciona suporte ao novo campo"
   ```
5. Abra um **Pull Request**.

---

## 💬 Comunidade

Participe da comunidade de desenvolvedores do OpenAC .Net:

- 💬 **Discord**: [Participe do nosso servidor](https://discord.com/invite/brdmJ7Yv6w)
- 🌐 **Portal Oficial**: [openac.net.br](https://openac.net.br/)

---

## 📄 Licença

Este projeto é distribuído sob a licença **MIT**. Consulte o arquivo [LICENSE](LICENSE) para mais detalhes.
