using System.Net;
using Api.Dtos;
using Domain.Products;
using FluentAssertions;
using Tests.Common;
using Tests.Data.Products;
using Tests.Data.ProductTypes;

namespace Api.Tests.Integration.Products;

// Тести для отримання продуктів (GET endpoints)
public class ProductsGetTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "products";

    private readonly ProductType _testProductType = ProductTypeData.FirstTestProductType();
    private Product? _firstTestProduct;
    private Product? _secondTestProduct;

    public ProductsGetTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен отримати всі продукти
    [Fact]
    public async Task ShouldGetAllProducts()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.ToResponseModel<List<ProductDto>>();
        products.Should().HaveCount(2);
        products.Should().Contain(p => p.Id == _firstTestProduct!.Id);
        products.Should().Contain(p => p.Id == _secondTestProduct!.Id);
    }

    // Повинен отримати всі продукти відсортовані за назвою
    [Fact]
    public async Task ShouldGetAllProductsOrderedByName()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.ToResponseModel<List<ProductDto>>();
        products.Should().BeInAscendingOrder(p => p.Name);
    }

    // Повинен отримати продукт за ID
    [Fact]
    public async Task ShouldGetProductById()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{_firstTestProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.ToResponseModel<ProductDto>();
        product.Id.Should().Be(_firstTestProduct.Id);
        product.Name.Should().Be(_firstTestProduct.Name);
        product.Description.Should().Be(_firstTestProduct.Description);
        product.ProductTypeId.Should().Be(_testProductType.Id);
        product.ProductTypeName.Should().Be(_testProductType.Name);
    }

    // Не повинен знайти продукт за неіснуючим ID
    [Fact]
    public async Task ShouldNotGetProductByIdBecauseNotFound()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен отримати продукт з ID = 0
    [Fact]
    public async Task ShouldNotGetProductByIdBecauseZeroId()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public async Task InitializeAsync()
    {
        await Context.ProductTypes.AddAsync(_testProductType);
        await SaveChangesAsync();

        _firstTestProduct = ProductData.FirstTestProduct(_testProductType.Id);
        _secondTestProduct = ProductData.SecondTestProduct(_testProductType.Id);

        await Context.Products.AddRangeAsync(_firstTestProduct, _secondTestProduct);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Products.RemoveRange(Context.Products);
        Context.ProductTypes.RemoveRange(Context.ProductTypes);

        await SaveChangesAsync();
    }
}
