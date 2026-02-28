using System.Net;
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

// Тести для пошуку записів наповнення контейнерів (GET containers/fills/search)
public class ContainersFillSearchTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "containers/fills/search";

    private readonly ContainerType _testContainerType = ContainerTypeData.FirstTestContainerType();
    private readonly ProductType _firstProductType = ProductTypeData.FirstTestProductType();
    private readonly ProductType _secondProductType = ProductTypeData.SecondTestProductType();
    private Container? _firstContainer;
    private Container? _secondContainer;
    private Product? _firstProduct;
    private Product? _secondProduct;
    private ContainerFill? _activeFill;
    private ContainerFill? _closedFill;

    public ContainersFillSearchTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен повернути всі записи наповнення без фільтрів
    [Fact]
    public async Task ShouldReturnAllFillsWhenNoFilters()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fills = await response.ToResponseModel<List<ContainerFillDto>>();
        fills.Should().HaveCount(2);
    }

    // Повинен знайти записи за ID продукту
    [Fact]
    public async Task ShouldSearchFillsByProductId()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}?productId={_firstProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fills = await response.ToResponseModel<List<ContainerFillDto>>();
        fills.Should().HaveCount(1);
        fills.Should().AllSatisfy(f => f.ProductId.Should().Be(_firstProduct.Id));
    }

    // Повинен знайти записи за ID контейнера
    [Fact]
    public async Task ShouldSearchFillsByContainerId()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}?containerId={_firstContainer!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fills = await response.ToResponseModel<List<ContainerFillDto>>();
        fills.Should().HaveCount(1);
        fills.Should().AllSatisfy(f => f.ContainerId.Should().Be(_firstContainer.Id));
    }

    // Повинен знайти записи за типом продукту
    [Fact]
    public async Task ShouldSearchFillsByProductTypeId()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}?productTypeId={_firstProductType.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fills = await response.ToResponseModel<List<ContainerFillDto>>();
        fills.Should().HaveCount(1);
        fills.Should().AllSatisfy(f => f.ProductId.Should().Be(_firstProduct!.Id));
    }

    // Повинен знайти записи тільки активні (ще не спорожнені)
    [Fact]
    public async Task ShouldSearchFillsOnlyActive()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}?onlyActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fills = await response.ToResponseModel<List<ContainerFillDto>>();
        fills.Should().HaveCount(1);
        fills.Should().AllSatisfy(f => f.EmptiedDate.Should().BeNull());
    }

    // Повинен знайти записи за діапазоном дат
    [Fact]
    public async Task ShouldSearchFillsByDateRange()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
        var toDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

        // Act
        var response = await Client.GetAsync($"{BaseRoute}?fromDate={fromDate}&toDate={toDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fills = await response.ToResponseModel<List<ContainerFillDto>>();
        fills.Should().NotBeEmpty();
    }

    // Повинен повернути порожній список якщо немає збігів
    [Fact]
    public async Task ShouldReturnEmptyWhenNoMatchingFills()
    {
        // Arrange
        var nonExistentProductId = 999999;

        // Act
        var response = await Client.GetAsync($"{BaseRoute}?productId={nonExistentProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fills = await response.ToResponseModel<List<ContainerFillDto>>();
        fills.Should().BeEmpty();
    }

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddAsync(_testContainerType);
        await Context.ProductTypes.AddRangeAsync(_firstProductType, _secondProductType);
        await SaveChangesAsync();

        _firstProduct = ProductData.FirstTestProduct(_firstProductType.Id);
        _secondProduct = ProductData.SecondTestProduct(_secondProductType.Id);
        await Context.Products.AddRangeAsync(_firstProduct, _secondProduct);
        await SaveChangesAsync();

        _firstContainer = ContainerData.FirstTestContainer(_testContainerType.Id);
        _secondContainer = ContainerData.SecondTestContainer(_testContainerType.Id);
        await Context.Containers.AddRangeAsync(_firstContainer, _secondContainer);
        await SaveChangesAsync();

        // Create a closed fill on first container (filled and emptied)
        _closedFill = ContainerFill.New(
            _firstContainer.Id,
            _firstProduct.Id,
            20m,
            "л",
            DateTime.UtcNow.AddDays(-15),
            DateTime.UtcNow.AddDays(15),
            MockUserId
        );
        await Context.ContainerFills.AddAsync(_closedFill);
        await SaveChangesAsync();

        _closedFill.Close(MockUserId);
        Context.ContainerFills.Update(_closedFill);
        await SaveChangesAsync();

        // Create an active fill on second container
        _activeFill = ContainerFill.New(
            _secondContainer.Id,
            _secondProduct.Id,
            30m,
            "л",
            DateTime.UtcNow.AddDays(-3),
            DateTime.UtcNow.AddDays(27),
            MockUserId
        );
        await Context.ContainerFills.AddAsync(_activeFill);
        await SaveChangesAsync();

        _secondContainer.Fill(
            _secondProduct.Id,
            _secondProductType.Id,
            _activeFill.Quantity,
            _activeFill.Unit,
            _activeFill.ProductionDate,
            _activeFill.ExpirationDate,
            _activeFill.Id,
            MockUserId
        );
        Context.Containers.Update(_secondContainer);
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
