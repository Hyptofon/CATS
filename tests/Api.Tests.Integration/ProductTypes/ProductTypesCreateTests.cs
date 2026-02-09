using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Products;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.ProductTypes;

namespace Api.Tests.Integration.ProductTypes;

// Тести для створення типів продуктів (POST endpoint)
public class ProductTypesCreateTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "product-types";

    private readonly ProductType _existingProductType = ProductTypeData.FirstTestProductType();

    public ProductTypesCreateTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен створити новий тип продукту
    [Fact]
    public async Task ShouldCreateProductType()
    {
        // Arrange
        var request = ProductTypeData.CreateTestProductTypeDto();

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productTypeDto = await response.ToResponseModel<ProductTypeDto>();

        productTypeDto.Name.Should().Be(request.Name);
        productTypeDto.ShelfLifeDays.Should().Be(request.ShelfLifeDays);
        productTypeDto.Meta.Should().Be(request.Meta);
        productTypeDto.Id.Should().BeGreaterThan(0);

        var dbProductType = await Context.ProductTypes
            .FirstOrDefaultAsync(pt => pt.Id == productTypeDto.Id);

        dbProductType.Should().NotBeNull();
        dbProductType!.Name.Should().Be(request.Name);
    }

    // Повинен створити тип продукту без терміну придатності
    [Fact]
    public async Task ShouldCreateProductTypeWithNullShelfLifeDays()
    {
        // Arrange
        var request = new CreateProductTypeDto("Test-NoShelfLife", null, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productTypeDto = await response.ToResponseModel<ProductTypeDto>();
        productTypeDto.ShelfLifeDays.Should().BeNull();
    }

    // Повинен створити тип продукту без мета-даних
    [Fact]
    public async Task ShouldCreateProductTypeWithNullMeta()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString()[..8];
        var request = new CreateProductTypeDto($"Test-{uniqueId}-NoMeta", 30, null);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productTypeDto = await response.ToResponseModel<ProductTypeDto>();
        productTypeDto.Meta.Should().BeNull();
    }

    // Не повинен створити тип продукту з дублікатом назви
    [Fact]
    public async Task ShouldNotCreateProductTypeBecauseDuplicateName()
    {
        // Arrange
        var request = new CreateProductTypeDto(_existingProductType.Name, 30, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // Не повинен створити тип продукту з порожньою назвою
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ShouldNotCreateProductTypeBecauseEmptyName(string? name)
    {
        // Arrange
        var request = new CreateProductTypeDto(name!, 30, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен створити тип продукту з від'ємним терміном придатності
    [Fact]
    public async Task ShouldNotCreateProductTypeBecauseNegativeShelfLifeDays()
    {
        // Arrange
        var request = new CreateProductTypeDto("Test-Negative", -1, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен створити тип продукту з занадто довгою назвою
    [Fact]
    public async Task ShouldNotCreateProductTypeBecauseNameTooLong()
    {
        // Arrange
        var tooLongName = new string('N', 101);
        var request = new CreateProductTypeDto(tooLongName, 30, null);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public async Task InitializeAsync()
    {
        await Context.ProductTypes.AddAsync(_existingProductType);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.ProductTypes.RemoveRange(Context.ProductTypes);

        await SaveChangesAsync();
    }
}
