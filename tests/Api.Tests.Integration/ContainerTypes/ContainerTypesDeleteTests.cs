using System.Net;
using Api.Dtos;
using Domain.Containers;
using Domain.ContainerTypes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.ContainerTypes;

namespace Api.Tests.Integration.ContainerTypes;

// Тести для видалення типів контейнерів (DELETE endpoint)
public class ContainerTypesDeleteTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "container-types";

    private readonly ContainerType _containerTypeToDelete = ContainerTypeData.FirstTestContainerType();
    private readonly ContainerType _containerTypeInUse = ContainerTypeData.SecondTestContainerType();
    private Container? _containerUsingType;

    public ContainerTypesDeleteTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен видалити тип контейнера
    [Fact]
    public async Task ShouldDeleteContainerType()
    {
        // Arrange - none

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_containerTypeToDelete.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerType = await response.ToResponseModel<ContainerTypeDto>();
        containerType.Id.Should().Be(_containerTypeToDelete.Id);

        var dbContainerType = await Context.ContainerTypes
            .FirstOrDefaultAsync(ct => ct.Id == _containerTypeToDelete.Id);
        dbContainerType.Should().BeNull();
    }

    // Не повинен видалити неіснуючий тип контейнера
    [Fact]
    public async Task ShouldNotDeleteContainerTypeBecauseNotFound()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен видалити тип контейнера, який використовується контейнерами
    [Fact]
    public async Task ShouldNotDeleteContainerTypeBecauseHasContainers()
    {
        // Arrange - container type is used by a container (created in InitializeAsync)

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_containerTypeInUse.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var dbContainerType = await Context.ContainerTypes
            .FirstOrDefaultAsync(ct => ct.Id == _containerTypeInUse.Id);
        dbContainerType.Should().NotBeNull();
    }

    // Не повинен видалити тип контейнера з ID = 0
    [Fact]
    public async Task ShouldNotDeleteContainerTypeBecauseZeroId()
    {
        // Arrange - none

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Видалення типу контейнера не повинно вплинути на інші типи
    [Fact]
    public async Task ShouldDeleteContainerTypeWithoutAffectingOthers()
    {
        // Arrange
        var otherContainerTypeId = _containerTypeInUse.Id;

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_containerTypeToDelete.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var otherContainerType = await Context.ContainerTypes
            .FirstOrDefaultAsync(ct => ct.Id == otherContainerTypeId);
        otherContainerType.Should().NotBeNull();
    }

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddRangeAsync(_containerTypeToDelete, _containerTypeInUse);
        await SaveChangesAsync();

        // Create a container that uses the second container type
        _containerUsingType = Container.New(
            "QR-TEST-001",
            "Test Container",
            50.0m,
            "л",
            _containerTypeInUse.Id,
            null,
            new Guid("00000000-0000-0000-0000-000000000001")
        );
        await Context.Containers.AddAsync(_containerUsingType);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Containers.RemoveRange(Context.Containers);
        Context.ContainerTypes.RemoveRange(Context.ContainerTypes);

        await SaveChangesAsync();
    }
}
