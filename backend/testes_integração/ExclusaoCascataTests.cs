using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace MinhasFinancas.IntegrationTests.Controllers;

public class ExclusaoCascataTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExclusaoCascataTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeletarPessoa_ComTransacoes_DeveExcluirTransacoesEmCascata()
    {

        var pessoaId = await CriarPessoaHelper("Pessoa Com Transações", -30);
        var categoriaId = await CriarCategoriaHelper("Teste Cascata", 0);

        var transacao1Id = await CriarTransacaoHelper("Transação 1", 100, 0, pessoaId, categoriaId);
        var transacao2Id = await CriarTransacaoHelper("Transação 2", 200, 0, pessoaId, categoriaId);
        var transacao3Id = await CriarTransacaoHelper("Transação 3", 300, 0, pessoaId, categoriaId);

        var responseT1 = await _client.GetAsync($"/api/transacoes/{transacao1Id}");
        responseT1.StatusCode.Should().Be(HttpStatusCode.OK, "transação 1 deve existir antes da exclusão");

        var response = await _client.DeleteAsync($"/api/pessoas/{pessoaId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, "pessoa deve ser deletada com sucesso");

        var responsePessoa = await _client.GetAsync($"/api/pessoas/{pessoaId}");
        responsePessoa.StatusCode.Should().Be(HttpStatusCode.NotFound, "pessoa não deve mais existir");

        var responseTransacao1 = await _client.GetAsync($"/api/transacoes/{transacao1Id}");
        var responseTransacao2 = await _client.GetAsync($"/api/transacoes/{transacao2Id}");
        var responseTransacao3 = await _client.GetAsync($"/api/transacoes/{transacao3Id}");

        responseTransacao1.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "transação 1 deve ser deletada em cascata");
        responseTransacao2.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "transação 2 deve ser deletada em cascata");
        responseTransacao3.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "transação 3 deve ser deletada em cascata");
    }

    [Fact]
    public async Task DeletarPessoa_SemTransacoes_DeveDeletarComSucesso()
    {

        var pessoaId = await CriarPessoaHelper("Pessoa Sem Transações", -25);

        var response = await _client.DeleteAsync($"/api/pessoas/{pessoaId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var responseGet = await _client.GetAsync($"/api/pessoas/{pessoaId}");
        responseGet.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletarCategoria_ComTransacoes_DeveValidarOuImpedirExclusao()
    {

        var pessoaId = await CriarPessoaHelper("Pessoa Teste Cat", -30);
        var categoriaId = await CriarCategoriaHelper("Categoria Com Transações", 0);
        
        await CriarTransacaoHelper("Transação Cat", 100, 0, pessoaId, categoriaId);


        var response = await _client.DeleteAsync($"/api/categorias/{categoriaId}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NoContent,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Conflict,
            "categoria pode ser deletada em cascata OU impedir exclusão");
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
