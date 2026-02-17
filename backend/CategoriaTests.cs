using FluentAssertions;
using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.UnitTests.Domain.Entities;

public class CategoriaTests
{
    [Fact]
    public void PermiteTipo_CategoriaDespesa_DevePermitirApenasTransacaoDespesa()
    {

        var categoria = new Categoria
        {
            Descricao = "Alimentação",
            Finalidade = Categoria.EFinalidade.Despesa
        };

        var permiteDespesa = categoria.PermiteTipo(Transacao.ETipo.Despesa);
        var permiteReceita = categoria.PermiteTipo(Transacao.ETipo.Receita);

        permiteDespesa.Should().BeTrue("categoria de despesa deve permitir transação tipo Despesa");
        permiteReceita.Should().BeFalse("categoria de despesa NÃO deve permitir transação tipo Receita");
    }

    [Fact]
    public void PermiteTipo_CategoriaReceita_DevePermitirApenasTransacaoReceita()
    {

        var categoria = new Categoria
        {
            Descricao = "Salário",
            Finalidade = Categoria.EFinalidade.Receita
        };

        var permiteDespesa = categoria.PermiteTipo(Transacao.ETipo.Despesa);
        var permiteReceita = categoria.PermiteTipo(Transacao.ETipo.Receita);

        permiteDespesa.Should().BeFalse("categoria de receita NÃO deve permitir transação tipo Despesa");
        permiteReceita.Should().BeTrue("categoria de receita deve permitir transação tipo Receita");
    }

    [Fact]
    public void PermiteTipo_CategoriaAmbas_DevePermitirDespesaEReceita()
    {

        var categoria = new Categoria
        {
            Descricao = "Transferências",
            Finalidade = Categoria.EFinalidade.Ambas
        };

        var permiteDespesa = categoria.PermiteTipo(Transacao.ETipo.Despesa);
        var permiteReceita = categoria.PermiteTipo(Transacao.ETipo.Receita);

        permiteDespesa.Should().BeTrue("categoria Ambas deve permitir transação tipo Despesa");
        permiteReceita.Should().BeTrue("categoria Ambas deve permitir transação tipo Receita");
    }

    [Theory]
    [InlineData(Categoria.EFinalidade.Despesa, Transacao.ETipo.Despesa, true)]
    [InlineData(Categoria.EFinalidade.Despesa, Transacao.ETipo.Receita, false)]
    [InlineData(Categoria.EFinalidade.Receita, Transacao.ETipo.Despesa, false)]
    [InlineData(Categoria.EFinalidade.Receita, Transacao.ETipo.Receita, true)]
    [InlineData(Categoria.EFinalidade.Ambas, Transacao.ETipo.Despesa, true)]
    [InlineData(Categoria.EFinalidade.Ambas, Transacao.ETipo.Receita, true)]
    public void PermiteTipo_DiferentesCombinacoes_DeveRetornarResultadoEsperado(
        Categoria.EFinalidade finalidade,
        Transacao.ETipo tipo,
        bool resultadoEsperado)
    {

        var categoria = new Categoria
        {
            Descricao = "Teste",
            Finalidade = finalidade
        };

        var resultado = categoria.PermiteTipo(tipo);

        resultado.Should().Be(resultadoEsperado,
            $"categoria {finalidade} {(resultadoEsperado ? "deve" : "não deve")} permitir {tipo}");
    }
}
