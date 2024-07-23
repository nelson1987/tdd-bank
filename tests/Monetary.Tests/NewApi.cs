using FluentAssertions;
using Monetary.Tests.Features.Units;

namespace Monetary.Tests;

public class Calculadora
{
    public int Adicionar(int a, int b)
    {
        return a + b;
    }
}

public class ProdutoControllerUnitTests : UnitTests
{
    private readonly Calculadora _sut;

    public ProdutoControllerUnitTests()
    {
        _sut = new Calculadora();
    }

    [Fact]
    public void Given_Valores_1_e_2_When_Parametros_1_E_2_Then_Resulta_Em_3()
    {
        // Arrange
        var valor = Given_Adicionar(1, 2);
        // Act
        var adicao = When_Parametros_1_E_2(valor.valor1, valor.valor2);
        // Assert
        Then_Resulta_Em_3(adicao, 3);
    }

    private (int valor1, int valor2) Given_Adicionar(int valor1, int valor2)
    {
        return (valor1, valor2);
    }

    private int When_Parametros_1_E_2(int valor1, int valor2)
    {
        return _sut.Adicionar(valor1, valor2);
    }

    private void Then_Resulta_Em_3(int adicao, int resultado)
    {
        adicao.Should().Be(resultado);
    }
}

/*
 Criar um serviço RESTful que permita que aplicações de client
gerenciem o catálogo de produtos do supermercado.
Ele precisa expor endpoints para criar, ler, editar e excluir categorias de produtos, como laticínios e cosméticos,
além de gerenciar produtos dessas categorias.
 */

public class Produto
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public Categoria Categoria { get; set; }
}

public class ProdutoService
{
    private readonly List<Produto> _produtos = new List<Produto>();

    public async Task<Produto> Create(Produto produto)
    {
        _produtos.Add(produto);
        return await Task.FromResult(produto);
    }

    public async Task<IEnumerable<Produto>> Read(Guid id)
    {
        return await Task.FromResult(_produtos.Where(x => x.Id == id));
    }

    public async Task<Produto> Update(Produto produto)
    {
        _produtos.RemoveAll(x => x.Id == produto.Id);
        _produtos.Add(produto);
        return await Task.FromResult(produto);
    }

    public async Task Delete(Guid id)
    {
        _produtos.RemoveAll(x => x.Id == id);
    }
}

public class CategoriaProductService
{
    public void Create()
    { }

    public void Read()
    { }

    public void Update()
    { }

    public void Delete()
    { }
}

public class Categoria
{
    public string Nome { get; set; }
}

/*
public class ProductCategoryServiceTests
{
    // Testar endpoints de criação, leitura, edição e exclusão de categorias de produtos
}

//Criar um novo endpoint para acrescentar um novo produto ao cat?logo de um determinado tipo de categoria.
//O endpoint precisa verificar se a categoria existe antes de adicionar o novo produto.

public class ProductServiceTests
{
    // Testar endpoints de cria??o, leitura, edi??o e exclus?o de produtos
}

Criar um novo endpoint para acrescentar um novo produto ao cat?logo de um determinado tipo de categoria.
O endpoint precisa verificar se a categoria existe antes de adicionar o novo produto.
 */