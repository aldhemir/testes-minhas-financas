# 🔄 Guia de Adaptação dos Testes

Este documento explica como adaptar os testes deste repositório para o projeto Minhas Finanças real.

## 📝 Checklist de Adaptação

### ✅ Backend - Testes Unitários

**Status:** Pronto para uso  
**Ação necessária:** Nenhuma (testes independentes)

Os testes unitários **não requerem** modificação pois testam apenas as entidades de domínio que já estão presentes no código fornecido.

### 🔧 Backend - Testes de Integração

**Status:** Requer configuração  
**Ação necessária:** Adicionar referências ao projeto

#### Passo 1: Ajustar `.csproj`

Editar `backend/MinhasFinancas.IntegrationTests/MinhasFinancas.IntegrationTests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <!-- ... configurações existentes ... -->
  
  <ItemGroup>
    <!-- Adicionar estas referências -->
    <ProjectReference Include="..\..\..\ExameDesenvolvedorDeTestes\api\MinhasFinancas.API\MinhasFinancas.API.csproj" />
    <ProjectReference Include="..\..\..\ExameDesenvolvedorDeTestes\api\MinhasFinancas.Application\MinhasFinancas.Application.csproj" />
    <ProjectReference Include="..\..\..\ExameDesenvolvedorDeTestes\api\MinhasFinancas.Domain\MinhasFinancas.Domain.csproj" />
    <ProjectReference Include="..\..\..\ExameDesenvolvedorDeTestes\api\MinhasFinancas.Infrastructure\MinhasFinancas.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

**IMPORTANTE:** Ajuste os caminhos `..\..\..` conforme sua estrutura de diretórios.

#### Passo 2: Adicionar classe Program parcial

No projeto `MinhasFinancas.API`, editar `Program.cs` e adicionar no final:

```csharp
// Adicionar esta linha no final do arquivo
public partial class Program { }
```

Isso permite que `WebApplicationFactory` referencie a classe Program.

#### Passo 3: Ajustar CustomWebApplicationFactory

Editar `backend/MinhasFinancas.IntegrationTests/CustomWebApplicationFactory.cs`:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MinhasFinancasDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<MinhasFinancasDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
            });

            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<MinhasFinancasDbContext>();
                
                db.Database.EnsureCreated();

            }
        });
    }

}
```

### 🎨 Frontend - Testes Unitários

**Status:** Requer adaptação dos componentes  
**Ação necessária:** Copiar componentes reais e ajustar testes

#### Passo 1: Identificar componentes reais

No projeto web, os componentes estão em:
```
web/src/components/molecules/PessoaForm.tsx
web/src/components/molecules/TransacaoForm.tsx
```

#### Passo 2: Copiar para projeto de testes

```bash
# Criar estrutura
mkdir -p frontend/unit-tests/src/components/molecules

# Copiar componentes
cp web/src/components/molecules/PessoaForm.tsx \
   frontend/unit-tests/src/components/molecules/

cp web/src/components/molecules/TransacaoForm.tsx \
   frontend/unit-tests/src/components/molecules/
```

#### Passo 3: Ajustar imports nos testes

Editar `frontend/unit-tests/src/components/PessoaForm.test.tsx`:

```typescript
// ❌ REMOVER: Mock component
// import MockPessoaForm from './MockPessoaForm';

// ✅ ADICIONAR: Import real
import PessoaForm from './molecules/PessoaForm';

describe('PessoaForm Component', () => {
  it('deve renderizar todos os campos', () => {
    const mockSubmit = vi.fn();
    
    // ✅ Usar componente real
    render(<PessoaForm onSubmit={mockSubmit} />);
    
    expect(screen.getByLabelText(/nome/i)).toBeInTheDocument();
  });
});
```

#### Passo 4: Configurar alias de imports

Se o projeto usa `@/` para imports, adicionar ao `vitest.config.ts`:

```typescript
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  // ... resto da config
});
```

### 🌐 Frontend - Testes E2E

**Status:** Requer ajuste de seletores  
**Ação necessária:** Atualizar seletores conforme HTML real

#### Problema Comum: Seletores não encontram elementos

```typescript
// ❌ Genérico (pode não funcionar)
await page.click('button:has-text("Nova Pessoa")');

// ✅ Específico (mais confiável)
await page.click('[data-testid="btn-nova-pessoa"]');
```

#### Passo 1: Adicionar data-testid nos componentes

No código da aplicação web, adicionar atributos de teste:

```tsx
// web/src/pages/PessoasList.tsx
<button 
  data-testid="btn-nova-pessoa"  // ✅ Adicionar isso
  onClick={handleNovaPessoa}
>
  Nova Pessoa
</button>

<input
  data-testid="input-nome-pessoa"  // ✅ Adicionar isso
  name="nome"
  type="text"
/>
```

#### Passo 2: Atualizar testes E2E

