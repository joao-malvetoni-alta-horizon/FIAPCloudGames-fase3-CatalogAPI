# FIAPCloudGames — Catalog API

API RESTful responsável pelo catálogo de jogos da plataforma FIAPCloudGames, desenvolvida como parte da Fase 2 do projeto acadêmico. Segue os princípios de **Clean Architecture** com separação em camadas: Domain, Application, Infrastructure e API.

---

## Tecnologias

| Camada | Tecnologias |
|---|---|
| API | .NET 10, ASP.NET Core Minimal APIs, Swagger (Swashbuckle) |
| Application | MediatR (CQRS), Flunt (notificações/validação) |
| Infrastructure | Entity Framework Core 10, PostgreSQL 18 (Npgsql), FiapCloudGames.RabbitMq |
| Domain | C# puro, sem dependências externas |
| Testes | xUnit, NSubstitute, Testcontainers, coverlet |
| Infraestrutura | Docker, Docker Compose, RabbitMQ 4 |

---

## Arquitetura

O domínio é organizado em **bounded contexts** (`Contexts/`): **Games** (catálogo de jogos) e **Libraries** (biblioteca de jogos adquiridos por usuário).

```
src/
├── CatalogAPI.Domain/          # Contexts/{Games,Libraries}: Entidades, Value Objects, Enums, Exceções, Interfaces, Eventos
├── CatalogAPI.Application/     # Contexts/{Games,Libraries}: Use Cases (Commands/Queries via MediatR), DTOs; Shared/Messaging
├── CatalogAPI.Infrastructure/  # DbContext, Repositórios, Messaging, Migrations (EF Core + PostgreSQL)
└── CatalogAPI.API/             # Endpoints (Minimal API), DI, Configuração (Swagger, JWT, tratamento de erros global)

test/
├── CatalogAPI.Domain.Tests/         # Testes unitários — Domain
├── CatalogAPI.Application.Tests/    # Testes unitários — Application
├── CatalogAPI.Infrastructure.Tests/ # Testes de integração — Repositórios (Testcontainers)
└── CatalogAPI.API.Tests/            # Testes de integração — Endpoints (Testcontainers)
```

---

## Endpoints

### Games — `/api/v1/games`

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/` | Cria um novo jogo |
| `GET` | `/` | Lista jogos com paginação |
| `GET` | `/{id}` | Busca jogo por ID |
| `PUT` | `/{id}` | Atualiza dados de um jogo |
| `DELETE` | `/{id}` | Remove um jogo |

### Library — `/api/v1/library`

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/add` | Inicia a aquisição de um jogo para a biblioteca do usuário autenticado |
| `GET` | `/` | Lista os jogos da biblioteca do usuário autenticado (paginado) |

Apenas os endpoints de **Library** exigem autenticação (`RequireAuthorization`). O `UserId` é extraído da claim `sub` (ou `NameIdentifier`) do token.

> ⚠️ **Atenção:** os endpoints de **Games** (`POST`, `PUT`, `DELETE`, `GET`) atualmente **não exigem autenticação nem autorização** — qualquer chamador pode criar, editar e remover jogos. O domínio prevê uma `InsufficientGameManagementPermissionException` (sugerindo gestão restrita a administradores), mas essa checagem ainda **não está aplicada nos endpoints**. Tratar antes de ir para produção.

- **`POST /add`** — o corpo informa apenas o `GameId`. A aquisição é **assíncrona**: validado o jogo e a posse, um evento `OrderPlacedEvent` é publicado no message bus e a API responde `202 Accepted`. A confirmação da posse ocorre depois, de forma assíncrona, pelo consumer (ver _Mensageria e Eventos_).
- **`GET /`** — retorna a biblioteca do usuário paginada via query string `?page=&pageSize=` (`Page` padrão 1, `PageSize` padrão 20, **máximo 50**). A resposta `200 OK` traz os jogos (`GameId`, `Title`, `Genre`, `Price`) e os metadados de paginação (`Page`, `PageSize`, `Total`).

