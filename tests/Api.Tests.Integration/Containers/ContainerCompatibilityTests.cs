using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Containers;
using Domain.ContainerTypes;
using Domain.Products;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;

namespace Api.Tests.Integration.Containers;

public class ContainerCompatibilityTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "containers";

    private readonly ContainerType _restrictedContainerType = ContainerType.New("Restricted Box", "RESTRICT", "kg", null, null);
    private readonly ProductType _allowedProductType = ProductType.New("Allowed Type", 10, null, null, null);
    private readonly ProductType _disallowedProductType = ProductType.New("Disallowed Type", 10, null, null, null);
    
    private Product? _allowedProduct;
    private Product? _disallowedProduct;
    private Product? _shelfLifeProduct;
    private Container? _container;

    public ContainerCompatibilityTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ShouldFillContainer_WhenProductTypeIsAllowed()
    {
        // Arrange
        var request = new FillContainerDto
        {
            ProductId = _allowedProduct!.Id,
            Quantity = 10m,
            Unit = "kg",
            ProductionDate = DateTime.UtcNow,
            ExpirationDate = null // Should be calculated
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{BaseRoute}/{_container!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();
        container.CurrentProductId.Should().Be(_allowedProduct.Id);
    }

    [Fact]
    public async Task ShouldFailToFillContainer_WhenProductTypeIsNotAllowed()
    {
        // Arrange
        var request = new FillContainerDto
        {
            ProductId = _disallowedProduct!.Id,
            Quantity = 10m,
            Unit = "kg",
            ProductionDate = DateTime.UtcNow,
            ExpirationDate = null
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{BaseRoute}/{_container!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldCalculateExpirationDate_WhenShelfLifeIsDefined()
    {
        // Arrange
        var productionDate = DateTime.UtcNow.Date;
        var request = new FillContainerDto
        {
            ProductId = _shelfLifeProduct!.Id, // Shelf life 5 days (from product override)
            Quantity = 10m,
            Unit = "kg",
            ProductionDate = productionDate,
            ExpirationDate = null
        };

        // Act
        var response = await Client.PostAsync(
            $"{BaseRoute}/{_container!.Id}/empty", null); // Ensure empty first
            
        response = await Client.PostAsJsonAsync(
            $"{BaseRoute}/{_container!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbContainer = await Context.Containers
            .AsNoTracking()
            .FirstAsync(c => EF.Property<int>(c, "Id") == _container.Id);
            
        // Product shelf life is 5 days.
        dbContainer.CurrentExpirationDate.Should().Be(productionDate.AddDays(5));
    }
    
    [Fact]
    public async Task ShouldUseProvidedExpirationDate_EvenTheShelfLifeIsDefined()
    {
         // Arrange
        var productionDate = DateTime.UtcNow.Date;
        var manualExpiration = productionDate.AddDays(20);
        
        var request = new FillContainerDto
        {
            ProductId = _shelfLifeProduct!.Id, 
            Quantity = 10m,
            Unit = "kg",
            ProductionDate = productionDate,
            ExpirationDate = manualExpiration
        };

        // Act
        // Clear container if needed (re-using valid container from setup might be tricky if tests run in parallel or state persists? Integration tests usually reset DB or use transaction).
        // Since we implemented IAsyncLifetime and use BaseIntegrationTest, usually it means fresh DB or transaction per test depending on implementation.
        // But here I'm using class fields initialized in InitializeAsync.
        // For simplicity, let's create a new container for this test or empty it.
        await Client.PostAsync($"{BaseRoute}/{_container!.Id}/empty", null);

        var response = await Client.PostAsJsonAsync(
            $"{BaseRoute}/{_container!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbContainer = await Context.Containers
            .AsNoTracking()
            .FirstAsync(c => c.Id == _container.Id);
            
        // Should use manual date
        dbContainer.CurrentExpirationDate.Should().BeCloseTo(manualExpiration, TimeSpan.FromSeconds(1));
    }

    public async Task InitializeAsync()
    {
        // Setup Types
        await Context.ContainerTypes.AddAsync(_restrictedContainerType);
        await Context.ProductTypes.AddRangeAsync(_allowedProductType, _disallowedProductType);
        await SaveChangesAsync();

        // Setup Restricted Compatibility
        _restrictedContainerType.SetAllowedProductTypes(new[] { _allowedProductType });
        Context.ContainerTypes.Update(_restrictedContainerType);
        await SaveChangesAsync();

        // Setup Products
        _allowedProduct = Product.New("Allowed Product", null, _allowedProductType!.Id, null, null, Guid.Empty);
        _disallowedProduct = Product.New("Disallowed Product", null, _disallowedProductType.Id, null, null, Guid.Empty);
        
        // Product with specific shelf life (overriding type's shelf life which is 10)
        _shelfLifeProduct = Product.New("Shelf Life Product", null, _allowedProductType.Id, 5, null, Guid.Empty); 

        await Context.Products.AddRangeAsync(_allowedProduct, _disallowedProduct, _shelfLifeProduct);
        await SaveChangesAsync();

        // Setup Container
        _container = Container.New("BOX-001", "Test Box", 100m, "kg", _restrictedContainerType.Id, null, Guid.Empty);
        await Context.Containers.AddAsync(_container);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.ContainerFills.RemoveRange(Context.ContainerFills);
        Context.Containers.RemoveRange(Context.Containers);
        Context.Products.RemoveRange(Context.Products);
        Context.ContainerTypes.RemoveRange(Context.ContainerTypes);
        Context.ProductTypes.RemoveRange(Context.ProductTypes);
        await SaveChangesAsync();
    }
}
