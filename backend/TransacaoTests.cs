using FluentAssertions;
using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.UnitTests.Domain.Entities;

public class TransacaoTests
{
    [Fact]
    public void SetarPessoa_ReceitaComMenorDeIdade_DeveLancarExcecao()
    {

        var pessoaMenor = new Pessoa
        {
            Nome = "Menor de Idade",
            DataNascimento = DateTime.Today.AddYears(-15)
        };

        var transacao = new Transacao
        {
            Descricao = "Mesada",
            Valor = 100,
            Tipo = Transacao.ETipo.Receita
        };

        Action act = () => transacao.Pessoa = pessoaMenor;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Menores de 18 anos não podem registrar receitas.");
    }

    [Fact]
    public void SetarPessoa_DespesaComMenorDeIdade_NaoDeveLancarExcecao()
    {

        var pessoaMenor = new Pessoa
        {
            Nome = "Menor de Idade",
            DataNascimento = DateTime.Today.AddYears(-15)
        };

        var transacao = new Transacao
        {
            Descricao = "Lanche",
            Valor = 50,
            Tipo = Transacao.ETipo.Despesa
        };

        Action act = () => transacao.Pessoa = pessoaMenor;

        act.Should().NotThrow("menores podem registrar despesas");
    }

    [Fact]
    public void SetarPessoa_ReceitaComMaiorDeIdade_NaoDeveLancarExcecao()
    {

        var pessoaMaior = new Pessoa
        {
            Nome = "Maior de Idade",
            DataNascimento = DateTime.Today.AddYears(-25)
        };

        var transacao = new Transacao
        {
            Descricao = "Salário",
            Valor = 3000,
            Tipo = Transacao.ETipo.Receita
        };

        Action act = () => transacao.Pessoa = pessoaMaior;

        act.Should().NotThrow("maiores de idade podem registrar receitas");
    }

    [Fact]
    public void SetarPessoa_PessoaCom18AnosReceita_NaoDeveLancarExcecao()
    {

        var pessoa18Anos = new Pessoa
        {
            Nome = "Pessoa com 18 anos",
            DataNascimento = DateTime.Today.AddYears(-18)
        };

        var transacao = new Transacao
        {
            Descricao = "Primeiro Salário",
            Valor = 1500,
            Tipo = Transacao.ETipo.Receita
        };

        Action act = () => transacao.Pessoa = pessoa18Anos;

        act.Should().NotThrow("pessoa com exatamente 18 anos pode registrar receitas");
    }

    [Fact]
    public void SetarCategoria_DespesaEmCategoriaReceita_DeveLancarExcecao()
    {

        var categoriaReceita = new Categoria
        {
            Descricao = "Salário",
            Finalidade = Categoria.EFinalidade.Receita
        };

        var transacao = new Transacao
        {
            Descricao = "Almoço",
            Valor = 50,
            Tipo = Transacao.ETipo.Despesa
        };


        Action act = () => transacao.Categoria = categoriaReceita;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível registrar despesa em categoria de receita.");
    }

    [Fact]
    public void SetarCategoria_ReceitaEmCategoriaDespesa_DeveLancarExcecao()
    {

        var categoriaDespesa = new Categoria
        {
            Descricao = "Alimentação",
            Finalidade = Categoria.EFinalidade.Despesa
        };

        var transacao = new Transacao
        {
            Descricao = "Freelance",
            Valor = 500,
            Tipo = Transacao.ETipo.Receita
        };

        Action act = () => transacao.Categoria = categoriaDespesa;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível registrar receita em categoria de despesa.");
    }

    [Fact]
    public void SetarCategoria_DespesaEmCategoriaDespesa_NaoDeveLancarExcecao()
    {

        var categoriaDespesa = new Categoria
        {
            Descricao = "Alimentação",
            Finalidade = Categoria.EFinalidade.Despesa
        };

        var transacao = new Transacao
        {
            Descricao = "Supermercado",
            Valor = 200,
            Tipo = Transacao.ETipo.Despesa
        };

        Action act = () => transacao.Categoria = categoriaDespesa;

        act.Should().NotThrow("despesa deve ser aceita em categoria de despesa");
    }

    [Fact]
    public void SetarCategoria_ReceitaEmCategoriaReceita_NaoDeveLancarExcecao()
    {

        var categoriaReceita = new Categoria
        {
            Descricao = "Salário",
            Finalidade = Categoria.EFinalidade.Receita
        };

        var transacao = new Transacao
        {
            Descricao = "Salário Mensal",
            Valor = 5000,
            Tipo = Transacao.ETipo.Receita
        };

        Action act = () => transacao.Categoria = categoriaReceita;

        act.Should().NotThrow("receita deve ser aceita em categoria de receita");
    }

    [Fact]
    public void SetarCategoria_QualquerTipoEmCategoriaAmbas_NaoDeveLancarExcecao()
    {

        var categoriaAmbas = new Categoria
        {
            Descricao = "Transferências",
            Finalidade = Categoria.EFinalidade.Ambas
        };

        var despesa = new Transacao
        {
            Descricao = "Transferência Saída",
            Valor = 100,
            Tipo = Transacao.ETipo.Despesa
        };

        var receita = new Transacao
        {
            Descricao = "Transferência Entrada",
            Valor = 200,
            Tipo = Transacao.ETipo.Receita
        };

        Action actDespesa = () => despesa.Categoria = categoriaAmbas;
        Action actReceita = () => receita.Categoria = categoriaAmbas;

        actDespesa.Should().NotThrow("categoria Ambas aceita despesas");
        actReceita.Should().NotThrow("categoria Ambas aceita receitas");
    }

    [Fact]
    public void SetarPessoaECategoria_ReceitaMenorEmCategoriaDespesa_DeveLancarExcecaoDaPessoa()
    {

        var pessoaMenor = new Pessoa
        {
            Nome = "Menor",
            DataNascimento = DateTime.Today.AddYears(-15)
        };

        var categoriaDespesa = new Categoria
        {
            Descricao = "Alimentação",
            Finalidade = Categoria.EFinalidade.Despesa
        };

        var transacao = new Transacao
        {
            Descricao = "Teste",
            Valor = 100,
            Tipo = Transacao.ETipo.Receita
        };

        Action actCategoria = () => transacao.Categoria = categoriaDespesa;
        Action actPessoa = () => transacao.Pessoa = pessoaMenor;

        actCategoria.Should().Throw<InvalidOperationException>()
            .WithMessage("*receita em categoria de despesa*");
        
        actPessoa.Should().Throw<InvalidOperationException>()
            .WithMessage("Menores de 18 anos não podem registrar receitas.");
    }
}
