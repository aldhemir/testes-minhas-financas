import { test, expect } from '@playwright/test';

test.describe('CRUD de Pessoas', () => {
  
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('deve criar uma nova pessoa maior de idade', async ({ page }) => {

    await page.click('text=Pessoas');
    await expect(page).toHaveURL(/.*pessoas/);

    await page.click('button:has-text("Nova Pessoa")');

    await page.fill('input[name="nome"]', 'João da Silva');
    await page.fill('input[name="dataNascimento"]', '1990-01-15');

    await page.click('button[type="submit"]');

    await expect(page.locator('text=João da Silva')).toBeVisible();
  });

  test('deve criar uma pessoa menor de idade', async ({ page }) => {
    await page.click('text=Pessoas');
    await page.click('button:has-text("Nova Pessoa")');

    await page.fill('input[name="nome"]', 'Maria Menor');
    
    const dataMenor = new Date();
    dataMenor.setFullYear(dataMenor.getFullYear() - 15);
    const dataFormatada = dataMenor.toISOString().split('T')[0];
    
    await page.fill('input[name="dataNascimento"]', dataFormatada);
    await page.click('button[type="submit"]');

    await expect(page.locator('text=Maria Menor')).toBeVisible();
  });

  test('deve editar uma pessoa existente', async ({ page }) => {
    await page.click('text=Pessoas');
    
    await page.click('button[title="Editar"]:first-of-type');

    await page.fill('input[name="nome"]', 'Nome Editado');
    await page.click('button[type="submit"]');

    await expect(page.locator('text=Nome Editado')).toBeVisible();
  });

  test('deve deletar uma pessoa', async ({ page }) => {
    await page.click('text=Pessoas');

    const primeiraPessoa = await page.locator('tr td:first-child').first().textContent();
    
    await page.click('button[title="Deletar"]:first-of-type');
    
    await page.click('button:has-text("Confirmar")');

    await expect(page.locator(`text=${primeiraPessoa}`)).not.toBeVisible();
  });

  test('deve validar campos obrigatórios', async ({ page }) => {
    await page.click('text=Pessoas');
    await page.click('button:has-text("Nova Pessoa")');
    await page.click('button[type="submit"]');
    await expect(page.locator('text=obrigatório')).toBeVisible();
  });

  test('deve buscar pessoa na listagem', async ({ page }) => {
    await page.click('text=Pessoas');
    await page.fill('input[placeholder*="Buscar"]', 'João');
    const resultados = page.locator('tbody tr');
    await expect(resultados).toHaveCount(1);
  });

  test('deve paginar lista de pessoas', async ({ page }) => {
    await page.click('text=Pessoas');
    const paginacao = page.locator('[role="navigation"]');
    await expect(paginacao).toBeVisible();
    const proximaPagina = page.locator('button:has-text("Próxima")');
    if (await proximaPagina.isEnabled()) {
      await proximaPagina.click();
      await expect(page).toHaveURL(/pageNumber=2/);
    }
  });
});
