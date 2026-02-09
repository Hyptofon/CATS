using System.Net;
using System.Net.Http.Json;
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

// Тести для операцій заповнення та спорожнення контейнерів (Fill/Empty/UpdateFill)
public class ContainersFillTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "containers";

    private readonly ContainerType _testContainerType = ContainerTypeData.FirstTestContainerType();
    private readonly ProductType _testProductType = ProductTypeData.FirstTestProductType();
    private Container? _emptyContainer;
    private Container? _fullContainer;
    private Product? _testProduct;
    private Product? _secondProduct;
    private ContainerFill? _existingFill;

    public ContainersFillTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Fill Tests

    // Повинен заповнити порожній контейнер
    [Fact]
    public async Task ShouldFillEmptyContainer()
    {
        // Arrange
        var request = new FillContainerDto
        {
            ProductId = _testProduct!.Id,
            Quantity = 30m,
            Unit = "л",
            ProductionDate = DateTime.UtcNow.AddDays(-1),
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{BaseRoute}/{_emptyContainer!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();

        container.Status.Should().Be(ContainerStatus.Full.ToString());
        container.CurrentProductId.Should().Be(_testProduct.Id);
        container.CurrentQuantity.Should().Be(request.Quantity);

        var dbContainer = await Context.Containers
            .AsNoTracking()
            .FirstAsync(c => c.Id == _emptyContainer.Id);
        dbContainer.Status.Should().Be(ContainerStatus.Full);
        dbContainer.CurrentProductId.Should().Be(_testProduct.Id);
    }

    // Не повинен заповнити контейнер, який вже заповнений
    [Fact]
    public async Task ShouldNotFillContainerBecauseAlreadyFull()
    {
        // Arrange
        var request = new FillContainerDto
        {
            ProductId = _testProduct!.Id,
            Quantity = 30m,
            Unit = "л",
            ProductionDate = DateTime.UtcNow.AddDays(-1),
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{BaseRoute}/{_fullContainer!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен заповнити контейнер кількістю, що перевищує об'єм
    [Fact]
    public async Task ShouldNotFillContainerBecauseQuantityExceedsVolume()
    {
        // Arrange
        var request = new FillContainerDto
        {
            ProductId = _testProduct!.Id,
            Quantity = 1000m, // Container volume is 50
            Unit = "л",
            ProductionDate = DateTime.UtcNow.AddDays(-1),
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{BaseRoute}/{_emptyContainer!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен заповнити контейнер з невідповідною одиницею виміру
    [Fact]
    public async Task ShouldNotFillContainerBecauseUnitMismatch()
    {
        // Arrange
        var request = new FillContainerDto
        {
            ProductId = _testProduct!.Id,
            Quantity = 30m,
            Unit = "кг", // Container uses "л"
            ProductionDate = DateTime.UtcNow.AddDays(-1),
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{BaseRoute}/{_emptyContainer!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен заповнити контейнер з неіснуючим продуктом
    [Fact]
    public async Task ShouldNotFillContainerBecauseProductNotFound()
    {
        // Arrange
        var request = new FillContainerDto
        {
            ProductId = 999999,
            Quantity = 30m,
            Unit = "л",
            ProductionDate = DateTime.UtcNow.AddDays(-1),
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{BaseRoute}/{_emptyContainer!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // Не повинен заповнити неіснуючий контейнер
    [Fact]
    public async Task ShouldNotFillContainerBecauseContainerNotFound()
    {
        // Arrange
        var request = new FillContainerDto
        {
            ProductId = _testProduct!.Id,
            Quantity = 30m,
            Unit = "л",
            ProductionDate = DateTime.UtcNow.AddDays(-1),
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{BaseRoute}/999999/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен заповнити контейнер з кількістю 0 або меншою
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task ShouldNotFillContainerBecauseInvalidQuantity(decimal quantity)
    {
        // Arrange
        var request = new FillContainerDto
        {
            ProductId = _testProduct!.Id,
            Quantity = quantity,
            Unit = "л",
            ProductionDate = DateTime.UtcNow.AddDays(-1),
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{BaseRoute}/{_emptyContainer!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Empty Tests

    // Повинен спорожнити заповнений контейнер
    [Fact]
    public async Task ShouldEmptyFullContainer()
    {
        // Arrange - none

        // Act
        var response = await Client.PostAsync(
            $"{BaseRoute}/{_fullContainer!.Id}/empty",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();

        container.Status.Should().Be(ContainerStatus.Empty.ToString());
        container.CurrentProductId.Should().BeNull();
        container.CurrentQuantity.Should().BeNull();

        var dbContainer = await Context.Containers
            .AsNoTracking()
            .FirstAsync(c => c.Id == _fullContainer.Id);
        dbContainer.Status.Should().Be(ContainerStatus.Empty);
    }

    // Не повинен спорожнити порожній контейнер
    [Fact]
    public async Task ShouldNotEmptyContainerBecauseAlreadyEmpty()
    {
        // Arrange - none

        // Act
        var response = await Client.PostAsync(
            $"{BaseRoute}/{_emptyContainer!.Id}/empty",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен спорожнити неіснуючий контейнер
    [Fact]
    public async Task ShouldNotEmptyContainerBecauseNotFound()
    {
        // Arrange - none

        // Act
        var response = await Client.PostAsync(
            $"{BaseRoute}/999999/empty",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Повинен закрити ContainerFill запис при спорожненні
    [Fact]
    public async Task ShouldCloseContainerFillRecordWhenEmptying()
    {
        // Arrange
        var fillId = _existingFill!.Id;

        // Act
        var response = await Client.PostAsync(
            $"{BaseRoute}/{_fullContainer!.Id}/empty",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbFill = await Context.ContainerFills
            .AsNoTracking()
            .FirstAsync(f => f.Id == fillId);
        dbFill.EmptiedDate.Should().NotBeNull();
        dbFill.EmptiedByUserId.Should().NotBeNull();
    }

    #endregion

    #region UpdateFill Tests

    // Повинен оновити заповнення контейнера
    [Fact]
    public async Task ShouldUpdateContainerFill()
    {
        // Arrange
        var request = new UpdateContainerFillDto
        {
            Quantity = 40m,
            Unit = "л",
            ProductionDate = DateTime.UtcNow.AddDays(-2),
            ExpirationDate = DateTime.UtcNow.AddDays(60)
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_fullContainer!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();

        container.CurrentQuantity.Should().Be(request.Quantity);
    }

    // Повинен оновити заповнення з іншим продуктом
    [Fact]
    public async Task ShouldUpdateContainerFillWithDifferentProduct()
    {
        // Arrange
        var request = new UpdateContainerFillDto
        {
            ProductId = _secondProduct!.Id,
            Quantity = 40m,
            Unit = "л",
            ProductionDate = DateTime.UtcNow.AddDays(-2),
            ExpirationDate = DateTime.UtcNow.AddDays(60)
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_fullContainer!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();
        container.CurrentProductId.Should().Be(_secondProduct.Id);
    }

    // Не повинен оновити заповнення порожнього контейнера
    [Fact]
    public async Task ShouldNotUpdateContainerFillBecauseContainerEmpty()
    {
        // Arrange
        var request = new UpdateContainerFillDto
        {
            Quantity = 40m,
            Unit = "л",
            ProductionDate = DateTime.UtcNow.AddDays(-2),
            ExpirationDate = DateTime.UtcNow.AddDays(60)
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_emptyContainer!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен оновити заповнення з кількістю, що перевищує об'єм
    [Fact]
    public async Task ShouldNotUpdateContainerFillBecauseQuantityExceedsVolume()
    {
        // Arrange
        var request = new UpdateContainerFillDto
        {
            Quantity = 1000m, // Container volume is 75.5
            Unit = "л",
            ProductionDate = DateTime.UtcNow.AddDays(-2),
            ExpirationDate = DateTime.UtcNow.AddDays(60)
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_fullContainer!.Id}/fill",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddAsync(_testContainerType);
        await Context.ProductTypes.AddAsync(_testProductType);
        await SaveChangesAsync();

        _testProduct = ProductData.FirstTestProduct(_testProductType.Id);
        _secondProduct = ProductData.SecondTestProduct(_testProductType.Id);
        await Context.Products.AddRangeAsync(_testProduct, _secondProduct);
        await SaveChangesAsync();

        _emptyContainer = ContainerData.FirstTestContainer(_testContainerType.Id);
        _fullContainer = ContainerData.SecondTestContainer(_testContainerType.Id);
        await Context.Containers.AddRangeAsync(_emptyContainer, _fullContainer);
        await SaveChangesAsync();

        // Fill the second container
        _existingFill = ContainerFill.New(
            _fullContainer.Id,
            _testProduct.Id,
            50m,
            "л",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(30),
            Guid.NewGuid()
        );
        await Context.ContainerFills.AddAsync(_existingFill);
        await SaveChangesAsync();

        _fullContainer.Fill(
            _testProduct.Id,
            _testProductType.Id,
            _existingFill.Quantity,
            _existingFill.Unit,
            _existingFill.ProductionDate,
            _existingFill.ExpirationDate,
            _existingFill.Id,
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
