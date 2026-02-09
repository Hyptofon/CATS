using System.Net;
using Api.Dtos;
using Domain.Containers;
using Domain.ContainerTypes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.Containers;
using Tests.Data.ContainerTypes;

namespace Api.Tests.Integration.Containers;

// Тести для видалення контейнерів (DELETE endpoint)
public class ContainersDeleteTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "containers";

    private readonly ContainerType _testContainerType = ContainerTypeData.FirstTestContainerType();
    private Container? _containerToDelete;
    private Container? _containerToKeep;

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

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddAsync(_testContainerType);
        await SaveChangesAsync();

        _containerToDelete = ContainerData.FirstTestContainer(_testContainerType.Id);
        _containerToKeep = ContainerData.SecondTestContainer(_testContainerType.Id);

        await Context.Containers.AddRangeAsync(_containerToDelete, _containerToKeep);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Containers.RemoveRange(Context.Containers);
        Context.ContainerTypes.RemoveRange(Context.ContainerTypes);

        await SaveChangesAsync();
    }
}