A documentação interativa via Swagger está disponível em `/swagger` no ambiente de desenvolvimento.

---

## Autenticação

A API usa **JWT Bearer**, validado **offline** (sem chamada HTTP à UsersAPI). O token é emitido pela **UsersAPI** e verificado aqui com uma chave simétrica compartilhada (HMAC). As claims do token (em especial `sub`) identificam o usuário nas operações de biblioteca.

Parâmetros de validação (`BuilderExtension.AddBearerAuthentication`):

| Validação | Ativa? |
|---|---|
| Assinatura (`ValidateIssuerSigningKey`) | ✅ Sim — HMAC com a `SecretKey` compartilhada |
| Expiração (`ValidateLifetime`) | ✅ Sim |
| Issuer (`ValidateIssuer`) | ❌ Não |
| Audience (`ValidateAudience`) | ❌ Não |

`MapInboundClaims = false` (os nomes das claims não são remapeados) e `RoleClaimType = ClaimTypes.Role`.

> A `SecretKey` **precisa ser exatamente a mesma configurada na UsersAPI** (emissora do token), pois a assinatura é verificada localmente. É lida de `JwtSettings:SecretKey` e é **obrigatória**: a aplicação lança exceção no startup se ela não estiver configurada (ver _Configuração_).

---

## Mensageria e Eventos

A aquisição de um jogo é uma **saga assíncrona** coordenada por dois eventos de integração, cujos
contratos e nomes de exchange/routing key vêm do pacote compartilhado **`FiapCloudGames.Contracts`**:

1. **`OrderPlacedEvent (UserId, GameId, Price)`** — publicado pela API ao iniciar a compra (`POST /add`), após validar que o jogo existe e que o usuário ainda não o possui. É publicado na exchange `catalog.exchange` (topic) com a routing key `order.placed` (`CatalogMessaging`). A PaymentsAPI consome esse evento.
2. **`PaymentProcessedEvent (UserId, GameId, PaymentStatus)`** — publicado de volta pela PaymentsAPI na exchange `payments.exchange` com a routing key `payment.status` (`PaymentsMessaging`). `PaymentStatus` pode ser `Approved` ou `Rejected`.

Toda a integração com o broker é feita pelo pacote **`FiapCloudGames.RabbitMq`** (o mesmo usado pelas
demais APIs da plataforma), registrado via `AddRabbitMq(configuration)`. A conexão é configurada pela
seção **`RabbitMq`** (`Host`, `Port`, `Username`, `Password`, `VirtualHost`) — ver _Configuração do RabbitMQ_.

### Publicação

A camada de Application define a abstração `IIntegrationEventPublisher` (`Shared/Messaging`), cuja
implementação `RabbitMqIntegrationEventPublisher` (Infrastructure) adapta o `IRabbitMqPublisher` do
pacote. O handler de `InitiateGamePurchase` publica o `OrderPlacedEvent` informando exchange e routing
key a partir das constantes de `CatalogMessaging` — sem _magic strings_.

### Consumo

O consumo é feito por um `IMessageProcessor` registrado com
`AddRabbitMqConsumer<PaymentProcessedMessageProcessor>(...)`. O pacote sobe um `BackgroundService`
(hosted service) que declara a topologia (exchange/fila/binding) e consome a fila `catalog.payment-processed`
(consumo manual, `autoAck: false`) — **não expõe endpoint HTTP**. A responsabilidade é separada em três camadas:

1. **`PaymentProcessedMessageProcessor`** (Infrastructure) — _fino_, sem lógica de aplicação: desserializa o `PaymentProcessedEvent`, delega ao `IEventDispatcher` e traduz o resultado para um `MessageProcessingResult`.
2. **`IEventDispatcher` / `EventDispatcher`** — abstração (Application) e implementação (Infrastructure) que resolvem o `IEventHandler<TEvent>` correspondente num **escopo de DI por mensagem** — único ponto que conhece o contêiner.
3. **`PaymentProcessedEventHandler : IEventHandler<PaymentProcessedEvent>`** (Application) — concentra a **lógica de aplicação**: se o pagamento foi `Approved`, adiciona o jogo à biblioteca (via repositório de domínio `IGamePurchase`, idempotente); caso contrário, nada faz.

