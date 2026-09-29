# Financial Engine — Motor de Saldos Bancários

Solução completa (backend + frontend) para o teste técnico de Desenvolvedor de Software Fullstack Pleno. O sistema recebe eventos de crédito/débito, atualiza o saldo consolidado de contas bancárias de forma idempotente e transacional, e expõe uma interface Angular para consulta e lançamento dessas movimentações.

## Stack

**Backend**
- C# / ASP.NET Core Web API
- Entity Framework Core + PostgreSQL
- xUnit, Moq, FluentAssertions

**Frontend**
- Angular (standalone components, roteamento com lazy loading)
- RxJS
- Angular Material
- Vitest (test runner padrão do Angular CLI neste projeto)

**Infraestrutura**
- Docker e Docker Compose

## Como rodar o projeto

Pré-requisitos: Docker e Docker Compose instalados.

Na raiz do repositório:

```bash
docker compose up --build
```

Isso sobe três serviços:

| Serviço  | URL                              |
|----------|-----------------------------------|
| Frontend | http://localhost:4200            |
| API      | http://localhost:5000/swagger    |
| Postgres | localhost:5432                   |

Ao subir, a API aplica as migrations automaticamente e popula o banco com três contas de exemplo (seed), incluindo a conta `7b895f64-5717-4562-b3fc-2c963f66afa7` usada no payload de exemplo do enunciado.

Para reiniciar do zero (apaga os dados do Postgres):

```bash
docker compose down -v
docker compose up --build
```

## Como rodar os testes

**Backend** (a partir de `backend/`):
```bash
dotnet test
```

**Frontend** (a partir de `frontend/`):
```bash
npm test
```

## Arquitetura

### Backend

O projeto mantém as camadas em um único projeto ASP.NET Core (`Controllers → Services → Repositories → Data/Models`), em vez de separar em múltiplos projetos (Domain/Application/Infrastructure). **Trade-off assumido:** uma separação em camadas físicas (projetos distintos) daria mais isolamento e reforçaria regras de dependência via referências de projeto, mas adiciona cerimônia desproporcional ao escopo do teste. As responsabilidades continuam separadas logicamente:

- **Controllers**: apenas recebem a requisição HTTP, delegam para o Service e traduzem o resultado em uma resposta HTTP.
- **Services**: orquestram o caso de uso (idempotência, transação de banco, chamada aos repositórios).
- **Repositories**: abstraem o acesso a dados via interfaces (`IAccountRepository`, `ITransactionRepository`), injetadas por DI — o que permite testar o `TransactionService` com mocks, sem tocar em banco real.
- **Models**: a entidade `Account` concentra a regra de negócio do domínio financeiro (método `Apply`), em vez de ser uma classe anêmica manipulada de fora. O setter de `Balance` é privado — o saldo só muda através da regra que garante que ele nunca fica negativo.

### Frontend

Estrutura em `core/` (services e models compartilhados) e `features/` (um componente standalone por tela: listagem de contas, extrato, formulário de lançamento), com lazy loading por rota. Os componentes não fazem chamadas HTTP diretamente — sempre passam pelos services (`AccountService`, `TransactionService`), que devolvem `Observable`s tipados a partir dos DTOs da API.

## Como as quatro regras de negócio obrigatórias foram garantidas

1. **Idempotência**: `EventId` é a chave primária da tabela `transactions`. Antes de processar, o serviço faz uma checagem rápida (`ExistsAsync`) para devolver erro cedo no caminho feliz; mas a garantia real contra condições de corrida é a constraint de chave primária do próprio Postgres — se duas requisições com o mesmo `eventId` chegarem simultaneamente, uma delas recebe violação de unicidade do banco, capturada e traduzida para `409 Conflict`.

2. **Consistência**: a regra "saldo não pode ficar negativo" vive dentro da entidade `Account` (método `Apply`), não espalhada pelo Service ou pelo Controller. Além disso, há uma *check constraint* no banco (`Balance >= 0`) como segunda linha de defesa, caso qualquer código futuro tente gravar direto na tabela sem passar pela entidade.

3. **Transacionalidade**: a gravação da transação e a atualização do saldo da conta ocorrem dentro de uma transação explícita do EF Core (`BeginTransactionAsync` / `CommitAsync` / `RollbackAsync`). Isso é tecnicamente redundante com o comportamento padrão de `SaveChangesAsync` (que já agrupa múltiplas alterações num único commit atômico), mas foi mantido explícito de propósito: deixa a exigência de atomicidade visível no código e facilita adicionar novas operações ao mesmo escopo transacional no futuro (ex.: publicar em uma fila) sem refatoração.

4. **Integridade ponta a ponta**: o frontend valida o formulário (valor positivo, campo obrigatório) para dar feedback imediato, mas cada resposta de erro da API (`404`, `409`, `422`) é tratada explicitamente no `TransactionFormComponent` e exibida ao usuário — o frontend nunca assume que uma operação teve sucesso sem confirmação do backend.

## Concorrência

A entidade `Account` usa a coluna interna `xmin` do Postgres como token de concorrência otimista (mapeado via `IsRowVersion()`). Se duas requisições tentarem alterar a mesma conta simultaneamente, o EF Core detecta o conflito no `SaveChangesAsync` e lança `DbUpdateConcurrencyException`, traduzida pelo middleware global de erros em `409 Conflict`.

## Tratamento de erros

Um middleware global (`ExceptionHandlingMiddleware`) captura as exceções de domínio e as traduz em respostas HTTP consistentes, no formato `application/problem+json`:

| Exceção                        | Status HTTP |
|---------------------------------|-------------|
| `DuplicateEventException`       | 409         |
| `InsufficientBalanceException`  | 422         |
| `AccountNotFoundException`      | 404         |
| `ArgumentOutOfRangeException`   | 400         |
| Conflito de concorrência        | 409         |
| Não tratada                     | 500         |

## O que não foi implementado

Os diferenciais opcionais do enunciado (NgRx, Keycloak/OIDC, RabbitMQ, Redis, Elasticsearch/Health Checks) não foram implementados, para priorizar a robustez dos quatro pilares obrigatórios e a qualidade dos testes dentro do prazo. Dado mais tempo, o próximo diferencial que eu priorizaria seria **Health Checks** (baixo custo de implementação, alto valor operacional), seguido de **RabbitMQ** para desacoplar o processamento do saldo da resposta HTTP.

## Endpoints principais

| Método | Rota                                          | Descrição                          |
|--------|------------------------------------------------|--------------------------------------|
| GET    | `/api/accounts`                                | Lista contas com saldo consolidado  |
| GET    | `/api/accounts/{accountId}/transactions`       | Extrato paginado (`page`, `pageSize`) |
| POST   | `/api/transactions`                            | Processa um novo evento de crédito/débito |

Documentação interativa completa disponível em `/swagger` com a API rodando.
