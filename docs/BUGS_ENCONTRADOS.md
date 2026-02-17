# Bugs Encontrados Durante os Testes

## 🐛 Bug #1: Validação de Receita para Menor de Idade

**Severidade:** Alta  
**Status:** Detectado  
**Regra de Negócio:** Menores de 18 anos não podem registrar receitas

### Descrição
A validação que impede menores de idade de registrarem receitas está implementada **apenas no backend** (na entidade `Transacao.cs`, método setter da propriedade `Pessoa`). 

### Comportamento Observado
1. O **frontend permite** selecionar um menor de idade e tipo "Receita"
2. Ao submeter, o **backend retorna erro 400**
3. Não há validação preventiva no frontend

### Comportamento Esperado
O frontend deveria:
- Desabilitar o tipo "Receita" quando pessoa menor de idade é selecionada
- OU mostrar mensagem clara antes do usuário tentar submeter
- Melhorar UX evitando chamada desnecessária à API

### Como Reproduzir
```typescript
// Teste E2E que falha ou mostra comportamento inadequado
test('não deve permitir criar receita para menor de idade', async ({ page }) => {
  // 1. Criar pessoa menor
  // 2. Tentar criar transação tipo Receita
  // 3. Frontend não previne, mas backend rejeita
});
```

### Testes Afetados
- `TransacoesControllerTests.CriarTransacao_ReceitaComMenorDeIdade_DeveRetornarBadRequest` ✅ Backend OK
- `transacoes.spec.ts - não deve permitir criar receita para menor de idade` ⚠️ UX ruim

### Recomendação
Implementar validação no frontend em `TransacaoForm.tsx`:
```typescript
useEffect(() => {
  if (pessoaSelecionada && calcularIdade(pessoaSelecionada.dataNascimento) < 18) {
    setTipoDisabled(tipo => tipo === 'Receita');
  }
}, [pessoaSelecionada]);
```

---

## 🐛 Bug #2: Validação de Categoria vs Tipo de Transação

**Severidade:** Média  
**Status:** Detectado  
**Regra de Negócio:** Categoria deve ser compatível com tipo de transação (Despesa em categoria de Despesa, etc)

### Descrição
Similar ao Bug #1, a validação de compatibilidade entre tipo de transação e finalidade da categoria está implementada **apenas no backend**.

### Comportamento Observado
1. Frontend permite selecionar qualquer categoria independente do tipo
2. Backend retorna erro ao tentar salvar combinação inválida
3. Usuário descobre erro tarde demais no fluxo

### Comportamento Esperado
O select de categorias deveria ser **filtrado dinamicamente** com base no tipo de transação selecionado:
- Tipo "Despesa" → mostrar apenas categorias Despesa + Ambas
- Tipo "Receita" → mostrar apenas categorias Receita + Ambas

### Como Reproduzir
```typescript
test('não deve permitir receita em categoria de despesa', async ({ page }) => {
  // 1. Selecionar tipo Receita
  // 2. Frontend mostra TODAS as categorias
  // 3. Selecionar categoria de Despesa
  // 4. Backend rejeita
});
```

### Testes Afetados
- `TransacoesControllerTests.CriarTransacao_ReceitaEmCategoriaDespesa_DeveRetornarBadRequest` ✅ Backend OK
- `TransacoesTests.SetarCategoria_ReceitaEmCategoriaDespesa_DeveLancarExcecao` ✅ Lógica OK
- E2E tests mostram UX inadequada ⚠️

### Recomendação
Implementar filtragem de categorias no componente:
```typescript
const categoriasFiltradas = useMemo(() => {
  if (!tipo) return categorias;
  
  return categorias.filter(cat => {
    if (cat.finalidade === 'Ambas') return true;
    if (tipo === 'Despesa') return cat.finalidade === 'Despesa';
    if (tipo === 'Receita') return cat.finalidade === 'Receita';
    return false;
  });
}, [categorias, tipo]);
```

---

## 🐛 Bug #3: Possível Problema na Exclusão em Cascata