O mapeamento do desfecho do despacho para a política de reentrega do broker fica no processor:

- **`Success`** (ack) — o handler concluiu (pagamento `Approved` → jogo adicionado à biblioteca, ou `Rejected` → nada a fazer). A adição é **idempotente**: se o usuário já possui o jogo, nada é inserido;
- **`PoisonMessage`** (nack sem _requeue_) — mensagem malformada/vazia, jogo inexistente no catálogo (`GameNotFoundException`) ou reentrega duplicada (violação de unicidade `(UserId, GameId)`): descartada;
- **`TransientFailure`** (nack com _requeue_) — falha passageira (ex.: erro de banco de dados): reenfileirada para nova tentativa.

Essa estrutura espelha o padrão da NotificationsAPI: o processor de mensageria fica livre de regra de negócio, que vive em handlers de evento testáveis na camada de Application.

### Serialização das mensagens

Os eventos trafegam em **JSON** no barramento. O `PaymentProcessedMessageProcessor` desserializa a
mensagem com opções que **precisam casar com o formato gravado pelo publisher** (PaymentsAPI):

```csharp
new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    Converters = { new JsonStringEnumConverter() },
}
```

- `JsonSerializerDefaults.Web` → nomes de propriedade **case-insensitive** (aceita `camelCase` e `PascalCase`);
- `JsonStringEnumConverter` → enums aceitos tanto como **string** (`"Approved"`) quanto como **número** (`1`).

> ⚠️ Usar as opções **padrão** do `System.Text.Json` aqui causa um bug silencioso: com `PropertyNameCaseInsensitive = false`, propriedades em `camelCase` não fazem match com os parâmetros do record posicional (`UserId`/`GameId` viram `Guid.Empty`) e enums como string estouram `JsonException`. Como `PaymentStatus` não tem valor `0`, um `Status` não desserializado cai no _early-return_ do handler e **nada é persistido**. Mantenha as opções acima alinhadas com o publisher.

---

## Domain

### Entidade `Game`

| Campo | Tipo | Regras |
|---|---|---|
| `Id` | `Guid` | Gerado automaticamente |
| `Title` | `GameTitle` | Obrigatório, máximo 200 caracteres |
| `Description` | `string` | Máximo 2000 caracteres |
| `Price` | `Price` | Valor não negativo |
| `Genre` | `GameGenre` | Action, RPG, Strategy, Sports, Puzzle, Other |
| `Status` | `GameStatus` | Active, Inactive, ComingSoon |
| `ReleaseDate` | `DateOnly` | Deve ser a data atual ou futura |
| `CreatedAt` | `DateTime` | Gerado automaticamente |
| `UpdatedAt` | `DateTime?` | Atualizado a cada modificação |

### Value Objects

- **`GameTitle`** — valida obrigatoriedade e limite de 200 caracteres; aplica trim automático.
- **`Price`** — valida que o valor não seja negativo.

### Entidade `LibraryItem`

Representa a posse de um jogo por um usuário (contexto Libraries).

| Campo | Tipo | Regras |
|---|---|---|
| `Id` | `Guid` | Gerado automaticamente |
| `UserId` | `Guid` | Identificador do usuário |
| `GameId` | `Guid` | Identificador do jogo |
| `AcquiredOn` | `DateTime` | Data/hora UTC da aquisição |

A combinação `(UserId, GameId)` é única — um usuário não pode adquirir o mesmo jogo duas vezes.

### Exceções de domínio

As regras de negócio sinalizam falhas com exceções que herdam de `DomainException` (`Shared`). Os
handlers da camada de Application **lançam** essas exceções (ou deixam que as lançadas pelo domínio
propaguem); a tradução para status HTTP é centralizada — ver _Tratamento de Erros_.

