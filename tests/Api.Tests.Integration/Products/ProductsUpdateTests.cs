using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Products;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.Products;
using Tests.Data.ProductTypes;

namespace Api.Tests.Integration.Products;

// Тести для оновлення продуктів (PUT endpoint)
public class ProductsUpdateTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "products";

    private readonly ProductType _firstProductType = ProductTypeData.FirstTestProductType();
    private readonly ProductType _secondProductType = ProductTypeData.SecondTestProductType();
    private Product? _testProduct;

    public ProductsUpdateTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен оновити продукт
    [Fact]
    public async Task ShouldUpdateProduct()
    {
        // Arrange
        var request = ProductData.UpdateTestProductDto(_firstProductType.Id);

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{_testProduct!.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.ToResponseModel<ProductDto>();

        product.Name.Should().Be(request.Name);
        product.Description.Should().Be(request.Description);
        product.ProductTypeId.Should().Be(request.ProductTypeId);

        var dbProduct = await Context.Products
            .AsNoTracking()
            .FirstAsync(p => p.Id == _testProduct.Id);
        dbProduct.Name.Should().Be(request.Name);
    }

    // Повинен оновити продукт зі зміною типу продукту
    [Fact]
    public async Task ShouldUpdateProductWithDifferentProductType()
    {
        // Arrange
        var request = ProductData.UpdateTestProductDto(_secondProductType.Id);

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{_testProduct!.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.ToResponseModel<ProductDto>();
        product.ProductTypeId.Should().Be(_secondProductType.Id);
    }

    // Повинен оновити опис продукту на null
    [Fact]
    public async Task ShouldUpdateProductDescriptionToNull()
    {
        // Arrange
        var request = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = null,
            ProductTypeId = _firstProductType.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{_testProduct!.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.ToResponseModel<ProductDto>();
        product.Description.Should().BeNull();
    }

    // Не повинен оновити неіснуючий продукт
    [Fact]
    public async Task ShouldNotUpdateProductBecauseNotFound()
    {
        // Arrange
        var request = ProductData.UpdateTestProductDto(_firstProductType.Id);
        var nonExistentId = 999999;

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен оновити продукт з неіснуючим типом продукту
    [Fact]
    public async Task ShouldNotUpdateProductBecauseProductTypeNotFound()
    {
        // Arrange
        var request = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Test",
            ProductTypeId = 999999
        };

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{_testProduct!.Id}", request);

        // Assert - API returns 500 for invalid ProductTypeId (unhandled)
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // Не повинен оновити продукт з порожньою назвою
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ShouldNotUpdateProductBecauseEmptyName(string? name)
    {
        // Arrange
        var request = new UpdateProductDto
        {
            Name = name!,
            Description = "Test",
            ProductTypeId = _firstProductType.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{_testProduct!.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен оновити продукт з занадто довгою назвою
    [Fact]
    public async Task ShouldNotUpdateProductBecauseNameTooLong()
    {
        // Arrange
        var tooLongName = new string('N', 201);
        var request = new UpdateProductDto
        {
            Name = tooLongName,
            Description = "Test",
            ProductTypeId = _firstProductType.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{_testProduct!.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public async Task InitializeAsync()
    {
        await Context.ProductTypes.AddRangeAsync(_firstProductType, _secondProductType);
        await SaveChangesAsync();

        _testProduct = ProductData.FirstTestProduct(_firstProductType.Id);
        await Context.Products.AddAsync(_testProduct);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Products.RemoveRange(Context.Products);
        Context.ProductTypes.RemoveRange(Context.ProductTypes);

        await SaveChangesAsync();
    }
}
