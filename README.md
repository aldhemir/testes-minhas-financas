# 🧪 Testes Automatizados - Minhas Finanças

Suíte completa de testes automatizados para o sistema Minhas Finanças, implementando uma pirâmide de testes com cobertura de regras de negócio críticas.

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Pirâmide de Testes](#pirâmide-de-testes)
- [Como Executar](#como-executar)
- [Regras de Negócio Testadas](#regras-de-negócio-testadas)
- [Bugs Encontrados](#bugs-encontrados)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Justificativa das Escolhas](#justificativa-das-escolhas)

## 🎯 Visão Geral

Este repositório contém **APENAS OS TESTES** para a aplicação Minhas Finanças. O código da aplicação original não foi incluído, conforme requisitos do teste técnico.

### Cobertura de Testes

- ✅ **Testes Unitários**: Validação de entidades e lógica de negócio
- ✅ **Testes de Integração**: Validação de APIs e fluxos completos
- ✅ **Testes E2E**: Validação de experiência do usuário
- ✅ **CI/CD**: Pipeline GitHub Actions (opcional)

### Regras de Negócio Focadas

1. **Menor de idade não pode ter receitas**
2. **Categoria deve ser compatível com tipo de transação**
3. **Exclusão em cascata** de transações ao deletar pessoa
4. **Cálculo correto de idade**
5. **Validação de finalidade de categoria**


## 🏗️ Pirâmide de Testes

```
        /\
       /  \       E2E Tests (Playwright)
      /    \      - Fluxos completos de usuário
     /------\     - Validação de UX
    /        \    
   /          \   Integration Tests (xUnit + WebApplicationFactory)
  /            \  - APIs REST
 /              \ - Regras de negócio via HTTP
/________________\ Unit Tests (xUnit + Vitest)
                   - Entidades do domínio
                   - Lógica de validação
                   - Componentes React
```

### Distribuição de Testes

| Tipo | Quantidade Aprox. | Tempo Execução | Finalidade |
|------|-------------------|----------------|------------|
| **Unitários** | ~35 testes | < 5 segundos | Validar lógica isolada |
| **Integração** | ~15 testes | ~30 segundos | Validar APIs e fluxos |
| **E2E** | ~12 testes | ~2-3 minutos | Validar experiência |

## 🚀 Como Executar

### Pré-requisitos

#### Backend (.NET)
```bash
- .NET 8.0 SDK
- Referência ao projeto MinhasFinancas.API (para testes de integração)
```

#### Frontend (Node.js)
```bash
- Node.js 18+
- npm ou yarn
```

### Executar Testes Unitários - Backend

```bash
cd backend/MinhasFinancas.UnitTests

# Executar todos os testes
dotnet test

# Com cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Executar teste específico
dotnet test --filter "PessoaTests"
```

### Executar Testes de Integração - Backend

⚠️ **IMPORTANTE**: Antes de executar, você precisa:
1. Adicionar referência ao projeto da aplicação no `.csproj`:
```xml
<ProjectReference Include="../../caminho-para-app/MinhasFinancas.API/MinhasFinancas.API.csproj" />
```

2. Verificar se `Program.cs` tem a classe parcial:
```csharp
public partial class Program { }
```

```bash
cd backend/MinhasFinancas.IntegrationTests

# Executar todos os testes
dotnet test

# Executar com logs detalhados
dotnet test --logger "console;verbosity=detailed"

# Executar apenas testes de transações
dotnet test --filter "TransacoesControllerTests"
```

### Executar Testes Unitários - Frontend

```bash
cd frontend/unit-tests

# Instalar dependências
npm install

# Executar testes
npm test

# Modo watch (desenvolvimento)
npm run test:watch

# Com cobertura
npm run test:coverage

# Interface gráfica
npm run test:ui
```

### Executar Testes E2E - Frontend

⚠️ **IMPORTANTE**: Configure a URL da aplicação em `playwright.config.ts`

```bash
cd frontend/e2e-tests

# Instalar dependências
npm install

# Instalar browsers do Playwright
npx playwright install

# Executar testes
npm test

# Modo headed (ver o navegador)
npm run test:headed

# Modo debug
npm run test:debug

# Interface gráfica
npm run test:ui

# Ver relatório
npm run test:report
```

### Executar Todos os Testes

```bash
# Backend
cd backend/MinhasFinancas.UnitTests && dotnet test && cd ..
cd MinhasFinancas.IntegrationTests && dotnet test && cd ../..

# Frontend
cd frontend/unit-tests && npm test && cd ..
cd e2e-tests && npm test && cd ../..
```

## 📝 Regras de Negócio Testadas

### 1. Menor de Idade e Receitas

**Regra**: Pessoas com menos de 18 anos não podem registrar receitas.

**Testes**:
- ✅ `TransacaoTests.SetarPessoa_ReceitaComMenorDeIdade_DeveLancarExcecao`
- ✅ `TransacoesControllerTests.CriarTransacao_ReceitaComMenorDeIdade_DeveRetornarBadRequest`
- ✅ `transacoes.spec.ts - não deve permitir criar receita para menor de idade`

**Cobertura**: Unitário → Integração → E2E ✅

### 2. Categoria e Tipo de Transação

**Regra**: Categoria deve ser compatível com o tipo de transação.

**Categorias**:
- **Despesa**: Aceita apenas transações de Despesa
- **Receita**: Aceita apenas transações de Receita
- **Ambas**: Aceita qualquer tipo

**Testes**:
- ✅ `CategoriaTests.PermiteTipo_*` (todos os cenários)
- ✅ `TransacaoTests.SetarCategoria_*` (validações)
- ✅ `TransacoesControllerTests` (via API)
- ✅ `transacoes.spec.ts` (fluxo completo)

**Cobertura**: Unitário → Integração → E2E ✅

### 3. Exclusão em Cascata

**Regra**: Ao deletar pessoa, suas transações devem ser deletadas automaticamente.

**Testes**:
- ✅ `ExclusaoCascataTests.DeletarPessoa_ComTransacoes_DeveExcluirTransacoesEmCascata`
- ✅ `exclusao-cascata.spec.ts` (validação completa)

**Cobertura**: Integração → E2E ✅

### 4. Cálculo de Idade

**Regra**: Idade deve ser calculada corretamente considerando dia/mês/ano.

**Testes**:
- ✅ `PessoaTests.CalcularIdade_*` (múltiplos cenários)
- ✅ `PessoaTests.EhMaiorDeIdade_*` (validação de maioridade)
- ✅ `validacoes.test.ts` (lógica frontend)

**Cobertura**: Unitário (Backend + Frontend) ✅

### 5. CRUD Completo

**Funcionalidades**:
- Criar, listar, buscar, atualizar e deletar Pessoas
- Criar e listar Transações
- Validações de campos obrigatórios

**Testes**:
- ✅ `PessoasControllerTests` (todas as operações)
- ✅ `pessoas.spec.ts` (fluxo de usuário)

**Cobertura**: Integração → E2E ✅

## 🐛 Bugs Encontrados

Consulte [BUGS_ENCONTRADOS.md](./docs/BUGS_ENCONTRADOS.md) para lista completa e detalhada.

### Resumo

| Bug | Descrição | Severidade |
|-----|-----------|------------|
| #1 | Validação de receita menor idade só no backend | Alta |
| #2 | Validação categoria vs tipo só no backend | Média |
| #3 | Possível problema cascade delete | Crítica |
| #4 | Sem validação data futura | Baixa |
| #5 | Mensagens de erro não amigáveis | Baixa |

**Total de bugs críticos/altos encontrados**: 2  
**Total de melhorias identificadas**: 3

## 🛠️ Tecnologias Utilizadas

### Backend
- **xUnit**: Framework de testes .NET
- **FluentAssertions**: Assertions mais legíveis
- **Moq**: Mock de dependências
- **WebApplicationFactory**: Testes de integração com API real
- **InMemoryDatabase**: Banco isolado para testes

### Frontend
- **Vitest**: Framework de testes rápido para React
- **Testing Library**: Testes focados no usuário
- **Playwright**: Automação E2E cross-browser
- **TypeScript**: Type safety nos testes

### CI/CD (Opcional)
- **GitHub Actions**: Automação de testes

## 💡 Justificativa das Escolhas

### Por que essa estrutura de pirâmide?

1. **Base larga de unitários**: 
   - Executam rapidamente (< 5s)
   - Feedback imediato durante desenvolvimento
   - Facilitam debug de lógica complexa
   - Alto ROI: baixo custo, alta cobertura

2. **Camada intermediária de integração**:
   - Validam contratos de API
   - Garantem que camadas se comunicam corretamente
   - Detectam problemas de configuração
   - Custo moderado, valor alto

3. **Topo fino de E2E**:
   - Validam fluxos críticos do usuário
   - Garantem que sistema funciona end-to-end
   - Detectam problemas de UX
   - Alto custo, mas essencial para confiança

### Por que xUnit?

- Padrão da comunidade .NET
- Excelente integração com VS e VSCode
- Suporte a paralelização
- Sintaxe limpa e moderna

### Por que Vitest ao invés de Jest?

- **Mais rápido**: ESM nativo, sem transformações
- **Compatível com Vite**: mesma config do projeto
- **Melhor DX**: HMR nos testes
- **Moderno**: API compatível com Jest mas otimizada

### Por que Playwright ao invés de Cypress?

- **Multi-browser**: Chrome, Firefox, Safari
- **Mais robusto**: melhor handling de race conditions
- **Paralelo por padrão**: testes mais rápidos
- **API moderna**: async/await nativo


## 🎓 Aprendizados e Observações

### O que os testes revelaram

1. **Lógica de negócio está CORRETA** ✅
   - Todas as validações funcionam no backend
   - Cálculo de idade está perfeito
   - Regras de categoria implementadas corretamente

2. **UX pode melhorar** ⚠️
   - Validações preventivas no frontend evitariam erros
   - Filtros dinâmicos melhorariam experiência
   - Mensagens de erro podem ser mais amigáveis

3. **Cascade delete precisa validação** 🔍
   - Configuração parece correta mas não foi testada contra app real
   - É área crítica que merece atenção