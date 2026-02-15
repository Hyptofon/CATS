using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Containers;
using Domain.ContainerTypes;
using Domain.Products;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;

namespace Api.Tests.Integration.Scenarios;

public class ExpirationScenarioTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "containers";
    
    private readonly ContainerType _testContainerType = ContainerType.New("Expiration Test Box", "EXP-BOX", "kg", null, null);
    private readonly ProductType _hoursProductType = ProductType.New("Hours Type", 0, 12, null, null); // 12 hours
    private readonly ProductType _daysProductType = ProductType.New("Days Type", 2, null, null, null); // 2 days
    private readonly ProductType _mixedProductType = ProductType.New("Mixed Type", 1, 6, null, null); // 1 day 6 hours

    private Product? _productHours;
    private Product? _productDays;
    private Product? _productMixed;
    private Product? _productOverride;
    
    private Container? _container;

    public ExpirationScenarioTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ShouldCalculateExpiration_WhenProductTypeHasHoursOnly()
    {
        // Arrange
        await ResetContainer();
        var productionDate = DateTime.UtcNow;
        var request = new FillContainerDto
        {
            ProductId = _productHours!.Id,
            Quantity = 10m,
            Unit = "kg",
            ProductionDate = productionDate,
            ExpirationDate = null
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseRoute}/{_container!.Id}/fill", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbContainer = await Context.Containers.AsNoTracking().FirstAsync(c => c.Id == _container.Id);
        // 12 hours
        dbContainer.CurrentExpirationDate.Should().BeCloseTo(productionDate.AddHours(12), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ShouldCalculateExpiration_WhenProductTypeHasDaysAndHours()
    {
        // Arrange
        await ResetContainer();
        var productionDate = DateTime.UtcNow;
        var request = new FillContainerDto
        {
            ProductId = _productMixed!.Id,
            Quantity = 10m,
            Unit = "kg",
            ProductionDate = productionDate,
            ExpirationDate = null
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseRoute}/{_container!.Id}/fill", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbContainer = await Context.Containers.AsNoTracking().FirstAsync(c => c.Id == _container.Id);
        // 1 day + 6 hours = 30 hours
        dbContainer.CurrentExpirationDate.Should().BeCloseTo(productionDate.AddDays(1).AddHours(6), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ShouldCalculateExpiration_WhenProductOverridesWithHours()
    {
        // Arrange
        await ResetContainer();
        // Product overrides Type (2 days) with (0 days, 5 hours)
        var productionDate = DateTime.UtcNow;
        var request = new FillContainerDto
        {
            ProductId = _productOverride!.Id,
            Quantity = 10m,
            Unit = "kg",
            ProductionDate = productionDate,
            ExpirationDate = null
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseRoute}/{_container!.Id}/fill", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbContainer = await Context.Containers.AsNoTracking().FirstAsync(c => c.Id == _container.Id);
        // 5 hours only
        dbContainer.CurrentExpirationDate.Should().BeCloseTo(productionDate.AddHours(5), TimeSpan.FromSeconds(2));
    }

    private async Task ResetContainer()
    {
        // Empty the container if it's full
        var container = await Context.Containers.AsNoTracking().FirstAsync(c => c.Id == _container!.Id);
        if (container.Status == ContainerStatus.Full)
        {
            await Client.PostAsync($"{BaseRoute}/{_container!.Id}/empty", null);
        }
    }

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddAsync(_testContainerType);
        await Context.ProductTypes.AddRangeAsync(_hoursProductType, _daysProductType, _mixedProductType);
        await SaveChangesAsync();

        _productHours = Product.New("Hours Product", null, _hoursProductType.Id, null, null, Guid.Empty);
        _productDays = Product.New("Days Product", null, _daysProductType.Id, null, null, Guid.Empty);
        _productMixed = Product.New("Mixed Product", null, _mixedProductType.Id, null, null, Guid.Empty);
        
        // Override: Type has 2 days, Product has 0 days, 5 hours
        _productOverride = Product.New("Override Product", null, _daysProductType.Id, 0, 5, Guid.Empty);

        await Context.Products.AddRangeAsync(_productHours, _productDays, _productMixed, _productOverride);
        await SaveChangesAsync();

        _container = Container.New("EXP-001", "Test Box", 100m, "kg", _testContainerType.Id, null, Guid.Empty);
        await Context.Containers.AddAsync(_container);
        await SaveChangesAsync();
        
        // Setup compatibility
        _testContainerType.SetAllowedProductTypes(new[] { _hoursProductType, _daysProductType, _mixedProductType });
        Context.ContainerTypes.Update(_testContainerType);
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