| Exceção | Contexto | Quando | Status HTTP |
|---|---|---|---|
| `InvalidGameTitleException` | Games | Título vazio ou acima de 200 caracteres (`GameTitle`) | `400` |
| `InvalidPriceException` | Games | Preço negativo (`Price`) | `400` |
| `InvalidReleaseDateException` | Games | Data de lançamento inválida (`Game`) | `400` |
| `DomainValidationException` | Games | Outras invariantes (ex.: descrição acima de 2000 caracteres) | `400` |
| `GameNotFoundException` | Games | Jogo inexistente no catálogo (get/update/delete/compra) | `404` |
| `GameAlreadyOwnedException` | Libraries | Usuário já possui o jogo ao iniciar a compra | `409` |
| `InsufficientGameManagementPermissionException` | Games | Gestão de jogos sem permissão de admin — **placeholder**, ainda não lançada (ver aviso em _Endpoints_) | `403` |
| `UnauthorizedAccessException` | Libraries | Acesso indevido a recurso de outro usuário — **placeholder**, ainda não lançada | `403` |

> As duas últimas são reservadas para regras de autorização ainda **não implementadas**. Já estão
> mapeadas no handler global, então passam a valer automaticamente assim que forem lançadas.

---

## Tratamento de Erros

O tratamento de erros é **centralizado** num `IExceptionHandler` global
(`CatalogAPI.API/Configuration/GlobalExceptionHandler.cs`), registrado no `Program.cs` via
`AddExceptionHandler<GlobalExceptionHandler>()` + `AddProblemDetails()` e ativado com
`app.UseExceptionHandler()`.

- **Handlers da Application** não capturam mais exceções para montar respostas de erro: eles apenas
  **lançam** a exceção de domínio adequada (ex.: `throw new GameNotFoundException(...)`) ou retornam
  sucesso. A única resposta de erro montada no handler é a **validação de request** (Flunt/Specification),
  que retorna `400` com a lista de `Notifications`.
- O `GlobalExceptionHandler` mapeia a exceção para o status HTTP (tabela acima) e responde com
  **`ProblemDetails`** (RFC 7807). Erros `5xx` são logados e têm a mensagem interna omitida do corpo;
  exceções de domínio (`4xx`) expõem sua mensagem em `detail`.
- Falhas de infraestrutura de banco (`DbException`) e quaisquer outras exceções não previstas viram
  `500`.

Isso mantém os handlers focados no fluxo de sucesso e garante respostas de erro consistentes em toda a API.

---

## Testes

O projeto possui 188 testes distribuídos em quatro camadas, todos passando. Os contextos **Games** e **Libraries** (aquisição de jogos, processamento de pagamento e consulta da biblioteca) estão cobertos.

| Projeto | Tipo | Testes | Ferramentas |
|---|---|---|---|
| `CatalogAPI.Domain.Tests` | Unitário | 37 | xUnit |
| `CatalogAPI.Application.Tests` | Unitário | 74 | xUnit, NSubstitute |
| `CatalogAPI.Infrastructure.Tests` | Integração/Unitário | 34 | xUnit, NSubstitute, Testcontainers (PostgreSQL) |
| `CatalogAPI.API.Tests` | Integração (E2E) | 43 | xUnit, Testcontainers, WebApplicationFactory |

Os testes de integração sobem um container PostgreSQL efêmero via **Testcontainers** — é necessário ter o Docker rodando.

> Os testes de endpoint não dependem de um broker RabbitMQ: o `ApiFactory` remove os hosted services (incluindo o consumer do `FiapCloudGames.RabbitMq`) e substitui o `IIntegrationEventPublisher` por um stub _no-op_. A lógica de consumo é coberta por testes unitários do `PaymentProcessedEventHandler` (Application) e do `PaymentProcessedMessageProcessor` (mapeamento de falhas para `MessageProcessingResult`, com o `IEventDispatcher` mockado); a persistência em si, pelos testes do repositório `GamePurchase`.

