using System.Net;
using Api.Dtos;
using Domain.Products;
using FluentAssertions;
using Tests.Common;
using Tests.Data.Products;
using Tests.Data.ProductTypes;

namespace Api.Tests.Integration.Products;

// Тести для пошуку продуктів (SEARCH endpoint)
public class ProductsSearchTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "products";

    private readonly ProductType _firstProductType = ProductTypeData.FirstTestProductType();
    private readonly ProductType _secondProductType = ProductTypeData.SecondTestProductType();
    private Product? _firstTestProduct;
    private Product? _secondTestProduct;

    public ProductsSearchTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен знайти продукти за пошуковим терміном
    [Fact]
    public async Task ShouldSearchProductsBySearchTerm()
    {
        // Arrange
        var searchTerm = "Product";

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search?searchTerm={searchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.ToResponseModel<List<ProductDto>>();
        products.Should().NotBeEmpty();
        products.Should().AllSatisfy(p => p.Name.Should().Contain("Product"));
    }

    // Повинен знайти продукти за типом продукту
    [Fact]
    public async Task ShouldSearchProductsByProductType()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search?productTypeId={_firstProductType.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.ToResponseModel<List<ProductDto>>();
        products.Should().HaveCount(1);
        products.Should().AllSatisfy(p => p.ProductTypeId.Should().Be(_firstProductType.Id));
    }

    // Повинен знайти продукти з кількома фільтрами
    [Fact]
    public async Task ShouldSearchProductsWithMultipleFilters()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(
            $"{BaseRoute}/search?searchTerm=Product&productTypeId={_firstProductType.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.ToResponseModel<List<ProductDto>>();
        products.Should().NotBeEmpty();
    }

    // Повинен знайти продукт за частковою назвою (case-insensitive)
    [Fact]
    public async Task ShouldSearchProductsCaseInsensitive()
    {
        // Arrange
        var searchTerm = "product"; // lowercase

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search?searchTerm={searchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.ToResponseModel<List<ProductDto>>();
        products.Should().NotBeEmpty();
    }

    // Повинен повернути всі продукти якщо фільтри порожні
    [Fact]
    public async Task ShouldReturnAllProductsWhenNoFiltersProvided()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.ToResponseModel<List<ProductDto>>();
        // Use AtLeast because other tests may leave data
        products.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    public async Task InitializeAsync()
    {
        await Context.ProductTypes.AddRangeAsync(_firstProductType, _secondProductType);
        await SaveChangesAsync();

        _firstTestProduct = ProductData.FirstTestProduct(_firstProductType.Id);
        _secondTestProduct = ProductData.SecondTestProduct(_secondProductType.Id);

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
