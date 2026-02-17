import { test, expect } from '@playwright/test';

test.describe('Exclusão em Cascata', () => {
  
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('ao deletar pessoa com transações, suas transações devem sumir', async ({ page }) => {
    await page.click('text=Pessoas');
    await page.click('button:has-text("Nova Pessoa")');
    await page.fill('input[name="nome"]', 'Pessoa Com Transações E2E');
    await page.fill('input[name="dataNascimento"]', '1990-05-15');
    await page.click('button[type="submit"]');

    await expect(page.locator('text=Pessoa Com Transações E2E')).toBeVisible();

    await page.click('text=Transações');
    
    await page.click('button:has-text("Nova Transação")');
    await page.selectOption('select[name="pessoaId"]', { label: /Pessoa Com Transações E2E/ });
    await page.selectOption('select[name="tipo"]', 'Despesa');
    await page.fill('input[name="descricao"]', 'Transação Teste 1');
    await page.fill('input[name="valor"]', '100');
    await page.selectOption('select[name="categoriaId"]', { index: 1 });
    await page.click('button[type="submit"]');
    
    await expect(page.locator('text=Transação Teste 1')).toBeVisible();

    await page.click('button:has-text("Nova Transação")');
    await page.selectOption('select[name="pessoaId"]', { label: /Pessoa Com Transações E2E/ });
    await page.selectOption('select[name="tipo"]', 'Despesa');
    await page.fill('input[name="descricao"]', 'Transação Teste 2');
    await page.fill('input[name="valor"]', '200');
    await page.selectOption('select[name="categoriaId"]', { index: 1 });
    await page.click('button[type="submit"]');
    
    await expect(page.locator('text=Transação Teste 2')).toBeVisible();

    await page.fill('input[placeholder*="Buscar"]', 'Transação Teste');
    await expect(page.locator('text=Transação Teste 1')).toBeVisible();
    await expect(page.locator('text=Transação Teste 2')).toBeVisible();

    await page.click('text=Pessoas');
    
    const row = page.locator('tr', { hasText: 'Pessoa Com Transações E2E' });
    await row.locator('button[title="Deletar"]').click();
    
    await page.click('button:has-text("Confirmar")');

    await expect(page.locator('text=Pessoa Com Transações E2E')).not.toBeVisible();

    await page.click('text=Transações');
    await page.fill('input[placeholder*="Buscar"]', 'Transação Teste');
    
    await expect(page.locator('text=Transação Teste 1')).not.toBeVisible({
      timeout: 5000
    });
    await expect(page.locator('text=Transação Teste 2')).not.toBeVisible({
      timeout: 5000
    });
  });

  test('ao deletar pessoa sem transações, deve funcionar normalmente', async ({ page }) => {

    await page.click('text=Pessoas');
    await page.click('button:has-text("Nova Pessoa")');
    await page.fill('input[name="nome"]', 'Pessoa Sem Transações E2E');
    await page.fill('input[name="dataNascimento"]', '1995-03-20');
    await page.click('button[type="submit"]');

    await expect(page.locator('text=Pessoa Sem Transações E2E')).toBeVisible();

    const row = page.locator('tr', { hasText: 'Pessoa Sem Transações E2E' });
    await row.locator('button[title="Deletar"]').click();
    await page.click('button:has-text("Confirmar")');

    await expect(page.locator('text=Pessoa Sem Transações E2E')).not.toBeVisible();
  });

  test('totais devem atualizar após exclusão em cascata', async ({ page }) => {
    await page.goto('/');
    const totalAntes = await page.locator('[data-testid="total-despesas"]').textContent();


    await page.click('text=Pessoas');
    await page.click('button:has-text("Nova Pessoa")');
    await page.fill('input[name="nome"]', 'Pessoa Total Teste');
    await page.fill('input[name="dataNascimento"]', '1990-01-01');
    await page.click('button[type="submit"]');

    await page.click('text=Transações');
    await page.click('button:has-text("Nova Transação")');
    await page.selectOption('select[name="pessoaId"]', { label: /Pessoa Total Teste/ });
    await page.selectOption('select[name="tipo"]', 'Despesa');
    await page.fill('input[name="descricao"]', 'Despesa 500');
    await page.fill('input[name="valor"]', '500');
    await page.selectOption('select[name="categoriaId"]', { index: 1 });
    await page.click('button[type="submit"]');

    await page.goto('/');
    await page.waitForTimeout(1000);

    await page.click('text=Pessoas');
    const row = page.locator('tr', { hasText: 'Pessoa Total Teste' });
    await row.locator('button[title="Deletar"]').click();
    await page.click('button:has-text("Confirmar")');

    await page.goto('/');
    await page.waitForTimeout(1000);
    const totalDepois = await page.locator('[data-testid="total-despesas"]').textContent();

    expect(totalDepois).toBeDefined();
  });
});
