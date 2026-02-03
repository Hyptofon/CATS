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

public class ContainersControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "containers";
    
    private readonly ContainerType _testContainerType = ContainerTypeData.FirstTestContainerType();
    private readonly ContainerType _secondContainerType = ContainerTypeData.SecondTestContainerType();
    private Container? _firstTestContainer;
    private Container? _secondTestContainer;

    public ContainersControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region GET Tests

    [Fact]
    public async Task ShouldGetAllContainers()
    {
        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().HaveCount(2);
        containers.Should().Contain(c => c.Id == _firstTestContainer!.Id);
        containers.Should().Contain(c => c.Id == _secondTestContainer!.Id);
    }

    [Fact]
    public async Task ShouldGetAllContainersOrderedByName()
    {
        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().BeInAscendingOrder(c => c.Name);
    }

    [Fact]
    public async Task ShouldGetContainerById()
    {
        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{_firstTestContainer!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();
        container.Id.Should().Be(_firstTestContainer.Id);
        container.Code.Should().Be(_firstTestContainer.Code);
        container.Name.Should().Be(_firstTestContainer.Name);
        container.Volume.Should().Be(_firstTestContainer.Volume);
        container.ContainerTypeId.Should().Be(_testContainerType.Id);
        container.Status.Should().Be(ContainerStatus.Empty.ToString());
    }

    [Fact]
    public async Task ShouldGetContainerByCode()
    {
        // Act
        var response = await Client.GetAsync($"{BaseRoute}/code/{_firstTestContainer!.Code}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();
        container.Id.Should().Be(_firstTestContainer.Id);
        container.Code.Should().Be(_firstTestContainer.Code);
    }

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

    #endregion

    #region SEARCH Tests

    [Fact]
    public async Task ShouldSearchContainersBySearchTerm()
    {
        var createTypeDto = ContainerTypeData.CreateTestContainerTypeDto();
        var typeResponse = await Client.PostAsJsonAsync("container-types", createTypeDto); 
        typeResponse.EnsureSuccessStatusCode();
        var createdType = await typeResponse.ToResponseModel<ContainerTypeDto>();
        var typeId = createdType.Id;
        var createContainerDto = ContainerData.CreateTestContainerDto(typeId);
        var postResponse = await Client.PostAsJsonAsync("containers", createContainerDto);
        postResponse.EnsureSuccessStatusCode();
        
        var searchTerm = "Test-New-Container"; 
        var response = await Client.GetAsync($"containers/search?searchTerm={searchTerm}");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().NotBeEmpty(); 
        containers.Should().Contain(c => c.Name.Contains(searchTerm));
    }

    [Fact]
    public async Task ShouldSearchContainersByContainerType()
    {
        // Act
        var response = await Client.GetAsync(
            $"{BaseRoute}/search?containerTypeId={_testContainerType.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().HaveCount(2);
        containers.Should().AllSatisfy(c => 
            c.ContainerTypeId.Should().Be(_testContainerType.Id));
    }

    [Fact]
    public async Task ShouldSearchContainersByStatus()
    {
        // Act
        var response = await Client.GetAsync($"{BaseRoute}/search?status=Empty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().HaveCount(2);
        containers.Should().AllSatisfy(c => c.Status.Should().Be("Empty"));
    }

    [Fact]
    public async Task ShouldSearchContainersWithMultipleFilters()
    {
        // Act
        var response = await Client.GetAsync(
            $"{BaseRoute}/search?searchTerm=Container&containerTypeId={_testContainerType.Id}&status=Empty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containers = await response.ToResponseModel<List<ContainerDto>>();
        containers.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task ShouldCreateContainer()
    {
        // Arrange
        var request = ContainerData.CreateTestContainerDto(_testContainerType.Id);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerDto = await response.ToResponseModel<ContainerDto>();
        
        containerDto.Code.Should().Be(request.Code);
        containerDto.Name.Should().Be(request.Name);
        containerDto.Volume.Should().Be(request.Volume);
        containerDto.ContainerTypeId.Should().Be(request.ContainerTypeId);
        containerDto.Status.Should().Be(ContainerStatus.Empty.ToString());
        containerDto.Id.Should().BeGreaterThan(0);

        var dbContainer = await Context.Containers
            .FirstOrDefaultAsync(c => c.Id == containerDto.Id);
            
        dbContainer.Should().NotBeNull();
        dbContainer!.Code.Should().Be(request.Code);
        dbContainer.Status.Should().Be(ContainerStatus.Empty);
    }

    [Fact]
    public async Task ShouldCreateContainerWithNullMeta()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString()[..8];
        var request = new CreateContainerDto(
            $"QR-{uniqueId}-NULL",
            "Test-NullMeta-Container",
            50.0m,
            _testContainerType.Id,
            null
        );

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerDto = await response.ToResponseModel<ContainerDto>();
        containerDto.Meta.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotCreateContainerBecauseDuplicateCode()
    {
        // Arrange
        var request = new CreateContainerDto(
            _firstTestContainer!.Code,
            "Test-Duplicate-Container",
            50.0m,
            _testContainerType.Id,
            null
        );

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ShouldCreateContainerWithAutoGeneratedCode()
    {
        // Arrange 
        var request = ContainerData.CreateTestContainerDtoWithAutoCode(_testContainerType.Id);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerDto = await response.ToResponseModel<ContainerDto>();
        
        // Перевіряємо, що код був автоматично згенерований у форматі CNT-YYYYMMDD-XXXX
        containerDto.Code.Should().StartWith("CNT-");
        containerDto.Code.Should().MatchRegex(@"^CNT-\d{8}-[A-Z0-9]{4}$");
        containerDto.Name.Should().Be(request.Name);
        containerDto.Id.Should().BeGreaterThan(0);

        var dbContainer = await Context.Containers
            .FirstOrDefaultAsync(c => c.Id == containerDto.Id);
            
        dbContainer.Should().NotBeNull();
        dbContainer!.Code.Should().Be(containerDto.Code);
    }

    [Fact]
    public async Task ShouldNotCreateContainerBecauseContainerTypeNotFound()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString()[..8];
        var request = new CreateContainerDto(
            $"QR-{uniqueId}-FAIL",
            "Test-Fail-Container",
            50.0m,
            999999,
            null
        );

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }



    [Theory]
    [InlineData("VALID-CODE", "", 50.0)]
    [InlineData("VALID-CODE", null, 50.0)]
    public async Task ShouldNotCreateContainerBecauseEmptyName(
        string code, 
        string name, 
        decimal volume)
    {
        // Arrange
        var request = new CreateContainerDto(
            code,
            name,
            volume,
            _testContainerType.Id,
            null
        );

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotCreateContainerBecauseVolumeZeroOrNegative()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString()[..8];
        var request = new CreateContainerDto(
            $"QR-{uniqueId}-ZERO",
            "Test-Zero-Volume",
            0m,
            _testContainerType.Id,
            null
        );

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotCreateContainerBecauseCodeTooLong()
    {
        // Arrange
        var tooLongCode = new string('C', 51);
        var request = new CreateContainerDto(
            tooLongCode,
            "Test-Long-Code",
            50.0m,
            _testContainerType.Id,
            null
        );

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotCreateContainerBecauseNameTooLong()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString()[..8];
        var tooLongName = new string('N', 101);
        var request = new CreateContainerDto(
            $"QR-{uniqueId}-LONG",
            tooLongName,
            50.0m,
            _testContainerType.Id,
            null
        );

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task ShouldUpdateContainer()
    {
        // Arrange
        var request = new UpdateContainerDto(
            "Updated-Container-Name",
            75.0m,
            _secondContainerType.Id,
            "{\"location\":\"Updated Location\"}"
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainer!.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();
        
        container.Name.Should().Be(request.Name);
        container.Volume.Should().Be(request.Volume);
        container.ContainerTypeId.Should().Be(request.ContainerTypeId);
        container.Meta.Should().Be(request.Meta);
        container.Id.Should().Be(_firstTestContainer.Id);

        var dbContainer = await Context.Containers
            .AsNoTracking()
            .FirstAsync(c => c.Id == _firstTestContainer.Id);
        dbContainer.Name.Should().Be(request.Name);
        dbContainer.Volume.Should().Be(request.Volume);
    }

    [Fact]
    public async Task ShouldUpdateContainerToNullMeta()
    {
        // Arrange
        var request = new UpdateContainerDto(
            "Updated-Name",
            60.0m,
            _testContainerType.Id,
            null
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainer!.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();
        container.Meta.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotUpdateContainerBecauseNotFound()
    {
        // Arrange
        var request = new UpdateContainerDto(
            "Updated-Name",
            60.0m,
            _testContainerType.Id,
            null
        );
        var nonExistentId = 999999;

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotUpdateContainerBecauseContainerTypeNotFound()
    {
        // Arrange
        var request = new UpdateContainerDto(
            "Updated-Name",
            60.0m,
            999999,
            null
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainer!.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("", 50.0)]
    [InlineData(null, 50.0)]
    public async Task ShouldNotUpdateContainerBecauseEmptyName(string name, decimal volume)
    {
        // Arrange
        var request = new UpdateContainerDto(
            name,
            volume,
            _testContainerType.Id,
            null
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainer!.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotUpdateContainerBecauseVolumeZeroOrNegative()
    {
        // Arrange
        var request = new UpdateContainerDto(
            "Valid-Name",
            0m,
            _testContainerType.Id,
            null
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainer!.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task ShouldDeleteContainer()
    {
        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_secondTestContainer!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var container = await response.ToResponseModel<ContainerDto>();
        container.Id.Should().Be(_secondTestContainer.Id);

        var dbContainer = await Context.Containers
            .FirstOrDefaultAsync(c => c.Id == _secondTestContainer.Id);
        dbContainer.Should().BeNull();
    }

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

    [Fact]
    public async Task ShouldNotDeleteContainerBecauseZeroId()
    {
        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddRangeAsync(_testContainerType, _secondContainerType);
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