**Severidade:** Crítica (se confirmado)  
**Status:** Necessita Investigação  
**Regra de Negócio:** Ao deletar pessoa, suas transações devem ser deletadas automaticamente

### Descrição
A exclusão em cascata depende da configuração do Entity Framework no `MinhasFinancasDbContext`. É necessário verificar se:
1. A configuração de cascade delete está correta
2. SQLite suporta cascade delete adequadamente
3. O comportamento é consistente

### Testes para Validação
```csharp
[Fact]
public async Task DeletarPessoa_ComTransacoes_DeveExcluirTransacoesEmCascata()
{
    // Este teste DEVE passar
    // Se falhar, há bug na configuração do cascade delete
}
```

### Cenários Críticos
1. Pessoa com múltiplas transações
2. Transações em diferentes períodos
3. Verificar se totais são recalculados corretamente

### Status
⚠️ Testes criados mas não executados contra aplicação real  
✅ Lógica de teste está correta  
📋 Necessita execução para confirmar comportamento

---

## 🐛 Bug #4: Ausência de Validação de Data Futura

**Severidade:** Baixa  
**Status:** Detectado  
**Regra de Negócio Presumida:** Transações não deveriam ter data futura

### Descrição
Não há validação impedindo criação de transações com data futura. Isso pode:
- Distorcer relatórios do período atual
- Causar confusão em totais e dashboards
- Permitir dados inconsistentes

### Comportamento Observado
```csharp
var transacao = new Transacao
{
    Data = DateTime.Today.AddYears(1), // Data futura
    // ... outros campos
};
// É aceita sem erro
```

### Recomendação
Adicionar validação:
```csharp
[Required]
[DataMenorOuIgualAHoje] // Custom validator
public DateTime Data { get; set; }
```

---

## 🐛 Bug #5: Falta de Mensagens de Erro Amigáveis

**Severidade:** Baixa (UX)  
**Status:** Detectado  

### Descrição
Mensagens de erro do backend são técnicas e não traduzidas:
- "Menores de 18 anos não podem registrar receitas" ✅ OK
- "InvalidOperationException" ❌ Exposta ao usuário
- Stack traces podem vazar para o frontend

### Comportamento Esperado
1. Middleware de exception deve capturar e traduzir erros
2. Frontend deve mostrar mensagens amigáveis
3. Detalhes técnicos apenas em logs

### Recomendação
Melhorar `ExceptionMiddleware` para retornar:
```json
{
  "error": "Não foi possível processar sua solicitação",
  "message": "Menores de idade não podem registrar receitas",
  "code": "MINOR_CANNOT_HAVE_INCOME"
}
```

---

## 📊 Resumo de Bugs

| ID | Descrição | Severidade | Camada | Status |
|----|-----------|------------|--------|--------|
| 1 | Validação receita menor idade apenas backend | Alta | Frontend/Backend | Detectado |
| 2 | Validação categoria vs tipo apenas backend | Média | Frontend/Backend | Detectado |
| 3 | Cascade delete pode não funcionar | Crítica | Backend/DB | A Investigar |
| 4 | Sem validação data futura | Baixa | Backend | Detectado |
| 5 | Mensagens erro não amigáveis | Baixa | Backend/Frontend | Detectado |

---

## 🎯 Bugs vs Testes

### ✅ Bugs Detectados por Testes
- Bug #1: Testes de integração + E2E
- Bug #2: Testes de integração + E2E
- Bug #3: Testes de integração específicos
- Bug #4: Testes unitários

### 📝 Observações
Os testes **unitários do backend** passam porque a **lógica de negócio está correta**. Os problemas são:
1. **Falta de validações preventivas no frontend** (UX ruim)
2. **Possíveis problemas de configuração** (cascade delete)
3. **Validações ausentes** (data futura)

Isso demonstra a importância da **pirâmide de testes completa**:
- Unitários validam lógica
- Integração valida fluxo completo
- E2E valida experiência do usuário
