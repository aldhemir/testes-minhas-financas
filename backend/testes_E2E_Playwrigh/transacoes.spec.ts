import { test, expect } from '@playwright/test';

test.describe('Regras de Negócio - Transações', () => {
  
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('não deve permitir criar receita para menor de idade', async ({ page }) => {
    await page.click('text=Pessoas');
    await page.click('button:has-text("Nova Pessoa")');
    await page.fill('input[name="nome"]', 'Menor Teste E2E');
    
    const dataMenor = new Date();
    dataMenor.setFullYear(dataMenor.getFullYear() - 15);
    await page.fill('input[name="dataNascimento"]', dataMenor.toISOString().split('T')[0]);
    await page.click('button[type="submit"]');
    await page.click('text=Transações');
    await page.click('button:has-text("Nova Transação")');
    await page.click('select[name="pessoaId"]');
    await page.selectOption('select[name="pessoaId"]', { label: /Menor Teste E2E/ });
    await page.selectOption('select[name="tipo"]', 'Receita');
    await page.fill('input[name="descricao"]', 'Tentativa Receita Menor');
    await page.fill('input[name="valor"]', '100');
    await page.selectOption('select[name="categoriaId"]', { index: 1 });
    await page.click('button[type="submit"]');
    await expect(page.locator('text=/18 anos|menor de idade/i')).toBeVisible({
      timeout: 5000
    });
  });

  test('deve permitir criar despesa para menor de idade', async ({ page }) => {
    await page.click('text=Transações');
    await page.click('button:has-text("Nova Transação")');
    await page.selectOption('select[name="pessoaId"]', { index: 1 });
    await page.selectOption('select[name="tipo"]', 'Despesa');

    await page.fill('input[name="descricao"]', 'Lanche Escola');
    await page.fill('input[name="valor"]', '20');
    await page.selectOption('select[name="categoriaId"]', { index: 1 });

    await page.click('button[type="submit"]');

    await expect(page.locator('text=Lanche Escola')).toBeVisible();
  });

  test('não deve permitir receita em categoria de despesa', async ({ page }) => {
    await page.click('text=Transações');
    await page.click('button:has-text("Nova Transação")');
    await page.selectOption('select[name="pessoaId"]', { index: 1 });
    await page.selectOption('select[name="tipo"]', 'Receita');
    await page.fill('input[name="descricao"]', 'Tentativa Incompatível');
    await page.fill('input[name="valor"]', '500');
  });

  test('não deve permitir despesa em categoria de receita', async ({ page }) => {
    await page.click('text=Transações');
    await page.click('button:has-text("Nova Transação")');
    await page.selectOption('select[name="pessoaId"]', { index: 1 });
    await page.selectOption('select[name="tipo"]', 'Despesa');
    await page.fill('input[name="descricao"]', 'Tentativa Incompatível 2');
    await page.fill('input[name="valor"]', '200');

  });

  test('deve permitir qualquer tipo em categoria Ambas', async ({ page }) => {
    await page.click('text=Transações');
    await page.click('button:has-text("Nova Transação")');
    await page.selectOption('select[name="pessoaId"]', { index: 1 });
    await page.selectOption('select[name="tipo"]', 'Despesa');
    await page.fill('input[name="descricao"]', 'Despesa em Ambas');
    await page.fill('input[name="valor"]', '100');
    await page.click('select[name="categoriaId"]');
    await page.selectOption('select[name="categoriaId"]', { label: /Ambas|Transferência/ });
    
    await page.click('button[type="submit"]');
    await expect(page.locator('text=Despesa em Ambas')).toBeVisible();
    await page.click('button:has-text("Nova Transação")');
    await page.selectOption('select[name="pessoaId"]', { index: 1 });
    await page.selectOption('select[name="tipo"]', 'Receita');
    await page.fill('input[name="descricao"]', 'Receita em Ambas');
    await page.fill('input[name="valor"]', '200');
    await page.selectOption('select[name="categoriaId"]', { label: /Ambas|Transferência/ });
    
    await page.click('button[type="submit"]');
    await expect(page.locator('text=Receita em Ambas')).toBeVisible();
  });

  test('deve validar valor mínimo da transação', async ({ page }) => {
    await page.click('text=Transações');
    await page.click('button:has-text("Nova Transação")');

    await page.fill('input[name="valor"]', '0');
    await page.click('button[type="submit"]');

    await expect(page.locator('text=/maior que zero|mínimo/i')).toBeVisible();
  });

  test('deve exibir totais corretos no dashboard', async ({ page }) => {
    await page.goto('/');

    await expect(page.locator('text=/Total Receitas|Receitas/i')).toBeVisible();
    await expect(page.locator('text=/Total Despesas|Despesas/i')).toBeVisible();
    await expect(page.locator('text=/Saldo|Balanço/i')).toBeVisible();

    const receitas = await page.locator('[data-testid="total-receitas"]').textContent();
    const despesas = await page.locator('[data-testid="total-despesas"]').textContent();

    expect(receitas).toMatch(/\d/);
    expect(despesas).toMatch(/\d/);
  });
});
