using System.Net;
using Api.Dtos;
using Domain.Products;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.Products;
using Tests.Data.ProductTypes;

namespace Api.Tests.Integration.ProductTypes;

// Тести для видалення типів продуктів (DELETE endpoint)
public class ProductTypesDeleteTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "product-types";

    private readonly ProductType _productTypeToDelete = ProductTypeData.FirstTestProductType();
    private readonly ProductType _productTypeInUse = ProductTypeData.SecondTestProductType();
    private Product? _productUsingType;

    public ProductTypesDeleteTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен видалити тип продукту
    [Fact]
    public async Task ShouldDeleteProductType()
    {
        // Arrange - none

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_productTypeToDelete.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productType = await response.ToResponseModel<ProductTypeDto>();
        productType.Id.Should().Be(_productTypeToDelete.Id);

        var dbProductType = await Context.ProductTypes
            .FirstOrDefaultAsync(pt => pt.Id == _productTypeToDelete.Id);
        dbProductType.Should().BeNull();
    }

    // Не повинен видалити неіснуючий тип продукту
    [Fact]
    public async Task ShouldNotDeleteProductTypeBecauseNotFound()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен видалити тип продукту з ID = 0
    [Fact]
    public async Task ShouldNotDeleteProductTypeBecauseZeroId()
    {
        // Arrange - none

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен видалити тип продукту, який використовується
    [Fact]
    public async Task ShouldNotDeleteProductTypeBecauseInUse()
    {
        // Arrange - product type is used by a product (created in InitializeAsync)

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_productTypeInUse.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // Видалення типу продукту не повинно вплинути на інші типи
    [Fact]
    public async Task ShouldDeleteProductTypeWithoutAffectingOthers()
    {
        // Arrange
        var otherProductTypeId = _productTypeInUse.Id;

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_productTypeToDelete.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var otherProductType = await Context.ProductTypes
            .FirstOrDefaultAsync(pt => pt.Id == otherProductTypeId);
        otherProductType.Should().NotBeNull();
    }

    public async Task InitializeAsync()
    {
        await Context.ProductTypes.AddRangeAsync(_productTypeToDelete, _productTypeInUse);
        await SaveChangesAsync();

        // Create a product that uses the second product type
        _productUsingType = ProductData.FirstTestProduct(_productTypeInUse.Id);
        await Context.Products.AddAsync(_productUsingType);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Products.RemoveRange(Context.Products);
        Context.ProductTypes.RemoveRange(Context.ProductTypes);

        await SaveChangesAsync();
    }
}
