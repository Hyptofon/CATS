using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Containers;
using Domain.ContainerTypes;
using Domain.Products;
using FluentAssertions;
using Tests.Common;
using Tests.Data.Containers;
using Tests.Data.ContainerTypes;
using Tests.Data.Products;
using Tests.Data.ProductTypes;

namespace Api.Tests.Integration.Containers;

// Тести для пошуку контейнерів (SEARCH endpoint)
public class ContainersSearchTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "containers";

    private readonly ContainerType _firstContainerType = ContainerTypeData.FirstTestContainerType();
    private readonly ContainerType _secondContainerType = ContainerTypeData.SecondTestContainerType();
    private readonly ProductType _testProductType = ProductTypeData.FirstTestProductType();
    private Container? _emptyContainer;
    private Container? _fullContainer;
    private Product? _testProduct;

    public ContainersSearchTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен знайти контейнери за пошуковим терміном
    [Fact]
    public async Task ShouldSearchContainersBySearchTerm()
    {
        // Arrange
        var searchTerm = "Container";

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search?searchTerm={searchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().NotBeEmpty();
    }

    // Повинен знайти контейнери за типом контейнера
    [Fact]
    public async Task ShouldSearchContainersByContainerType()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(
            $"{BaseRoute}/search?containerTypeId={_firstContainerType.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().AllSatisfy(c => c.ContainerTypeId.Should().Be(_firstContainerType.Id));
    }

    // Повинен знайти контейнери за статусом Empty
    [Fact]
    public async Task ShouldSearchContainersByStatusEmpty()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search?status=Empty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().AllSatisfy(c => c.Status.Should().Be("Empty"));
    }

    // Повинен знайти контейнери за статусом Full
    [Fact]
    public async Task ShouldSearchContainersByStatusFull()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search?status=Full");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().AllSatisfy(c => c.Status.Should().Be("Full"));
    }

    // Повинен знайти контейнери з кількома фільтрами
    [Fact]
    public async Task ShouldSearchContainersWithMultipleFilters()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(
            $"{BaseRoute}/search?searchTerm=Container&containerTypeId={_firstContainerType.Id}&status=Empty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().NotBeNull();
    }

    // Повинен знайти контейнери за ID продукту
    [Fact]
    public async Task ShouldSearchContainersByProductId()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search?currentProductId={_testProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().NotBeEmpty();
        containers.Should().AllSatisfy(c => c.CurrentProductId.Should().Be(_testProduct.Id));
    }

    // Повинен знайти контейнери за типом продукту
    [Fact]
    public async Task ShouldSearchContainersByProductTypeId()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(
            $"{BaseRoute}/search?currentProductTypeId={_testProductType.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().NotBeEmpty();
    }

    // Повинен повернути порожній список, якщо немає відповідних контейнерів
    [Fact]
    public async Task ShouldReturnEmptyWhenNoMatchingContainers()
    {
        // Arrange
        var nonExistentSearchTerm = "NonExistentContainerXYZ123";

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search?searchTerm={nonExistentSearchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().BeEmpty();
    }

    // Повинен повернути всі контейнери якщо фільтри порожні
    [Fact]
    public async Task ShouldReturnAllContainersWhenNoFiltersProvided()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().HaveCount(2);
    }

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddRangeAsync(_firstContainerType, _secondContainerType);
        await Context.ProductTypes.AddAsync(_testProductType);
        await SaveChangesAsync();

        _testProduct = ProductData.FirstTestProduct(_testProductType.Id);
        await Context.Products.AddAsync(_testProduct);
        await SaveChangesAsync();

        _emptyContainer = ContainerData.FirstTestContainer(_firstContainerType.Id);
        _fullContainer = ContainerData.SecondTestContainer(_firstContainerType.Id);

        await Context.Containers.AddRangeAsync(_emptyContainer, _fullContainer);
        await SaveChangesAsync();

        // Fill the second container to have it in Full status
        var containerFill = ContainerFill.New(
            _fullContainer.Id,
            _testProduct.Id,
            50m,
            "л",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            Guid.NewGuid()
        );
        await Context.ContainerFills.AddAsync(containerFill);
        await SaveChangesAsync();

        _fullContainer.Fill(
            _testProduct.Id,
            _testProductType.Id,
            containerFill.Quantity,
            containerFill.Unit,
            containerFill.ProductionDate,
            containerFill.ExpirationDate,
            containerFill.Id,
            Guid.NewGuid()
        );
        Context.Containers.Update(_fullContainer);
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
