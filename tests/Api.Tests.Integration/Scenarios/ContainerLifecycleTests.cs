using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.ContainerTypes;
using Domain.Products;
using FluentAssertions;
using Tests.Common;
using Tests.Data.ContainerTypes;
using Tests.Data.Products;
using Tests.Data.ProductTypes;

namespace Api.Tests.Integration.Scenarios;

// Тести для повного життєвого циклу контейнера:
// Створення → Наповнення → Оновлення наповнення → Спорожнення → Повторне наповнення → Перевірка історії
public class ContainerLifecycleTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "containers";

    private readonly ContainerType _testContainerType = ContainerTypeData.FirstTestContainerType();
    private readonly ProductType _testProductType = ProductTypeData.FirstTestProductType();
    private Product? _testProduct;
    private Product? _secondProduct;

    public ContainerLifecycleTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повний життєвий цикл контейнера від створення до перевірки історії
    [Fact]
    public async Task ShouldCompleteFullContainerLifecycle()
    {
        // === STEP 1: Create container ===
        var createDto = new CreateContainerDto
        {
            Code = $"QR-LIFECYCLE-{Guid.NewGuid().ToString()[..8]}",
            Name = "Lifecycle Test Container",
            Volume = 100.0m,
            Unit = "л",
            ContainerTypeId = _testContainerType.Id,
            Meta = "{\"test\":\"lifecycle\"}"
        };

        var createResponse = await Client.PostAsJsonAsync(BaseRoute, createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdContainer = await createResponse.ToResponseModel<ContainerDto>();
        createdContainer.Status.Should().Be("Empty");
        var containerId = createdContainer.Id;

        // === STEP 2: Fill container ===
        var fillDto = new FillContainerDto
        {
            ProductId = _testProduct!.Id,
            Quantity = 50m,
            Unit = "л",
            ProductionDate = DateTime.UtcNow.AddDays(-1),
            ExpirationDate = DateTime.UtcNow.AddDays(29)
        };

        var fillResponse = await Client.PostAsJsonAsync($"{BaseRoute}/{containerId}/fill", fillDto);
        fillResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var filledContainer = await fillResponse.ToResponseModel<ContainerDto>();
        filledContainer.Status.Should().Be("Full");
        filledContainer.CurrentProductId.Should().Be(_testProduct.Id);
        filledContainer.CurrentQuantity.Should().Be(50m);

        // === STEP 3: Update fill ===
        var updateFillDto = new UpdateContainerFillDto
        {
            ProductId = null, // keep same product
            Quantity = 45m,
            Unit = "л",
            ProductionDate = DateTime.UtcNow.AddDays(-1),
            ExpirationDate = DateTime.UtcNow.AddDays(29)
        };

        var updateFillResponse = await Client.PutAsJsonAsync($"{BaseRoute}/{containerId}/fill", updateFillDto);
        updateFillResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedContainer = await updateFillResponse.ToResponseModel<ContainerDto>();
        updatedContainer.CurrentQuantity.Should().Be(45m);

        // === STEP 4: Empty container ===
        var emptyResponse = await Client.PostAsync($"{BaseRoute}/{containerId}/empty", null);
        emptyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var emptiedContainer = await emptyResponse.ToResponseModel<ContainerDto>();
        emptiedContainer.Status.Should().Be("Empty");
        emptiedContainer.CurrentProductId.Should().BeNull();
        emptiedContainer.CurrentQuantity.Should().BeNull();

        // === STEP 5: Fill again with different product ===
        var secondFillDto = new FillContainerDto
        {
            ProductId = _secondProduct!.Id,
            Quantity = 80m,
            Unit = "л",
            ProductionDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(60)
        };

        var secondFillResponse = await Client.PostAsJsonAsync($"{BaseRoute}/{containerId}/fill", secondFillDto);
        secondFillResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refilledContainer = await secondFillResponse.ToResponseModel<ContainerDto>();
        refilledContainer.Status.Should().Be("Full");
        refilledContainer.CurrentProductId.Should().Be(_secondProduct.Id);
        refilledContainer.CurrentQuantity.Should().Be(80m);

        // === STEP 6: Verify history has 2 entries ===
        var historyResponse = await Client.GetAsync($"{BaseRoute}/{containerId}/history");
        historyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await historyResponse.ToResponseModel<List<ContainerFillDto>>();
        history.Should().HaveCount(2);

        // First entry (most recent) should be the second fill — still active
        history[0].ProductId.Should().Be(_secondProduct.Id);
        history[0].Quantity.Should().Be(80m);
        history[0].EmptiedDate.Should().BeNull();

        // Second entry (older) should be the first fill — closed
        history[1].ProductId.Should().Be(_testProduct.Id);
        history[1].EmptiedDate.Should().NotBeNull();
    }

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddAsync(_testContainerType);
        await Context.ProductTypes.AddAsync(_testProductType);
        await SaveChangesAsync();

        _testProduct = ProductData.FirstTestProduct(_testProductType.Id);
        _secondProduct = ProductData.SecondTestProduct(_testProductType.Id);
        await Context.Products.AddRangeAsync(_testProduct, _secondProduct);
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