```typescript
// frontend/e2e-tests/tests/pessoas.spec.ts

test('deve criar nova pessoa', async ({ page }) => {
  // ❌ Antes (frágil)
  // await page.click('button:has-text("Nova Pessoa")');
  
  // ✅ Depois (robusto)
  await page.click('[data-testid="btn-nova-pessoa"]');
  
  // ❌ Antes
  // await page.fill('input[name="nome"]', 'João');
  
  // ✅ Depois
  await page.fill('[data-testid="input-nome-pessoa"]', 'João');
});
```

#### Passo 3: Mapear todas as telas

Criar arquivo `frontend/e2e-tests/page-objects/pessoas-page.ts`:

```typescript
import { Page } from '@playwright/test';

export class PessoasPage {
  constructor(private page: Page) {}

  async irParaPessoas() {
    await this.page.goto('/pessoas');
  }

  async clicarNovaPessoa() {
    await this.page.click('[data-testid="btn-nova-pessoa"]');
  }

  async preencherNome(nome: string) {
    await this.page.fill('[data-testid="input-nome-pessoa"]', nome);
  }

  async preencherDataNascimento(data: string) {
    await this.page.fill('[data-testid="input-data-nascimento"]', data);
  }

  async submeter() {
    await this.page.click('[data-testid="btn-salvar-pessoa"]');
  }
}
```

Usar no teste:

```typescript
import { PessoasPage } from '../page-objects/pessoas-page';

test('deve criar nova pessoa', async ({ page }) => {
  const pessoasPage = new PessoasPage(page);
  
  await pessoasPage.irParaPessoas();
  await pessoasPage.clicarNovaPessoa();
  await pessoasPage.preencherNome('João Silva');
  await pessoasPage.preencherDataNascimento('1990-01-15');
  await pessoasPage.submeter();
  
  await expect(page.locator('text=João Silva')).toBeVisible();
});
```

## 🔍 Descobrindo Seletores Corretos

### Método 1: Playwright Inspector

```bash
cd frontend/e2e-tests
npx playwright codegen http://localhost:3000
```

Isso abre um navegador onde você pode:
1. Interagir com a aplicação
2. Ver código gerado automaticamente
3. Copiar seletores corretos

### Método 2: DevTools do Navegador

```bash
npm run test:debug
```

Quando teste pausar:
1. Abrir DevTools (F12)
2. Usar console para testar seletores:
```javascript
document.querySelector('[data-testid="btn-nova-pessoa"]')
```

### Método 3: Executar com headed

```bash
npm run test:headed
```

Ver exatamente o que Playwright está tentando clicar.

## 📋 Checklist de Validação

Após adaptar, verificar:

### ✅ Backend - Unitários
```bash
cd backend/MinhasFinancas.UnitTests
dotnet test
# Deve passar: ~35 testes
```

### ✅ Backend - Integração
```bash
cd backend/MinhasFinancas.IntegrationTests
dotnet test
# Deve passar: ~15 testes
```

### ✅ Frontend - Unitários
```bash
cd frontend/unit-tests
npm test
# Deve passar: ~13 testes
```

### ✅ Frontend - E2E
```bash
# 1. Subir aplicação
docker-compose up -d

# 2. Executar testes
cd frontend/e2e-tests
npm test
# Deve passar: ~12 testes
```

## 🐛 Problemas Comuns

### ❌ "Cannot find module MinhasFinancas.Domain"

**Causa:** Caminho incorreto nas ProjectReferences

**Solução:**
```xml
<!-- Verificar caminho relativo -->
<ProjectReference Include="..\..\..\caminho-correto\MinhasFinancas.Domain\MinhasFinancas.Domain.csproj" />
```

### ❌ "Test timeout exceeded"

**Causa:** Aplicação não está rodando ou está lenta

**Solução:**
```typescript
// playwright.config.ts
export default defineConfig({
  use: {
    navigationTimeout: 60000, // Aumentar timeout
  },
});
```

### ❌ "Locator not found"

**Causa:** Seletor incorreto ou elemento não existe

**Solução:**
1. Usar Playwright Inspector para descobrir seletor correto
2. Adicionar data-testid nos componentes
3. Verificar se elemento está realmente renderizado

## 📈 Evolução Gradual

Não é necessário adaptar tudo de uma vez. Sugestão de ordem:

1. ✅ **Semana 1:** Backend unitários (já funciona)
2. ✅ **Semana 2:** Backend integração (configurar referências)
3. ✅ **Semana 3:** Frontend unitários (copiar componentes)
4. ✅ **Semana 4:** E2E críticos (principais fluxos)
5. ✅ **Semana 5+:** Refinar e adicionar mais casos

## 🎯 Resultado Final

Após adaptação completa, você terá:

```
✅ ~35 testes unitários backend
✅ ~15 testes integração backend
✅ ~13 testes unitários frontend
✅ ~12 testes E2E
───────────────────────────────
   ~75 testes automatizados
   
Tempo total: ~5 minutos
Confiança: 🚀 Alta
```

---

**Dica:** Comece pelos testes que agregam mais valor (regras de negócio críticas)!
