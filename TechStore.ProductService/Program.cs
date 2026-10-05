using TechStore.ProductService;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(politica =>
    politica
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/produtos", async () =>
{
    var produtosBanco = new List<Produto>();
    using var conexao = new SqlConnection(connectionString);
    await conexao.OpenAsync();
    var sql = "SELECT Id, Nome, Descricao, Preco FROM Produtos ORDER BY Id";
    using var comando = new SqlCommand(sql, conexao);
    using var leitor = await comando.ExecuteReaderAsync();

    while (await leitor.ReadAsync())
    {
        var produto = new Produto
        {
            Id = leitor.GetInt32(0),
            Nome = leitor.GetString(1),
            Descricao = leitor.IsDBNull(2) ? string.Empty : leitor.GetString(2),
            Preco = leitor.GetDecimal(3)
        };

        produtosBanco.Add(produto);
    }

    return Results.Ok(produtosBanco);
});

app.MapGet("/api/produtos/{id}", async (int id) =>
{
    using var conexao = new SqlConnection(connectionString);
    await conexao.OpenAsync();

    var sql = @"
        SELECT Id, Nome, Descricao, Preco
        FROM Produtos
        WHERE Id = @Id;
    ";

    using var comando = new SqlCommand(sql, conexao);
    comando.Parameters.AddWithValue("@Id", id);

    using var leitor = await comando.ExecuteReaderAsync();

    if (!await leitor.ReadAsync())
    {
        return Results.NotFound();
    }

    var produto = new Produto
    {
        Id = leitor.GetInt32(0),
        Nome = leitor.GetString(1),
        Descricao = leitor.IsDBNull(2) ? string.Empty : leitor.GetString(2),
        Preco = leitor.GetDecimal(3)
    };

    return Results.Ok(produto);
});

app.MapPost("/api/produtos", async (Produto produto) =>
{
    using var conexao = new SqlConnection(connectionString);
    await conexao.OpenAsync();

    var sql = @"
        INSERT INTO Produtos (Nome, Descricao, Preco)
        OUTPUT INSERTED.Id
        VALUES (@Nome, @Descricao, @Preco);
    ";

    using var comando = new SqlCommand(sql, conexao);

    comando.Parameters.AddWithValue("@Nome", produto.Nome);
    comando.Parameters.AddWithValue("@Descricao", produto.Descricao);
    comando.Parameters.AddWithValue("@Preco", produto.Preco);

    var resultado = await comando.ExecuteScalarAsync();

    if (resultado == null)
    {
        return Results.Problem("Não foi possível cadastrar o produto.");
    }

    produto.Id = Convert.ToInt32(resultado);

    return Results.Created($"/api/produtos/{produto.Id}", produto);
});

app.MapPut("/api/produtos/{id}", async (int id, Produto produtoAtualizado) =>
{
    using var conexao = new SqlConnection(connectionString);
    await conexao.OpenAsync();

    var sql = @"
        UPDATE Produtos
        SET Nome = @Nome,
            Descricao = @Descricao,
            Preco = @Preco
        WHERE Id = @Id;
    ";

    using var comando = new SqlCommand(sql, conexao);

    comando.Parameters.AddWithValue("@Id", id);
    comando.Parameters.AddWithValue("@Nome", produtoAtualizado.Nome);
    comando.Parameters.AddWithValue("@Descricao", produtoAtualizado.Descricao);
    comando.Parameters.AddWithValue("@Preco", produtoAtualizado.Preco);

    var linhasAfetadas = await comando.ExecuteNonQueryAsync();

    if (linhasAfetadas == 0)
    {
        return Results.NotFound();
    }

    produtoAtualizado.Id = id;

    return Results.Ok(produtoAtualizado);
});

app.MapDelete("/api/produtos/{id}", async (int id) =>
{
    using var conexao = new SqlConnection(connectionString);
    await conexao.OpenAsync();

    var sql = "DELETE FROM Produtos WHERE Id = @Id;";

    using var comando = new SqlCommand(sql, conexao);
    comando.Parameters.AddWithValue("@Id", id);

    var linhasAfetadas = await comando.ExecuteNonQueryAsync();

    if (linhasAfetadas == 0)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
});

app.Run();