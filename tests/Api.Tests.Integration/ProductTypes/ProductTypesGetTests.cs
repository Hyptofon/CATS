using System.Net;
using Api.Dtos;
using Domain.Products;
using FluentAssertions;
using Tests.Common;
using Tests.Data.ProductTypes;

namespace Api.Tests.Integration.ProductTypes;

// Тести для отримання типів продуктів (GET endpoints)
public class ProductTypesGetTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "product-types";

    private readonly ProductType _firstTestProductType = ProductTypeData.FirstTestProductType();
    private readonly ProductType _secondTestProductType = ProductTypeData.SecondTestProductType();

    public ProductTypesGetTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен отримати всі типи продуктів
    [Fact]
    public async Task ShouldGetAllProductTypes()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productTypes = await response.ToResponseModel<List<ProductTypeDto>>();
        productTypes.Should().HaveCount(2);
        productTypes.Should().Contain(pt => pt.Id == _firstTestProductType.Id);
        productTypes.Should().Contain(pt => pt.Id == _secondTestProductType.Id);
    }

    // Повинен отримати всі типи продуктів відсортовані за назвою
    [Fact]
    public async Task ShouldGetAllProductTypesOrderedByName()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productTypes = await response.ToResponseModel<List<ProductTypeDto>>();
        productTypes.Should().BeInAscendingOrder(pt => pt.Name);
    }

    // Повинен отримати тип продукту за ID
    [Fact]
    public async Task ShouldGetProductTypeById()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{_firstTestProductType.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productType = await response.ToResponseModel<ProductTypeDto>();
        productType.Id.Should().Be(_firstTestProductType.Id);
        productType.Name.Should().Be(_firstTestProductType.Name);
        productType.ShelfLifeDays.Should().Be(_firstTestProductType.ShelfLifeDays);
        productType.Meta.Should().Be(_firstTestProductType.Meta);
    }

    // Не повинен знайти тип продукту за неіснуючим ID
    [Fact]
    public async Task ShouldNotGetProductTypeByIdBecauseNotFound()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен отримати тип продукту з ID = 0
    [Fact]
    public async Task ShouldNotGetProductTypeByIdBecauseZeroId()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
