using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Containers;
using Domain.ContainerTypes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.Containers;
using Tests.Data.ContainerTypes;

namespace Api.Tests.Integration.Containers;

// Тести для оновлення контейнерів (PUT endpoint)
public class ContainersUpdateTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "containers";

    private readonly ContainerType _firstContainerType = ContainerTypeData.FirstTestContainerType();
    private readonly ContainerType _secondContainerType = ContainerTypeData.SecondTestContainerType();
    private Container? _testContainer;

    public ContainersUpdateTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен оновити контейнер
    [Fact]
    public async Task ShouldUpdateContainer()
    {
        // Arrange
        var request = new UpdateContainerDto
        {
            Name = "Updated-Container-Name",
            Volume = 75.0m,
            Unit = "кг",
            ContainerTypeId = _secondContainerType.Id,
            Meta = "{\"location\":\"Updated Location\"}"
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_testContainer!.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();

        container.Name.Should().Be(request.Name);
        container.Volume.Should().Be(request.Volume);
        container.Unit.Should().Be(request.Unit);
        container.ContainerTypeId.Should().Be(request.ContainerTypeId);
        container.Meta.Should().Be(request.Meta);
        container.Id.Should().Be(_testContainer.Id);

        var dbContainer = await Context.Containers
            .AsNoTracking()
            .FirstAsync(c => c.Id == _testContainer.Id);
        dbContainer.Name.Should().Be(request.Name);
        dbContainer.Volume.Should().Be(request.Volume);
        dbContainer.Unit.Should().Be(request.Unit);
    }

    // Повинен оновити Meta на null
    [Fact]
    public async Task ShouldUpdateContainerToNullMeta()
    {
        // Arrange
        var request = new UpdateContainerDto
        {
            Name = "Updated-Name",
            Volume = 60.0m,
            Unit = "л",
            ContainerTypeId = _firstContainerType.Id,
            Meta = null
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_testContainer!.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();
        container.Meta.Should().BeNull();
    }

    // Повинен зберегти код контейнера після оновлення
    [Fact]
    public async Task ShouldPreserveContainerCodeAfterUpdate()
    {
        // Arrange
        var originalCode = _testContainer!.Code;
        var request = new UpdateContainerDto
        {
            Name = "Completely-New-Name",
            Volume = 100.0m,
            Unit = "л",
            ContainerTypeId = _firstContainerType.Id,
            Meta = null
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_testContainer.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();
        container.Code.Should().Be(originalCode);
    }

    // Не повинен оновити неіснуючий контейнер
    [Fact]
    public async Task ShouldNotUpdateContainerBecauseNotFound()
    {
        // Arrange
        var request = new UpdateContainerDto
        {
            Name = "Updated-Name",
            Volume = 60.0m,
            Unit = "л",
            ContainerTypeId = _firstContainerType.Id,
            Meta = null
        };
        var nonExistentId = 999999;

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен оновити контейнер з неіснуючим типом контейнера
    [Fact]
    public async Task ShouldNotUpdateContainerBecauseContainerTypeNotFound()
    {
        // Arrange
        var request = new UpdateContainerDto
        {
            Name = "Updated-Name",
            Volume = 60.0m,
            Unit = "л",
            ContainerTypeId = 999999,
            Meta = null
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_testContainer!.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен оновити контейнер з порожньою назвою
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ShouldNotUpdateContainerBecauseEmptyName(string? name)
    {
        // Arrange
        var request = new UpdateContainerDto
        {
            Name = name!,
            Volume = 50.0m,
            Unit = "л",
            ContainerTypeId = _firstContainerType.Id,
            Meta = null
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_testContainer!.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен оновити контейнер з порожньою одиницею виміру
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ShouldNotUpdateContainerBecauseEmptyUnit(string? unit)
    {
        // Arrange
        var request = new UpdateContainerDto
        {
            Name = "Valid-Name",
            Volume = 50.0m,
            Unit = unit!,
            ContainerTypeId = _firstContainerType.Id,
            Meta = null
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_testContainer!.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен оновити контейнер з об'ємом 0 або негативним
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task ShouldNotUpdateContainerBecauseVolumeZeroOrNegative(decimal volume)
    {
        // Arrange
        var request = new UpdateContainerDto
        {
            Name = "Valid-Name",
            Volume = volume,
            Unit = "л",
            ContainerTypeId = _firstContainerType.Id,
            Meta = null
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_testContainer!.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddRangeAsync(_firstContainerType, _secondContainerType);
        await SaveChangesAsync();

        _testContainer = ContainerData.FirstTestContainer(_firstContainerType.Id);
        await Context.Containers.AddAsync(_testContainer);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Containers.RemoveRange(Context.Containers);
        Context.ContainerTypes.RemoveRange(Context.ContainerTypes);

        await SaveChangesAsync();
    }
}
