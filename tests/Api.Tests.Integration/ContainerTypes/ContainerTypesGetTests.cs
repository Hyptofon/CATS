using System.Net;
using Api.Dtos;
using Domain.ContainerTypes;
using FluentAssertions;
using Tests.Common;
using Tests.Data.ContainerTypes;

namespace Api.Tests.Integration.ContainerTypes;

// Тести для отримання типів контейнерів (GET endpoints)
public class ContainerTypesGetTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "container-types";

    private readonly ContainerType _firstTestContainerType = ContainerTypeData.FirstTestContainerType();
    private readonly ContainerType _secondTestContainerType = ContainerTypeData.SecondTestContainerType();

    public ContainerTypesGetTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен отримати всі типи контейнерів
    [Fact]
    public async Task ShouldGetAllContainerTypes()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerTypes = await response.ToResponseModel<List<ContainerTypeDto>>();
        containerTypes.Should().HaveCount(2);
        containerTypes.Should().Contain(ct => ct.Id == _firstTestContainerType.Id);
        containerTypes.Should().Contain(ct => ct.Id == _secondTestContainerType.Id);
    }

    // Повинен отримати всі типи контейнерів відсортовані за назвою
    [Fact]
    public async Task ShouldGetAllContainerTypesOrderedByName()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerTypes = await response.ToResponseModel<List<ContainerTypeDto>>();
        containerTypes.Should().BeInAscendingOrder(ct => ct.Name);
    }

    // Повинен повернути порожній список, якщо немає типів контейнерів
    [Fact]
    public async Task ShouldGetEmptyListWhenNoContainerTypesExist()
    {
        // Arrange
        Context.ContainerTypes.RemoveRange(Context.ContainerTypes);
        await SaveChangesAsync();

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerTypes = await response.ToResponseModel<List<ContainerTypeDto>>();
        containerTypes.Should().BeEmpty();
    }

    // Повинен отримати тип контейнера за ID
    [Fact]
    public async Task ShouldGetContainerTypeById()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{_firstTestContainerType.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerType = await response.ToResponseModel<ContainerTypeDto>();
        containerType.Id.Should().Be(_firstTestContainerType.Id);
        containerType.Name.Should().Be(_firstTestContainerType.Name);
        containerType.DefaultUnit.Should().Be(_firstTestContainerType.DefaultUnit);
        containerType.Meta.Should().Be(_firstTestContainerType.Meta);
        containerType.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    // Повинен отримати тип контейнера з null Meta
    [Fact]
    public async Task ShouldGetContainerTypeByIdWithNullMeta()
    {
        // Arrange
        var containerTypeWithoutMeta = ContainerType.New(
            "Test-NoMeta-Type",
            "TEST-NM",
            "л",
            null,
            new Guid("00000000-0000-0000-0000-000000000001")
        );
        await Context.ContainerTypes.AddAsync(containerTypeWithoutMeta);
        await SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{containerTypeWithoutMeta.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerType = await response.ToResponseModel<ContainerTypeDto>();
        containerType.Meta.Should().BeNull();
    }

    // Не повинен знайти тип контейнера за неіснуючим ID
    [Fact]
    public async Task ShouldNotGetContainerTypeByIdBecauseNotFound()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен отримати тип контейнера з ID = 0
    [Fact]
    public async Task ShouldNotGetContainerTypeByIdBecauseZeroId()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddRangeAsync(_firstTestContainerType, _secondTestContainerType);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.ContainerTypes.RemoveRange(Context.ContainerTypes);

        await SaveChangesAsync();
    }
}
