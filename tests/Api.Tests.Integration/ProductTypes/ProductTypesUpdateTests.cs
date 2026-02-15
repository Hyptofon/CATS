using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Products;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.ProductTypes;

namespace Api.Tests.Integration.ProductTypes;

// Тести для оновлення типів продуктів (PUT endpoint)
public class ProductTypesUpdateTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "product-types";

    private readonly ProductType _firstTestProductType = ProductTypeData.FirstTestProductType();
    private readonly ProductType _secondTestProductType = ProductTypeData.SecondTestProductType();

    public ProductTypesUpdateTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен оновити тип продукту
    [Fact]
    public async Task ShouldUpdateProductType()
    {
        // Arrange
        var request = ProductTypeData.UpdateTestProductTypeDto();

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestProductType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productType = await response.ToResponseModel<ProductTypeDto>();

        productType.Name.Should().Be(request.Name);
        productType.ShelfLifeDays.Should().Be(request.ShelfLifeDays);
        productType.Meta.Should().Be(request.Meta);

        var dbProductType = await Context.ProductTypes
            .AsNoTracking()
            .FirstAsync(pt => pt.Id == _firstTestProductType.Id);
        dbProductType.Name.Should().Be(request.Name);
    }

    // Повинен оновити тип продукту на null ShelfLifeDays
    [Fact]
    public async Task ShouldUpdateProductTypeToNullShelfLifeDays()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString()[..8];
        var request = new UpdateProductTypeDto($"Updated-{uniqueId}", null, null, null);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestProductType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productType = await response.ToResponseModel<ProductTypeDto>();
        productType.ShelfLifeDays.Should().BeNull();
    }

    // Не повинен оновити неіснуючий тип продукту
    [Fact]
    public async Task ShouldNotUpdateProductTypeBecauseNotFound()
    {
        // Arrange
        var request = ProductTypeData.UpdateTestProductTypeDto();
        var nonExistentId = 999999;

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен оновити тип продукту з дублікатом назви
    [Fact]
    public async Task ShouldNotUpdateProductTypeBecauseDuplicateName()
    {
        // Arrange
        var request = new UpdateProductTypeDto(_secondTestProductType.Name, 60, null, "{\"updated\":\"meta\"}");

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestProductType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // Не повинен оновити тип продукту з порожньою назвою
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ShouldNotUpdateProductTypeBecauseEmptyName(string? name)
    {
        // Arrange
        var request = new UpdateProductTypeDto(name!, 30, null, null);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestProductType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен оновити тип продукту з від'ємним терміном придатності
    [Fact]
    public async Task ShouldNotUpdateProductTypeBecauseNegativeShelfLifeDays()
    {
        // Arrange
        var request = new UpdateProductTypeDto("Valid-Name", -1, null, null);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestProductType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public async Task InitializeAsync()
    {
        await Context.ProductTypes.AddRangeAsync(_firstTestProductType, _secondTestProductType);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.ProductTypes.RemoveRange(Context.ProductTypes);

        await SaveChangesAsync();
    }
}
