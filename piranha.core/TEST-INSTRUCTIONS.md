# 🧪 Como Verificar se o Módulo Editorial Workflow Funciona

## 📋 Pré-requisitos

1. **.NET 8.0 SDK** instalado
2. **Piranha CMS** como base
3. **Entity Framework Core** configurado

## 🚀 Passos para Testar

### 1. Compilar o Projeto

```bash
cd /Users/andreoliveira/Documents/GitHub/Projeto-AS-2/piranha.core
dotnet build
```

**Resultado esperado:**
- ✅ Compilação sem erros
- ✅ Todos os projetos de Editorial Workflow compilam correctamente

### 2. Executar os Testes Unitários

```bash
dotnet test test/Piranha.EditorialWorkflow.Tests/ --verbosity normal
```

**Resultado esperado:**
- ✅ Todos os testes passam
- ✅ `WorkflowDefinitionRepositoryTests` executa com sucesso
- ✅ `WorkflowIntegrationTests` cria o workflow completo do JSON

### 3. Executar o Exemplo Prático

```bash
dotnet run --project examples/EditorialWorkflowExample/
```

**Resultado esperado:**
```
🚀 Piranha Editorial Workflow Example
=====================================
✅ Services configured successfully
🔧 Creating workflow from JSON...
✅ Created workflow: Standard Editorial Workflow
✅ Created state: Draft (draft)
✅ Created state: Review (review)
✅ Created state: Approved (approved)
✅ Created state: Published (published)
✅ Created transition: draft -> review
✅ Created transition: review -> draft
✅ Created transition: review -> approved
✅ Created transition: approved -> published
🔍 Testing workflow retrieval...
✅ Retrieved workflow: Standard Editorial Workflow
   - States: 4
   - Initial state: Draft
   - Published state: Published
📝 Testing workflow instance creation...
✅ Created workflow instance for content: Test Article
✅ Retrieved instance: Test Article
   - Status: Active
   - Current State ID: [GUID]
🔄 Testing state transitions...
✅ Found 1 available transitions from draft state
   - Can transition to state ID: [GUID]
   - Allowed roles: ["Editor","Admin"]

🎉 All tests completed successfully!
📋 Summary:
   - Workflow created: Standard Editorial Workflow
   - States created: 4
   - Transitions created: 4
   - Instance created for content: Test Article
```

## 🔍 Testes de Funcionalidade

### Teste 1: Verificar Modelos de Domínio
```csharp
// Os seguintes modelos devem estar disponíveis:
- WorkflowDefinition
- WorkflowState  
- TransitionRule
- WorkflowInstance
- WorkflowContentExtension
```

### Teste 2: Verificar Repositórios
```csharp
// Os seguintes repositórios devem funcionar:
- IWorkflowDefinitionRepository
- IWorkflowStateRepository
- ITransitionRuleRepository
- IWorkflowInstanceRepository
- IWorkflowContentExtensionRepository
```

### Teste 3: Verificar Integração com Base de Dados
```csharp
// Deve criar as seguintes tabelas:
- Piranha_WorkflowDefinitions
- Piranha_WorkflowStates
- Piranha_TransitionRules
- Piranha_WorkflowInstances
- Piranha_WorkflowContentExtensions
```

## 🛠 Como Integrar num Projeto Existente

### 1. Adicionar Referências ao Projeto

```xml
<ProjectReference Include="path/to/Piranha.EditorialWorkflow/Piranha.EditorialWorkflow.csproj" />
<ProjectReference Include="path/to/Piranha.Data.EF.EditorialWorkflow/Piranha.Data.EF.EditorialWorkflow.csproj" />
```

### 2. Configurar o DbContext

```csharp
public class MyPiranhaDb : Db<MyPiranhaDb>, IEditorialWorkflowDb
{
    // Editorial Workflow DbSets
    public DbSet<Data.EditorialWorkflow.WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<Data.EditorialWorkflow.WorkflowState> WorkflowStates { get; set; }
    public DbSet<Data.EditorialWorkflow.TransitionRule> TransitionRules { get; set; }
    public DbSet<Data.EditorialWorkflow.WorkflowInstance> WorkflowInstances { get; set; }
    public DbSet<Data.EditorialWorkflow.WorkflowContentExtension> WorkflowContentExtensions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureEditorialWorkflow();
    }
}
```

### 3. Registar Serviços

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ... configuração existente do Piranha
    
    // Adicionar Editorial Workflow
    services.AddEditorialWorkflowRepositories();
}
```

### 4. Executar Migrações

```bash
dotnet ef migrations add AddEditorialWorkflow
dotnet ef database update
```

## ✅ Indicadores de Sucesso

### Build
- [x] `Piranha.EditorialWorkflow` compila sem erros
- [x] `Piranha.Data.EF.EditorialWorkflow` compila sem erros
- [x] Todos os testes unitários passam

### Funcionalidade
- [x] Consegue criar workflows a partir do JSON fornecido
- [x] Consegue guardar e recuperar definições de workflow
- [x] Consegue criar instâncias de workflow para conteúdo
- [x] Consegue verificar transições disponíveis baseadas em roles

### Base de Dados
- [x] Cria todas as tabelas necessárias
- [x] Foreign keys funcionam correctamente
- [x] Índices são criados para performance

## 🐛 Possíveis Problemas e Soluções

### Erro: "Type or namespace 'Models' does not exist"
**Solução:** Verificar se todas as referências a modelos usam o namespace completo `Piranha.EditorialWorkflow.Models`

### Erro: "Cannot resolve DbSet"
**Solução:** Verificar se o DbContext implementa `IEditorialWorkflowDb` e se `ConfigureEditorialWorkflow()` é chamado

### Erro: "Migration fails"
**Solução:** Verificar se não há conflitos de nomes de tabelas com outros módulos

## 📊 Métricas de Teste

Quando tudo funcionar correctamente, deves ver:
- **0 erros de compilação**
- **100% de testes a passar**
- **5 tabelas criadas** na base de dados
- **JSON do exemplo** convertido correctamente em workflow funcional

## 🎯 Próximos Passos

Uma vez verificado que a fundação funciona:

1. **Serviços de Negócio**: Implementar lógica de transições de estado
2. **API Controllers**: Criar endpoints RESTful
3. **Manager UI**: Interface administrativa
4. **Integração com Conteúdo**: Hooks no processo de save/publish
5. **Notificações**: Sistema de alertas para mudanças de estado
