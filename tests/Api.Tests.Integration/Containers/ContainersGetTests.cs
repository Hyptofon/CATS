using System.Net;
using Api.Dtos;
using Domain.Containers;
using Domain.ContainerTypes;
using FluentAssertions;
using Tests.Common;
using Tests.Data.Containers;
using Tests.Data.ContainerTypes;

namespace Api.Tests.Integration.Containers;

// Тести для отримання контейнерів (GET endpoints)
public class ContainersGetTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "containers";

    private readonly ContainerType _testContainerType = ContainerTypeData.FirstTestContainerType();
    private Container? _firstTestContainer;
    private Container? _secondTestContainer;

    public ContainersGetTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен отримати всі контейнери
    [Fact]
    public async Task ShouldGetAllContainers()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().HaveCount(2);
        containers.Should().Contain(c => c.Id == _firstTestContainer!.Id);
        containers.Should().Contain(c => c.Id == _secondTestContainer!.Id);
    }

    // Повинен отримати всі контейнери відсортовані за назвою
    [Fact]
    public async Task ShouldGetAllContainersOrderedByName()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().BeInAscendingOrder(c => c.Name);
    }

    // Повинен отримати контейнер за ID
    [Fact]
    public async Task ShouldGetContainerById()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{_firstTestContainer!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();
        container.Id.Should().Be(_firstTestContainer.Id);
        container.Code.Should().Be(_firstTestContainer.Code);
        container.Name.Should().Be(_firstTestContainer.Name);
        container.Volume.Should().Be(_firstTestContainer.Volume);
        container.Unit.Should().Be(_firstTestContainer.Unit);
        container.ContainerTypeId.Should().Be(_testContainerType.Id);
        container.Status.Should().Be(ContainerStatus.Empty.ToString());
    }

    // Повинен отримати контейнер за кодом
    [Fact]
    public async Task ShouldGetContainerByCode()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/code/{_firstTestContainer!.Code}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();
        container.Id.Should().Be(_firstTestContainer.Id);
        container.Code.Should().Be(_firstTestContainer.Code);
    }

    // Не повинен знайти контейнер за неіснуючим ID
    [Fact]
    public async Task ShouldNotGetContainerByIdBecauseNotFound()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен знайти контейнер за неіснуючим кодом
    [Fact]
    public async Task ShouldNotGetContainerByCodeBecauseNotFound()
    {
        // Arrange
        var nonExistentCode = "NONEXISTENT-CODE";

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/code/{nonExistentCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен отримати контейнер з ID = 0
    [Fact]
    public async Task ShouldNotGetContainerByIdBecauseZeroId()
    {
        // Arrange - none

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddAsync(_testContainerType);
        await SaveChangesAsync();

        _firstTestContainer = ContainerData.FirstTestContainer(_testContainerType.Id);
        _secondTestContainer = ContainerData.SecondTestContainer(_testContainerType.Id);

        await Context.Containers.AddRangeAsync(_firstTestContainer, _secondTestContainer);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Containers.RemoveRange(Context.Containers);
        Context.ContainerTypes.RemoveRange(Context.ContainerTypes);

        await SaveChangesAsync();
    }
}
