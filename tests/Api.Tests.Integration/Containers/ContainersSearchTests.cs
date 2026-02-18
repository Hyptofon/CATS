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
    private Container? _expiredContainer;
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
        containers.Should().HaveCount(3); // empty + full + expired
    }

    // Повинен знайти тільки прострочені контейнери (showExpired=true)
    [Fact]
    public async Task ShouldSearchContainersByShowExpiredTrue()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search?showExpired=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().NotBeEmpty();
        containers.Should().AllSatisfy(c =>
        {
            c.CurrentExpirationDate.Should().NotBeNull();
            c.CurrentExpirationDate!.Value.Should().BeBefore(DateTime.UtcNow);
        });
    }

    // showExpired=false не фільтрує — повертає всі контейнери (за поточною реалізацією)
    [Fact]
    public async Task ShouldSearchContainersByShowExpiredFalse()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search?showExpired=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().HaveCount(3);
    }

    // Повинен знайти контейнери за датою виробництва
    [Fact]
    public async Task ShouldSearchContainersByProductionDate()
    {
        // Arrange - use today's date since the full container was filled with ProductionDate = DateTime.UtcNow
        var productionDate = DateTime.UtcNow.Date.ToString("O");

        // Act
        var response = await Client.GetAsync(
            $"{BaseRoute}/search?productionDate={Uri.EscapeDataString(productionDate)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().NotBeEmpty();
        containers.Should().AllSatisfy(c =>
            c.CurrentProductionDate.Should().NotBeNull());
    }

    // Повинен знайти контейнери, наповнені сьогодні (filledToday)
    [Fact]
    public async Task ShouldSearchContainersByFilledToday()
    {
        // Arrange - use today's date since the full container was filled today
        var filledDate = DateTime.UtcNow.Date.ToString("O");

        // Act
        var response = await Client.GetAsync(
            $"{BaseRoute}/search?filledToday={Uri.EscapeDataString(filledDate)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().NotBeEmpty();
        containers.Should().AllSatisfy(c =>
            c.CurrentFilledAt.Should().NotBeNull());
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

        // Third container for expired product fill
        _expiredContainer = Container.New(
            $"QR-EXPIRED-{Guid.NewGuid().ToString()[..8]}",
            $"Test-Expired-Container-{Guid.NewGuid().ToString()[..8]}",
            60.0m,
            "л",
            _firstContainerType.Id,
            null,
            new Guid("00000000-0000-0000-0000-000000000001")
        );

        await Context.Containers.AddRangeAsync(_emptyContainer, _fullContainer, _expiredContainer);
        await SaveChangesAsync();

        // Fill the second container (non-expired, filled today)
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

        // Fill the third container with expired product
        var expiredFill = ContainerFill.New(
            _expiredContainer.Id,
            _testProduct.Id,
            40m,
            "л",
            DateTime.UtcNow.AddDays(-60),
            DateTime.UtcNow.AddDays(-10), // already expired
            Guid.NewGuid()
        );
        await Context.ContainerFills.AddAsync(expiredFill);
        await SaveChangesAsync();

        _expiredContainer.Fill(
            _testProduct.Id,
            _testProductType.Id,
            expiredFill.Quantity,
            expiredFill.Unit,
            expiredFill.ProductionDate,
            expiredFill.ExpirationDate,
            expiredFill.Id,
            Guid.NewGuid()
        );
        Context.Containers.Update(_expiredContainer);
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
