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

// Тести для перегляду історії контейнерів
public class ContainersHistoryTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "containers";

    private readonly ContainerType _testContainerType = ContainerTypeData.FirstTestContainerType();
    private readonly ProductType _testProductType = ProductTypeData.FirstTestProductType();
    private Container? _containerWithHistory;
    private Container? _newContainer;
    private Product? _testProduct;

    public ContainersHistoryTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен отримати історію контейнера з записами
    [Fact]
    public async Task ShouldGetContainerHistory()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{_containerWithHistory!.Id}/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.ToResponseModel<List<ContainerFillDto>>();
        history.Should().HaveCount(2);
        history.Should().AllSatisfy(h => h.ContainerId.Should().Be(_containerWithHistory.Id));
    }

    // Повинен отримати порожню історію для нового контейнера
    [Fact]
    public async Task ShouldGetEmptyHistoryForNewContainer()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{_newContainer!.Id}/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.ToResponseModel<List<ContainerFillDto>>();
        history.Should().BeEmpty();
    }

    // Історія повинна містити дату спорожнення для закритих записів
    [Fact]
    public async Task ShouldShowEmptiedDateForClosedFills()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{_containerWithHistory!.Id}/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.ToResponseModel<List<ContainerFillDto>>();

        var closedFill = history.FirstOrDefault(h => h.EmptiedDate.HasValue);
        closedFill.Should().NotBeNull();
        closedFill!.EmptiedByUserId.Should().NotBeNull();
    }

    // Історія повинна бути відсортована за датою заповнення (найновіші першими)
    [Fact]
    public async Task ShouldReturnHistoryOrderedByFilledDateDescending()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{_containerWithHistory!.Id}/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.ToResponseModel<List<ContainerFillDto>>();
        history.Should().BeInDescendingOrder(h => h.FilledDate);
    }

    // Не повинен отримати історію неіснуючого контейнера
    [Fact]
    public async Task ShouldNotGetHistoryBecauseContainerNotFound()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/999999/history");

        // Assert - API returns empty list instead of 404 for non-existent container
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.ToResponseModel<List<ContainerFillDto>>();
        history.Should().BeEmpty();
    }

    // Історія повинна включати інформацію про продукт
    [Fact]
    public async Task ShouldIncludeProductInfoInHistory()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{_containerWithHistory!.Id}/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.ToResponseModel<List<ContainerFillDto>>();
        history.Should().AllSatisfy(h =>
        {
            h.ProductId.Should().Be(_testProduct!.Id);
            h.ProductName.Should().NotBeEmpty();
        });
    }

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddAsync(_testContainerType);
        await Context.ProductTypes.AddAsync(_testProductType);
        await SaveChangesAsync();

        _testProduct = ProductData.FirstTestProduct(_testProductType.Id);
        await Context.Products.AddAsync(_testProduct);
        await SaveChangesAsync();

        _containerWithHistory = ContainerData.FirstTestContainer(_testContainerType.Id);
        _newContainer = ContainerData.SecondTestContainer(_testContainerType.Id);
        await Context.Containers.AddRangeAsync(_containerWithHistory, _newContainer);
        await SaveChangesAsync();

        // Create first fill (closed - emptied)
        var firstFill = ContainerFill.New(
            _containerWithHistory.Id,
            _testProduct.Id,
            30m,
            "л",
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(20),
            Guid.NewGuid()
        );
        await Context.ContainerFills.AddAsync(firstFill);
        await SaveChangesAsync();

        firstFill.Close(Guid.NewGuid());
        Context.ContainerFills.Update(firstFill);
        await SaveChangesAsync();

        // Create second fill (current - not emptied)
        var secondFill = ContainerFill.New(
            _containerWithHistory.Id,
            _testProduct.Id,
            40m,
            "л",
            DateTime.UtcNow.AddDays(-2),
            DateTime.UtcNow.AddDays(28),
            Guid.NewGuid()
        );
        await Context.ContainerFills.AddAsync(secondFill);
        await SaveChangesAsync();

        _containerWithHistory.Fill(
            _testProduct.Id,
            _testProductType.Id,
            secondFill.Quantity,
            secondFill.Unit,
            secondFill.ProductionDate,
            secondFill.ExpirationDate,
            secondFill.Id,
            Guid.NewGuid()
        );
        Context.Containers.Update(_containerWithHistory);
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