### Executar todos os testes

```bash
dotnet test
```

### Executar por camada

```bash
dotnet test test/CatalogAPI.Domain.Tests/
dotnet test test/CatalogAPI.Application.Tests/
dotnet test test/CatalogAPI.Infrastructure.Tests/
dotnet test test/CatalogAPI.API.Tests/
```

---

## Configuração

Todas as chaves abaixo podem ser definidas no `appsettings.json` ou sobrescritas por **variáveis de
ambiente** (usando `__` como separador de seção — ex.: `RabbitMq__Host`, `ConnectionStrings__DefaultConnection`).

### JWT (`JwtSettings`)

```json
"JwtSettings": {
  "SecretKey": "<chave HMAC compartilhada com a UsersAPI>"
}
```

> **Obrigatório.** A API lança exceção no startup se `JwtSettings:SecretKey` não estiver presente. A chave
> precisa ser **idêntica** à usada pela UsersAPI, pois a assinatura do token é validada offline (ver _Autenticação_).

### RabbitMQ (`RabbitMq`)

O pacote `FiapCloudGames.RabbitMq` lê a seção `RabbitMq` (ou variáveis com o prefixo `RabbitMq__`):

```json
"RabbitMq": {
  "Host": "rabbitmq",
  "Port": 5672,
  "Username": "fcg",
  "Password": "fcg123",
  "VirtualHost": "/"
}
```

No `appsettings.Development.json` o `Host` é `localhost`. No `compose.yaml` esses valores são
sobrescritos por variáveis de ambiente (`RabbitMq__Host=rabbitmq`, `RabbitMq__Username=guest`, etc.).

> ℹ️ As credenciais variam por ambiente: o `appsettings.json` usa `fcg/fcg123` (padrão da plataforma),
> enquanto o `compose.yaml` provisiona os containers de Postgres e RabbitMQ com `postgres/pass` e
> `guest/pass` e injeta essas credenciais na API via variáveis de ambiente. Ao rodar via Docker, valem
> os valores do compose; fora do Docker, valem os do `appsettings`.

---

## Executando

### Com Docker (recomendado)

```bash
docker compose up -d --build

dotnet ef database update --project src/CatalogAPI.Infrastructure --startup-project src/CatalogAPI.API
```

Isso sobe três serviços:

| Serviço | Porta (host:container) |
|---|---|
| CatalogAPI | `5001:8080` |
| PostgreSQL 18 | `5432:5432` |
| RabbitMQ 4 | `5672:5672` / `15672:15672` (management) |

A API ficará disponível em `http://localhost:5001`. O container roda com
`ASPNETCORE_ENVIRONMENT=Development`, então o **Swagger fica ativo** em `http://localhost:5001/swagger`.

---

## Observabilidade (New Relic)

A plataforma de APM gerenciada escolhida para a Fase 3 (**Opção B**) é o **New Relic**. A CatalogAPI
é instrumentada pelo **agente .NET do New Relic**, distribuído pelo pacote NuGet `NewRelic.Agent`
(referenciado apenas em `CatalogAPI.API`, que é o processo que roda). O pacote publica o agente em
`newrelic/` na saída do `dotnet publish`; o profiler do CoreCLR é ativado pelas variáveis
`CORECLR_*` já definidas no **estágio de runtime do `Dockerfile`**, então a imagem sobe instrumentada
sem nenhuma chamada no `Program.cs`.

O pacote `NewRelic.Agent.Api` (referenciado em `CatalogAPI.Application`) expõe a API do agente em
tempo de compilação e é usado apenas para anexar **atributos customizados** ao trace da compra de
jogo — ver _Trace do fluxo "Compra de Jogo"_.

> Nenhuma configuração de log foi trocada: a aplicação continua usando o `Microsoft.Extensions.Logging`
> padrão do ASP.NET Core, que o agente instrumenta automaticamente. Não há sink HTTP nem Serilog.

