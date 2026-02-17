using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace MinhasFinancas.IntegrationTests.Controllers;

public class PessoasControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public PessoasControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task CriarPessoa_ComDadosValidos_DeveRetornarCreated()
    {

        var novaPessoa = new
        {
            Nome = "João Silva",
            DataNascimento = DateTime.Today.AddYears(-25).ToString("yyyy-MM-dd")
        };


        var response = await _client.PostAsJsonAsync("/api/pessoas", novaPessoa);


        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var resultado = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task CriarPessoa_MenorDeIdade_DeveCriarComSucesso()
    {

        var novaPessoa = new
        {
            Nome = "Maria Menor",
            DataNascimento = DateTime.Today.AddYears(-15).ToString("yyyy-MM-dd")
        };


        var response = await _client.PostAsJsonAsync("/api/pessoas", novaPessoa);

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "sistema deve permitir cadastro de menores de idade");
    }

    [Fact]
    public async Task ListarPessoas_DeveRetornarListaPaginada()
    {

        await CriarPessoaHelper("Pessoa Teste 1", -20);
        await CriarPessoaHelper("Pessoa Teste 2", -30);


        var response = await _client.GetAsync("/api/pessoas?pageNumber=1&pageSize=10");


        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var conteudo = await response.Content.ReadAsStringAsync();
        conteudo.Should().Contain("items");
    }

    [Fact]
    public async Task BuscarPessoaPorId_PessoaExistente_DeveRetornarPessoa()
    {

        var pessoaId = await CriarPessoaHelper("Pessoa Para Buscar", -28);


        var response = await _client.GetAsync($"/api/pessoas/{pessoaId}");


        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var conteudo = await response.Content.ReadAsStringAsync();
        conteudo.Should().Contain("Pessoa Para Buscar");
    }

    [Fact]
    public async Task BuscarPessoaPorId_PessoaInexistente_DeveRetornarNotFound()
    {

        var idInexistente = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/pessoas/{idInexistente}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AtualizarPessoa_ComDadosValidos_DeveRetornarNoContent()
    {

        var pessoaId = await CriarPessoaHelper("Pessoa Original", -25);
        
        var pessoaAtualizada = new
        {
            Nome = "Pessoa Atualizada",
            DataNascimento = DateTime.Today.AddYears(-26).ToString("yyyy-MM-dd")
        };

        var response = await _client.PutAsJsonAsync($"/api/pessoas/{pessoaId}", pessoaAtualizada);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var responseGet = await _client.GetAsync($"/api/pessoas/{pessoaId}");
        var conteudo = await responseGet.Content.ReadAsStringAsync();
        conteudo.Should().Contain("Pessoa Atualizada");
    }

    [Fact]
    public async Task DeletarPessoa_PessoaExistente_DeveRetornarNoContent()
    {

        var pessoaId = await CriarPessoaHelper("Pessoa Para Deletar", -30);

        var response = await _client.DeleteAsync($"/api/pessoas/{pessoaId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var responseGet = await _client.GetAsync($"/api/pessoas/{pessoaId}");
        responseGet.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CriarPessoa_SemNome_DeveRetornarBadRequest()
    {

        var novaPessoa = new
        {
            Nome = "",
            DataNascimento = DateTime.Today.AddYears(-25).ToString("yyyy-MM-dd")
        };

        var response = await _client.PostAsJsonAsync("/api/pessoas", novaPessoa);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
}
