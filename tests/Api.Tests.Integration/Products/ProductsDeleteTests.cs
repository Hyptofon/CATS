using System.Net;
using Api.Dtos;
using Domain.Containers;
using Domain.ContainerTypes;
using Domain.Products;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.ContainerTypes;
using Tests.Data.Products;
using Tests.Data.ProductTypes;

namespace Api.Tests.Integration.Products;

// Тести для видалення продуктів (DELETE endpoint)
public class ProductsDeleteTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "products";

    private readonly ProductType _testProductType = ProductTypeData.FirstTestProductType();
    private readonly ContainerType _testContainerType = ContainerTypeData.FirstTestContainerType();
    private Product? _productToDelete;
    private Product? _productInUse;
    private Container? _containerWithProduct;

    public ProductsDeleteTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен видалити продукт
    [Fact]
    public async Task ShouldDeleteProduct()
    {
        // Arrange - none

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_productToDelete!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.ToResponseModel<ProductDto>();
        product.Id.Should().Be(_productToDelete.Id);

        var dbProduct = await Context.Products
            .FirstOrDefaultAsync(p => p.Id == _productToDelete.Id);
        dbProduct.Should().BeNull();
    }

    // Не повинен видалити неіснуючий продукт
    [Fact]
    public async Task ShouldNotDeleteProductBecauseNotFound()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен видалити продукт з ID = 0
    [Fact]
    public async Task ShouldNotDeleteProductBecauseZeroId()
    {
        // Arrange - none

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен видалити продукт, який використовується в ContainerFill
    [Fact]
    public async Task ShouldNotDeleteProductBecauseInUse()
    {
        // Arrange - product is used in ContainerFill (created in InitializeAsync)

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_productInUse!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public async Task InitializeAsync()
    {
        await Context.ProductTypes.AddAsync(_testProductType);
        await Context.ContainerTypes.AddAsync(_testContainerType);
        await SaveChangesAsync();

        _productToDelete = ProductData.FirstTestProduct(_testProductType.Id);
        _productInUse = ProductData.SecondTestProduct(_testProductType.Id);

        await Context.Products.AddRangeAsync(_productToDelete, _productInUse);
        await SaveChangesAsync();

        // Create container and fill it with product to test deletion protection
        _containerWithProduct = Container.New(
            $"QR-Test-{Guid.NewGuid().ToString()[..8]}",
            "Container with product",
            100m,
            "л",
            _testContainerType.Id,
            null,
            Guid.NewGuid()
        );
        await Context.Containers.AddAsync(_containerWithProduct);
        await SaveChangesAsync();

        // Create ContainerFill to link product to container
        var containerFill = ContainerFill.New(
            _containerWithProduct.Id,
            _productInUse.Id,
            50m,
            "л",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            Guid.NewGuid()
        );
        await Context.ContainerFills.AddAsync(containerFill);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.ContainerFills.RemoveRange(Context.ContainerFills);
        Context.Containers.RemoveRange(Context.Containers);
        Context.Products.RemoveRange(Context.Products);
        Context.ProductTypes.RemoveRange(Context.ProductTypes);
        Context.ContainerTypes.RemoveRange(Context.ContainerTypes);

        await SaveChangesAsync();
    }
}
