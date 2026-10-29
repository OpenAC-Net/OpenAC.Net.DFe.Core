# HTTP e armazenamento compartilhados

O DFe.Core contém os contratos e a implementação comum para REST, SOAP e multipart:

- `IDFeHttpTransport`, `DFeHttpTransport` e `DFeHttpTransportOptions`, em `OpenAC.Net.DFe.Core.Http`.
- `IDFeDocumentStore`, `DFeFileSystemDocumentStore`, `DFeDocument` e `DFeTipoDocumento`, em `OpenAC.Net.DFe.Core.Storage`.

As configurações derivadas de `DFeConfigBase<...>` herdam `HttpTransport`, `DocumentStore` e `OperationToken`. Aplicações desktop recebem implementações padrão; hosts podem substituir esses serviços sem alterar os provedores fiscais.

## Envio HTTP

```csharp
using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
var options = new DFeHttpTransportOptions(config.WebServices, certificado, validarCertificadoServidor);
using var response = await config.HttpTransport.SendAsync(request, options, cancellationToken);
```

O chamador possui a requisição e a resposta. O certificado informado fica emprestado durante o envio; o pool cria e possui sua própria cópia. As conexões são isoladas por certificado, TLS, proxy, validação do servidor e host. Cookies não são compartilhados e não há alteração de `ServicePointManager.SecurityProtocol`.

`Tentativas` inclui o primeiro envio. Somente GET, HEAD, OPTIONS e TRACE repetem falhas de conexão ou respostas 408, 429 e 5xx. POST, PUT, PATCH e DELETE são enviados uma vez, evitando repetir emissões e eventos fiscais. A última resposta, inclusive de erro, é devolvida para o componente interpretar. Cancelamento interrompe o envio e a espera entre tentativas.

`WebServices.TimeOut` aceita um `TimeSpan` explícito; `null` mantém o cálculo anterior por `AjustaAguardaConsultaRet`, com padrão HTTP de 100 segundos. O transporte aplica o limite à operação inteira, incluindo retentativas e leitura da resposta.

Os clientes existentes `DFeServiceClientBase`, `DFeRestServiceClient` e `DFeSoapServiceClient` usam esse transporte. A base também oferece `ExecuteAsync` para texto e `HttpContent`. Os métodos síncronos continuam disponíveis. Em erro HTTP, a base preenche `StatusCode` e `EnvelopeRetorno`, persiste a resposta quando habilitado e lança `OpenDFeCommunicationException`. Cancelamentos continuam sendo `OperationCanceledException`.

Overrides existentes de `GravarSoap` continuam sendo chamados. O hook `GravarSoapAsync` permite que componentes implementem persistência inteiramente assíncrona; seu padrão chama o hook síncrono para manter esse comportamento.

## Documentos e diretórios

```csharp
await config.DocumentStore.SaveAsync(
    new DFeDocument(DFeTipoDocumento.DocumentoFiscal, xml, "dps.xml", cnpj, data),
    config.Arquivos, cancellationToken);
```

O store resolve o diretório com `DFeArquivosConfigBase.GetDocumentPath`. O padrão respeita `PathSalvar`, contribuinte, data, literal e ordenação configurados. O enum contém `SolicitacaoTecnica`, `RespostaTecnica`, `Solicitacao`, `Resposta` e `DocumentoFiscal`. A distinção entre modelos fiscais e suas convenções de diretório pertence ao componente. Hosts podem chamar `DFeFileSystemDocumentStore.SaveToDirectoryAsync` com um diretório já resolvido, inclusive com isolamento por empresa.

A gravação usa UTF-8 sem BOM e `FileMode.CreateNew`. Colisões acrescentam `_1`, `_2`, etc.; `PreservarNomeArquivo = true` faz a colisão gerar erro sem sobrescrever. Somente colisões de arquivos existentes são repetidas. Erros de I/O são propagados. Nomes com diretórios são rejeitados e cancelamentos são verificados antes da criação e durante a escrita.

O store persiste documentos explicitamente solicitados. As decisões de salvar documentos fiscais e mensagens técnicas pertencem ao componente. Os clientes do Core respeitam `WebServices.Salvar` para envelopes.

## Adoção em NFSe e NFSe.Nacional

Esta alteração concentra a infraestrutura no Core. Os repositórios consumidores ainda usam seus contratos locais e devem adotar a nova API em uma atualização coordenada:

1. Atualizar a referência ao DFe.Core para uma versão contendo esta API, ou usar `ProjectReference` durante o desenvolvimento.
2. Substituir `INFSeHttpTransport` por `IDFeHttpTransport`, construir `DFeHttpTransportOptions` na operação e usar `DFeHttpTransport.Instance` como padrão. Manter montagem de envelopes, autenticação e interpretação fiscal nos provedores.
3. Substituir os tipos locais de storage pelos tipos `DFe`, passando `config.Arquivos` ao store. Mapear RPS, DPS e NFSe para `DocumentoFiscal`, envelopes SOAP para `Solicitacao`/`Resposta` e payloads técnicos para `SolicitacaoTecnica`/`RespostaTecnica`, sem converter os valores numéricos das enumerações antigas.
4. Preservar as regras de diretórios dos componentes em `GetDocumentPath` ou em um store próprio. Como `DocumentoFiscal` reúne os modelos fiscais, o componente deve resolver seus diretórios específicos usando suas convenções de documentos e nomes de arquivo.
5. Remover as propriedades locais de transporte, store, token e proxy que passam a ser herdadas do Core. No NFSe, `ConfigWebServicesNFSe.Proxy` deve usar a propriedade herdada para que as opções HTTP recebam o valor correto.
6. Adaptar os registros web de DI e seus stores aos contratos compartilhados. Manter identidade da empresa, `IHttpClientFactory`, logs e políticas do host nos adaptadores web. Aplicar retentativas em uma única camada.
7. Remover os pools e algoritmos de gravação duplicados após validar os consumidores. Testar emissão, consulta, evento e armazenamento com suas configurações desktop e web.

As regras de empresa e de diretório fiscal não exigem dependências de ASP.NET Core no pacote comum. No .NET Framework, a seleção de protocolos usa o setter disponível no runtime, por reflexão quando ele não está exposto nas referências antigas.