### Variáveis de ambiente

| Variável | Onde é definida | Valor | Descrição |
|---|---|---|---|
| `CORECLR_ENABLE_PROFILING` | `Dockerfile` | `1` | Habilita o profiler do CoreCLR (sem isso o agente não carrega) |
| `CORECLR_PROFILER` | `Dockerfile` | `{36032161-FFC0-4B61-B559-F6C5D41BAE5A}` | CLSID fixo do profiler do New Relic (case-sensitive) |
| `CORECLR_NEWRELIC_HOME` | `Dockerfile` | `/app/newrelic` | Diretório do agente dentro da imagem |
| `CORECLR_PROFILER_PATH` | `Dockerfile` | `/app/newrelic/libNewRelicProfiler.so` | Biblioteca nativa do profiler (linux-x64) |
| `NEW_RELIC_APP_NAME` | `Dockerfile` | `FCG-CatalogAPI` | Nome da aplicação no New Relic (padrão `FCG-<serviço>`) |
| `NEW_RELIC_DISTRIBUTED_TRACING_ENABLED` | `Dockerfile` | `true` | Liga o trace distribuído (propagação automática sobre HTTP) |
| `NEW_RELIC_APPLICATION_LOGGING_ENABLED` | `Dockerfile` | `true` | Liga a instrumentação de logs |
| `NEW_RELIC_APPLICATION_LOGGING_FORWARDING_ENABLED` | `Dockerfile` | `true` | Encaminha os logs da aplicação para a plataforma |
| `NEW_RELIC_APPLICATION_LOGGING_LOCAL_DECORATING_ENABLED` | `Dockerfile` | `true` | Decora os logs com os metadados de correlação (trace/span) |
| `NEW_RELIC_LICENSE_KEY` | **Secret / ambiente** | _(segredo)_ | License key da conta. **Nunca** vai para o repositório nem para a imagem |

Sem `NEW_RELIC_LICENSE_KEY` a aplicação sobe normalmente — o agente apenas não conecta à plataforma.

### Kubernetes — license key via Secret

