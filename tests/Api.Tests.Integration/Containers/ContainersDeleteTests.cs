using System.Net;
using Api.Dtos;
using Domain.Containers;
using Domain.ContainerTypes;
using Domain.Products;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.Containers;
using Tests.Data.ContainerTypes;
using Tests.Data.Products;
using Tests.Data.ProductTypes;

namespace Api.Tests.Integration.Containers;

// Тести для видалення контейнерів (DELETE endpoint)
public class ContainersDeleteTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "containers";

    private readonly ContainerType _testContainerType = ContainerTypeData.FirstTestContainerType();
    private readonly ProductType _testProductType = ProductTypeData.FirstTestProductType();
    private Container? _containerToDelete;
    private Container? _containerToKeep;
    private Container? _containerWithFills;
    private Product? _testProduct;

    public ContainersDeleteTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен видалити контейнер
    [Fact]
    public async Task ShouldDeleteContainer()
    {
        // Arrange - none

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_containerToDelete!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();
        container.Id.Should().Be(_containerToDelete.Id);

        var dbContainer = await Context.Containers
            .FirstOrDefaultAsync(c => c.Id == _containerToDelete.Id);
        dbContainer.Should().BeNull();
    }

    // Не повинен видалити неіснуючий контейнер
    [Fact]
    public async Task ShouldNotDeleteContainerBecauseNotFound()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен видалити контейнер з ID = 0
    [Fact]
    public async Task ShouldNotDeleteContainerBecauseZeroId()
    {
        // Arrange - none

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Видалення контейнера не повинно вплинути на інші контейнери
    [Fact]
    public async Task ShouldDeleteContainerWithoutAffectingOthers()
    {
        // Arrange
        var otherContainerId = _containerToKeep!.Id;

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_containerToDelete!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var otherContainer = await Context.Containers
            .FirstOrDefaultAsync(c => c.Id == otherContainerId);
        otherContainer.Should().NotBeNull();
    }

    // Не повинен видалити контейнер з історією наповнень (FK Restrict)
    [Fact]
    public async Task ShouldNotDeleteContainerWithFillHistory()
    {
        // Arrange - container with fills is created in InitializeAsync

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_containerWithFills!.Id}");

        // Assert - should fail because ContainerFill FK has DeleteBehavior.Restrict
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);

        // Verify container still exists
        var dbContainer = await Context.Containers
            .FirstOrDefaultAsync(c => c.Id == _containerWithFills.Id);
        dbContainer.Should().NotBeNull();
    }

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddAsync(_testContainerType);
        await Context.ProductTypes.AddAsync(_testProductType);
        await SaveChangesAsync();

        _testProduct = ProductData.FirstTestProduct(_testProductType.Id);
        await Context.Products.AddAsync(_testProduct);
        await SaveChangesAsync();

        _containerToDelete = ContainerData.FirstTestContainer(_testContainerType.Id);
        _containerToKeep = ContainerData.SecondTestContainer(_testContainerType.Id);
        _containerWithFills = Container.New(
            $"QR-FILLS-{Guid.NewGuid().ToString()[..8]}",
            $"Test-Container-WithFills-{Guid.NewGuid().ToString()[..8]}",
            50.0m,
            "л",
            _testContainerType.Id,
            null,
            new Guid("00000000-0000-0000-0000-000000000001")
        );

        await Context.Containers.AddRangeAsync(_containerToDelete, _containerToKeep, _containerWithFills);
        await SaveChangesAsync();

        // Create a fill record for _containerWithFills
        var fill = ContainerFill.New(
            _containerWithFills.Id,
            _testProduct.Id,
            30m,
            "л",
            DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow.AddDays(25),
            Guid.NewGuid()
        );
        await Context.ContainerFills.AddAsync(fill);
        await SaveChangesAsync();

        // Close the fill (empty the container) so it has history
        fill.Close(Guid.NewGuid());
        Context.ContainerFills.Update(fill);
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

