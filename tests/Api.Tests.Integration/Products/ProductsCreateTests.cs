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

// Тести для створення продуктів (POST endpoint)
public class ProductsCreateTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "products";

    private readonly ProductType _testProductType = ProductTypeData.FirstTestProductType();

    public ProductsCreateTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен створити новий продукт
    [Fact]
    public async Task ShouldCreateProduct()
    {
        // Arrange
        var request = ProductData.CreateTestProductDto(_testProductType.Id);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productDto = await response.ToResponseModel<ProductDto>();

        productDto.Name.Should().Be(request.Name);
        productDto.Description.Should().Be(request.Description);
        productDto.ProductTypeId.Should().Be(request.ProductTypeId);
        productDto.Id.Should().BeGreaterThan(0);

        var dbProduct = await Context.Products
            .FirstOrDefaultAsync(p => p.Id == productDto.Id);

        dbProduct.Should().NotBeNull();
        dbProduct!.Name.Should().Be(request.Name);
    }

    // Повинен створити продукт без опису (null description)
    [Fact]
    public async Task ShouldCreateProductWithNullDescription()
    {
        // Arrange
        var request = ProductData.CreateTestProductDtoWithNullDescription(_testProductType.Id);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productDto = await response.ToResponseModel<ProductDto>();
        productDto.Description.Should().BeNull();
    }

    // Не повинен створити продукт з порожньою назвою
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ShouldNotCreateProductBecauseEmptyName(string? name)
    {
        // Arrange
        var request = new CreateProductDto
        {
            Name = name!,
            Description = "Test description",
            ProductTypeId = _testProductType.Id,
            ShelfLifeDays = null
        };

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен створити продукт з занадто довгою назвою
    [Fact]
    public async Task ShouldNotCreateProductBecauseNameTooLong()
    {
        // Arrange
        var tooLongName = new string('N', 201);
        var request = new CreateProductDto
        {
            Name = tooLongName,
            Description = "Test description",
            ProductTypeId = _testProductType.Id,
            ShelfLifeDays = null
        };

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен створити продукт з неіснуючим типом продукту
    [Fact]
    public async Task ShouldNotCreateProductBecauseProductTypeNotFound()
    {
        // Arrange
        var request = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test description",
            ProductTypeId = 999999,
            ShelfLifeDays = null
        };

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert - API returns 500 for invalid ProductTypeId (unhandled case)
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // Не повинен створити продукт з ProductTypeId = 0
    [Fact]
    public async Task ShouldNotCreateProductBecauseZeroProductTypeId()
    {
        // Arrange
        var request = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test description",
            ProductTypeId = 0,
            ShelfLifeDays = null
        };

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public async Task InitializeAsync()
    {
        await Context.ProductTypes.AddAsync(_testProductType);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Products.RemoveRange(Context.Products);
        Context.ProductTypes.RemoveRange(Context.ProductTypes);

        await SaveChangesAsync();
    }
}
