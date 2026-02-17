using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace MinhasFinancas.IntegrationTests.Controllers;


public class TransacoesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public TransacoesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task CriarTransacao_ReceitaComMenorDeIdade_DeveRetornarBadRequest()
    {

        var pessoaMenorId = await CriarPessoaHelper("Menor Teste", -15);
        var categoriaReceitaId = await CriarCategoriaHelper("Salário", 1);

        var transacao = new
        {
            Descricao = "Mesada",
            Valor = 100.00,
            Tipo = 1,
            Data = DateTime.Today.ToString("yyyy-MM-dd"),
            PessoaId = pessoaMenorId,
            CategoriaId = categoriaReceitaId
        };


        var response = await _client.PostAsJsonAsync("/api/transacoes", transacao);


        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "menores de 18 anos NÃO podem registrar receitas");
        
        var conteudo = await response.Content.ReadAsStringAsync();
        conteudo.Should().Contain("18 anos");
    }

    [Fact]
    public async Task CriarTransacao_DespesaComMenorDeIdade_DeveRetornarCreated()
    {

        var pessoaMenorId = await CriarPessoaHelper("Menor Teste 2", -15);
        var categoriaDespesaId = await CriarCategoriaHelper("Lanche", 0);

        var transacao = new
        {
            Descricao = "Lanche Escola",
            Valor = 20.00,
            Tipo = 0,
            Data = DateTime.Today.ToString("yyyy-MM-dd"),
            PessoaId = pessoaMenorId,
            CategoriaId = categoriaDespesaId
        };


        var response = await _client.PostAsJsonAsync("/api/transacoes", transacao);

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "menores podem registrar despesas");
    }

    [Fact]
    public async Task CriarTransacao_ReceitaComMaiorDeIdade_DeveRetornarCreated()
    {

        var pessoaMaiorId = await CriarPessoaHelper("Maior Teste", -25);
        var categoriaReceitaId = await CriarCategoriaHelper("Freelance", 1);

        var transacao = new
        {
            Descricao = "Projeto Web",
            Valor = 1500.00,
            Tipo = 1,
            Data = DateTime.Today.ToString("yyyy-MM-dd"),
            PessoaId = pessoaMaiorId,
            CategoriaId = categoriaReceitaId
        };


        var response = await _client.PostAsJsonAsync("/api/transacoes", transacao);


        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "maiores de idade podem registrar receitas");
    }

    [Fact]
    public async Task CriarTransacao_ReceitaEmCategoriaDespesa_DeveRetornarBadRequest()
    {

        var pessoaId = await CriarPessoaHelper("Pessoa Teste", -30);
        var categoriaDespesaId = await CriarCategoriaHelper("Alimentação", 0);

        var transacao = new
        {
            Descricao = "Tentando receita em despesa",
            Valor = 500.00,
            Tipo = 1,
            Data = DateTime.Today.ToString("yyyy-MM-dd"),
            PessoaId = pessoaId,
            CategoriaId = categoriaDespesaId
        };

        var response = await _client.PostAsJsonAsync("/api/transacoes", transacao);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "não é possível registrar receita em categoria de despesa");
        
        var conteudo = await response.Content.ReadAsStringAsync();
        conteudo.Should().Contain("despesa");
    }

    [Fact]
    public async Task CriarTransacao_DespesaEmCategoriaReceita_DeveRetornarBadRequest()
    {

        var pessoaId = await CriarPessoaHelper("Pessoa Teste 2", -30);
        var categoriaReceitaId = await CriarCategoriaHelper("Salário Mensal", 1);

        var transacao = new
        {
            Descricao = "Tentando despesa em receita",
            Valor = 200.00,
            Tipo = 0,
            Data = DateTime.Today.ToString("yyyy-MM-dd"),
            PessoaId = pessoaId,
            CategoriaId = categoriaReceitaId
        };

        var response = await _client.PostAsJsonAsync("/api/transacoes", transacao);


        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "não é possível registrar despesa em categoria de receita");
        
        var conteudo = await response.Content.ReadAsStringAsync();
        conteudo.Should().Contain("receita");
    }

    [Fact]
    public async Task CriarTransacao_DespesaEmCategoriaAmbas_DeveRetornarCreated()
    {

        var pessoaId = await CriarPessoaHelper("Pessoa Teste 3", -30);
        var categoriaAmbasId = await CriarCategoriaHelper("Transferências", 2);

        var transacao = new
        {
            Descricao = "Transferência Saída",
            Valor = 300.00,
            Tipo = 0,
            Data = DateTime.Today.ToString("yyyy-MM-dd"),
            PessoaId = pessoaId,
            CategoriaId = categoriaAmbasId
        };

        var response = await _client.PostAsJsonAsync("/api/transacoes", transacao);

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "categoria Ambas deve aceitar despesas");
    }

    [Fact]
    public async Task CriarTransacao_ReceitaEmCategoriaAmbas_DeveRetornarCreated()
    {

        var pessoaId = await CriarPessoaHelper("Pessoa Teste 4", -30);
        var categoriaAmbasId = await CriarCategoriaHelper("Transferências Entrada", 2);

        var transacao = new
        {
            Descricao = "Transferência Entrada",
            Valor = 400.00,
            Tipo = 1,
            Data = DateTime.Today.ToString("yyyy-MM-dd"),
            PessoaId = pessoaId,
            CategoriaId = categoriaAmbasId
        };


        var response = await _client.PostAsJsonAsync("/api/transacoes", transacao);


        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "categoria Ambas deve aceitar receitas");
    }

    [Fact]
    public async Task ListarTransacoes_DeveRetornarListaPaginada()
    {

        var response = await _client.GetAsync("/api/transacoes?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var conteudo = await response.Content.ReadAsStringAsync();
        conteudo.Should().Contain("items");
    }

    [Fact]
    public async Task BuscarTransacaoPorId_TransacaoExistente_DeveRetornarTransacao()
    {

        var pessoaId = await CriarPessoaHelper("Pessoa Busca", -25);
        var categoriaId = await CriarCategoriaHelper("Categoria Busca", 0);
        var transacaoId = await CriarTransacaoHelper("Transação Teste", 100, 0, pessoaId, categoriaId);

        var response = await _client.GetAsync($"/api/transacoes/{transacaoId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var conteudo = await response.Content.ReadAsStringAsync();
        conteudo.Should().Contain("Transação Teste");
    }

    private async Task<Guid> CriarPessoaHelper(string nome, int anosAtras)
    {
        var pessoa = new
        {
            Nome = nome,
            DataNascimento = DateTime.Today.AddYears(anosAtras).ToString("yyyy-MM-dd")
        };

        var response = await _client.PostAsJsonAsync("/api/pessoas", pessoa);
        response.EnsureSuccessStatusCode();
        
        var conteudo = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(conteudo);
        return Guid.Parse(json.RootElement.GetProperty("id").GetString()!);
    }

    private async Task<Guid> CriarCategoriaHelper(string descricao, int finalidade)
    {
        var categoria = new
        {
            Descricao = descricao,
            Finalidade = finalidade
        };

        var response = await _client.PostAsJsonAsync("/api/categorias", categoria);
        response.EnsureSuccessStatusCode();
        
        var conteudo = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(conteudo);
        return Guid.Parse(json.RootElement.GetProperty("id").GetString()!);
    }

    private async Task<Guid> CriarTransacaoHelper(string descricao, decimal valor, int tipo, Guid pessoaId, Guid categoriaId)
    {
        var transacao = new
        {
            Descricao = descricao,
            Valor = valor,
            Tipo = tipo,
            Data = DateTime.Today.ToString("yyyy-MM-dd"),
            PessoaId = pessoaId,
            CategoriaId = categoriaId
        };

        var response = await _client.PostAsJsonAsync("/api/transacoes", transacao);
        response.EnsureSuccessStatusCode();
        
        var conteudo = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(conteudo);
        return Guid.Parse(json.RootElement.GetProperty("id").GetString()!);
    }
}
