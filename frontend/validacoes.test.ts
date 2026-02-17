import { describe, it, expect } from 'vitest';


function calcularIdade(dataNascimento: Date): number {
  const hoje = new Date();
  let idade = hoje.getFullYear() - dataNascimento.getFullYear();
  const mesAtual = hoje.getMonth();
  const mesNascimento = dataNascimento.getMonth();
  
  if (mesAtual < mesNascimento || 
      (mesAtual === mesNascimento && hoje.getDate() < dataNascimento.getDate())) {
    idade--;
  }
  
  return idade;
}

function ehMaiorDeIdade(dataNascimento: Date): boolean {
  return calcularIdade(dataNascimento) >= 18;
}

describe('Validação de Idade', () => {
  
  it('deve calcular idade corretamente para pessoa com 25 anos', () => {
    const hoje = new Date();
    const dataNascimento = new Date(hoje.getFullYear() - 25, hoje.getMonth(), hoje.getDate());
    
    const idade = calcularIdade(dataNascimento);
    
    expect(idade).toBe(25);
  });

  it('deve calcular idade corretamente quando aniversário não passou', () => {
    const hoje = new Date();
    const dataNascimento = new Date(hoje.getFullYear() - 20, hoje.getMonth(), hoje.getDate() + 1);
    
    const idade = calcularIdade(dataNascimento);
    
    expect(idade).toBe(19);
  });

  it('deve retornar true para pessoa com 18 anos', () => {
    const hoje = new Date();
    const dataNascimento = new Date(hoje.getFullYear() - 18, hoje.getMonth(), hoje.getDate());
    
    const resultado = ehMaiorDeIdade(dataNascimento);
    
    expect(resultado).toBe(true);
  });

  it('deve retornar false para pessoa com 17 anos', () => {
    const hoje = new Date();
    const dataNascimento = new Date(hoje.getFullYear() - 17, hoje.getMonth(), hoje.getDate());
    
    const resultado = ehMaiorDeIdade(dataNascimento);
    
    expect(resultado).toBe(false);
  });

  it('deve retornar true para pessoa com 30 anos', () => {
    const hoje = new Date();
    const dataNascimento = new Date(hoje.getFullYear() - 30, hoje.getMonth(), hoje.getDate());
    
    const resultado = ehMaiorDeIdade(dataNascimento);
    
    expect(resultado).toBe(true);
  });
});

describe('Validação de Regras de Negócio', () => {
  
  it('deve permitir receita para maior de idade', () => {
    const dataNascimento = new Date(1990, 0, 1);
    const tipo = 'Receita';
    
    const podeRegistrar = tipo === 'Despesa' || ehMaiorDeIdade(dataNascimento);
    
    expect(podeRegistrar).toBe(true);
  });

  it('não deve permitir receita para menor de idade', () => {
    const hoje = new Date();
    const dataNascimento = new Date(hoje.getFullYear() - 15, 0, 1);
    const tipo = 'Receita';
    
    const podeRegistrar = tipo === 'Despesa' || ehMaiorDeIdade(dataNascimento);
    
    expect(podeRegistrar).toBe(false);
  });

  it('deve sempre permitir despesa independente da idade', () => {
    const dataNascimento = new Date(2010, 0, 1);
    const tipo = 'Despesa';
    
    const podeRegistrar = tipo === 'Despesa' || ehMaiorDeIdade(dataNascimento);
    
    expect(podeRegistrar).toBe(true);
  });
});