Conforme o requisito técnico da Fase 3 (*"as chaves de API devem ser gerenciadas via Kubernetes
Secrets"*), a license key é lida do Secret `fcg-secrets` (chave `NewRelic__LicenseKey`) e injetada em
`k8s/deployment.yaml` — nunca de ConfigMap, nunca literal no manifesto:

```yaml
- name: NEW_RELIC_LICENSE_KEY
  valueFrom: { secretKeyRef: { name: fcg-secrets, key: NewRelic__LicenseKey } }
```

Para criar/atualizar a chave no cluster:

```bash
kubectl -n fcg create secret generic fcg-secrets \
  --from-literal=NewRelic__LicenseKey=<sua-license-key> \
  --dry-run=client -o yaml | kubectl apply -f -
```

> O comando acima cria o Secret apenas com essa chave. Se o `fcg-secrets` já existir com as demais
> chaves (`JwtSettings__SecretKey`, `Catalog__ConnectionString`, `Catalog__RabbitMqConnection`),
> repita-as no mesmo comando ou use `kubectl patch` para não sobrescrever o Secret existente.

### Rodando localmente

Com Docker Compose, a chave é repassada do ambiente do host (ou de um arquivo `.env`, que está no
`.gitignore`). Use o `.env.example` como base:

```bash
cp .env.example .env          # e preencha NEW_RELIC_LICENSE_KEY
docker compose up -d --build
```

Ou exportando a variável direto no shell:

```bash
export NEW_RELIC_LICENSE_KEY=<sua-license-key>
docker compose up -d --build
```

Rodando fora do Docker (`dotnet run`), o agente só carrega se as variáveis `CORECLR_*` apontarem para
a pasta `newrelic/` da saída de build/publish — o mais simples é usar o Compose. Sem elas, a aplicação
roda normalmente, apenas sem instrumentação.

### Os três pilares

| Pilar | Como é atendido | Observação |
|---|---|---|
| **Métricas** | Automático pelo agente APM: latência, throughput e taxa de erro por endpoint, além de métricas de banco (EF Core/Npgsql) | O dashboard é montado na UI do New Relic, fora deste repositório |
| **Logs** | `NEW_RELIC_APPLICATION_LOGGING_*` liga o encaminhamento e a decoração automáticos do `Microsoft.Extensions.Logging` | Sem alteração no código de log da aplicação |
| **Traces** | Trace distribuído ligado; propagação automática **sobre HTTP** | A propagação **não** atravessa o RabbitMQ — ver a limitação abaixo |

### Trace do fluxo "Compra de Jogo"

A CatalogAPI é o ponto de entrada do fluxo: `POST /api/v1/library/add` valida o jogo e a posse e
publica o `OrderPlacedEvent`, consumido pela PaymentsAPI, que devolve o `PaymentProcessedEvent`
(ver _Mensageria e Eventos_). Para o trace ficar navegável, o handler de `InitiateGamePurchase`
anexa atributos customizados à transação:

| Atributo | Valor |
|---|---|
| `fcg.flow` | `compra-jogo` |
| `fcg.userId` | `Guid` do usuário (identificador opaco) |
| `fcg.gameId` | `Guid` do jogo |
| `fcg.orderPlacedEventId` | `Guid` do `OrderPlacedEvent` publicado |

São apenas identificadores opacos — **nenhum dado sensível** (e-mail, senha, token ou CPF) é enviado
para a plataforma.

### Limitação conhecida: o trace distribuído não atravessa o RabbitMQ

O agente New Relic **10.54.0** instrumenta o RabbitMQ apenas até a versão **6.8.1** do cliente. A
instrumentação (`NewRelic.Providers.Wrapper.RabbitMq.Instrumentation.xml`) casa com
`maxVersion="6.8.1"` nos tipos `RabbitMQ.Client.Framing.Impl.Model` e
`RabbitMQ.Client.Events.EventingBasicConsumer`, que **não existem mais** na API 7.x.

Este repositório resolve **`RabbitMQ.Client` 7.2.1**, trazido transitivamente por
`FiapCloudGames.RabbitMq` 1.0.0. Consequências:

- a publicação e o consumo de mensagens **não geram spans** de mensageria no New Relic;
- o `traceparent` **não é propagado** pela fila, então o trace da CatalogAPI e o da PaymentsAPI
  aparecem como **traces separados**, e não como um único trace ponta-a-ponta;
- o segundo trecho da compra (consumo do `PaymentProcessedEvent` pelo `BackgroundService`) roda fora
  de uma transação do APM — a evidência dele vem dos **logs**, que continuam sendo encaminhados.

A correlação entre os dois lados é feita **manualmente**, pelos atributos `fcg.gameId` / `fcg.userId`
e pelo `fcg.orderPlacedEventId`, que também aparecem nos logs do consumidor.

Como este MR é de observabilidade, o wrapper de mensageria **não foi reescrito** para contornar isso.
Caminhos possíveis, se a limitação precisar ser resolvida no futuro:

1. usar a ponte de OpenTelemetry do agente (`NEW_RELIC_OPENTELEMETRY_ENABLED=true`, desligada por
   padrão) junto com o `RabbitMQActivitySource` do cliente 7.x — que exige atribuir explicitamente
   `RabbitMQActivitySource.ContextInjector = RabbitMQActivitySource.DefaultContextInjector` (e o
   `ContextExtractor` correspondente) no pacote `FiapCloudGames.RabbitMq`, já que por padrão ele
   **não** injeta o `traceparent` nos headers da mensagem;
2. propagar o payload do trace manualmente com a API do agente
   (`CreateDistributedTracePayload` / `AcceptDistributedTraceHeaders`) no publisher e no processor.

Ambos alteram o pacote compartilhado de mensageria e ficam fora do escopo deste MR.
