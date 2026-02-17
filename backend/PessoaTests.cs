using FluentAssertions;
using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.UnitTests.Domain.Entities;

public class PessoaTests
{
    [Fact]
    public void CalcularIdade_PessoaMaiorDeIdade_DeveRetornarIdadeCorreta()
    {

        var dataNascimento = DateTime.Today.AddYears(-25);
        var pessoa = new Pessoa
        {
            Nome = "João Silva",
            DataNascimento = dataNascimento
        };

        var idade = pessoa.Idade;

        idade.Should().Be(25);
    }

    [Fact]
    public void CalcularIdade_PessoaMenorDeIdade_DeveRetornarIdadeCorreta()
    {

        var dataNascimento = DateTime.Today.AddYears(-15);
        var pessoa = new Pessoa
        {
            Nome = "Maria Santos",
            DataNascimento = dataNascimento
        };

        var idade = pessoa.Idade;

        idade.Should().Be(15);
    }

    [Fact]
    public void CalcularIdade_AniversarioNaoPassou_NaoDeveContarAnoAtual()
    {

        var dataNascimento = DateTime.Today.AddYears(-20).AddDays(1);
        var pessoa = new Pessoa
        {
            Nome = "Pedro Costa",
            DataNascimento = dataNascimento
        };

        var idade = pessoa.Idade;

        idade.Should().Be(19, "porque o aniversário ainda não ocorreu este ano");
    }

    [Fact]
    public void CalcularIdade_AniversarioHoje_DeveContarAnoAtual()
    {

        var dataNascimento = DateTime.Today.AddYears(-18);
        var pessoa = new Pessoa
        {
            Nome = "Ana Lima",
            DataNascimento = dataNascimento
        };

        var idade = pessoa.Idade;

        idade.Should().Be(18);
    }

    [Fact]
    public void EhMaiorDeIdade_PessoaCom18Anos_DeveRetornarTrue()
    {

        var dataNascimento = DateTime.Today.AddYears(-18);
        var pessoa = new Pessoa
        {
            Nome = "Carlos Souza",
            DataNascimento = dataNascimento
        };

        var ehMaior = pessoa.EhMaiorDeIdade();

        ehMaior.Should().BeTrue("pessoa com exatamente 18 anos é maior de idade");
    }

    [Fact]
    public void EhMaiorDeIdade_PessoaCom17Anos_DeveRetornarFalse()
    {

        var dataNascimento = DateTime.Today.AddYears(-17);
        var pessoa = new Pessoa
        {
            Nome = "Juliana Oliveira",
            DataNascimento = dataNascimento
        };

        var ehMaior = pessoa.EhMaiorDeIdade();

        ehMaior.Should().BeFalse("pessoa com 17 anos é menor de idade");
    }

    [Fact]
    public void EhMaiorDeIdade_PessoaCom25Anos_DeveRetornarTrue()
    {

        var dataNascimento = DateTime.Today.AddYears(-25);
        var pessoa = new Pessoa
        {
            Nome = "Roberto Alves",
            DataNascimento = dataNascimento
        };


        var ehMaior = pessoa.EhMaiorDeIdade();

        ehMaior.Should().BeTrue("pessoa com 25 anos é maior de idade");
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(-15)]
    [InlineData(-17)]
    public void EhMaiorDeIdade_PessoaMenorQue18_DeveRetornarFalse(int anosAtras)
    {

        var dataNascimento = DateTime.Today.AddYears(anosAtras);
        var pessoa = new Pessoa
        {
            Nome = "Teste",
            DataNascimento = dataNascimento
        };

        var ehMaior = pessoa.EhMaiorDeIdade();

        ehMaior.Should().BeFalse($"pessoa com {Math.Abs(anosAtras)} anos é menor de idade");
    }

    [Theory]
    [InlineData(-18)]
    [InlineData(-19)]
    [InlineData(-30)]
    [InlineData(-50)]
    public void EhMaiorDeIdade_PessoaMaiorOuIgual18_DeveRetornarTrue(int anosAtras)
    {

        var dataNascimento = DateTime.Today.AddYears(anosAtras);
        var pessoa = new Pessoa
        {
            Nome = "Teste",
            DataNascimento = dataNascimento
        };

        var ehMaior = pessoa.EhMaiorDeIdade();

        ehMaior.Should().BeTrue($"pessoa com {Math.Abs(anosAtras)} anos é maior de idade");
    }
}
