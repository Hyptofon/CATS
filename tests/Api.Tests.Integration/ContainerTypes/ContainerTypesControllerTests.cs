using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.ContainerTypes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.ContainerTypes;

namespace Api.Tests.Integration.ContainerTypes;

public class ContainerTypesControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "container-types";
    
    private readonly ContainerType _firstTestContainerType = ContainerTypeData.FirstTestContainerType();
    private readonly ContainerType _secondTestContainerType = ContainerTypeData.SecondTestContainerType();

    public ContainerTypesControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region GET Tests

    [Fact]
    public async Task ShouldGetAllContainerTypes()
    {
        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerTypes = await response.ToResponseModel<List<ContainerTypeDto>>();
        containerTypes.Should().HaveCount(2);
        containerTypes.Should().Contain(ct => ct.Id == _firstTestContainerType.Id.Value);
        containerTypes.Should().Contain(ct => ct.Id == _secondTestContainerType.Id.Value);
    }

    [Fact]
    public async Task ShouldGetAllContainerTypesOrderedByName()
    {
        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerTypes = await response.ToResponseModel<List<ContainerTypeDto>>();
        containerTypes.Should().BeInAscendingOrder(ct => ct.Name);
    }

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

    [Fact]
    public async Task ShouldGetContainerTypeById()
    {
        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{_firstTestContainerType.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerType = await response.ToResponseModel<ContainerTypeDto>();
        containerType.Id.Should().Be(_firstTestContainerType.Id.Value);
        containerType.Name.Should().Be(_firstTestContainerType.Name);
        containerType.Meta.Should().Be(_firstTestContainerType.Meta);
        containerType.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ShouldGetContainerTypeByIdWithNullMeta()
    {
        // Arrange
        var containerTypeWithoutMeta = ContainerType.New(
            "Test-NoMeta-Type", 
            null,
            new Guid("00000000-0000-0000-0000-000000000001")
        );
        await Context.ContainerTypes.AddAsync(containerTypeWithoutMeta);
        await SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{containerTypeWithoutMeta.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerType = await response.ToResponseModel<ContainerTypeDto>();
        containerType.Meta.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotGetContainerTypeByIdBecauseNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task ShouldCreateContainerType()
    {
        // Arrange
        var request = ContainerTypeData.CreateTestContainerTypeDto();

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerTypeDto = await response.ToResponseModel<ContainerTypeDto>();
        
        containerTypeDto.Name.Should().Be(request.Name);
        containerTypeDto.Meta.Should().Be(request.Meta);
        containerTypeDto.Id.Should().NotBeEmpty();
        containerTypeDto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        var containerTypeId = new ContainerTypeId(containerTypeDto.Id);
        var dbContainerType = await Context.ContainerTypes
            .FirstOrDefaultAsync(ct => ct.Id == containerTypeId);
            
        dbContainerType.Should().NotBeNull();
        dbContainerType!.Name.Should().Be(request.Name);
        dbContainerType.Meta.Should().Be(request.Meta);
    }

    [Fact]
    public async Task ShouldCreateContainerTypeWithNullMeta()
    {
        // Arrange
        var request = new CreateContainerTypeDto("Test-NullMeta-Type", null);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerTypeDto = await response.ToResponseModel<ContainerTypeDto>();
        containerTypeDto.Meta.Should().BeNull();
    }

    [Fact]
    public async Task ShouldCreateContainerTypeWithMaxLengthName()
    {
        // Arrange
        var longName = new string('A', 100);
        var request = new CreateContainerTypeDto(longName, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerTypeDto = await response.ToResponseModel<ContainerTypeDto>();
        containerTypeDto.Name.Should().HaveLength(100);
    }

    [Fact]
    public async Task ShouldNotCreateContainerTypeBecauseDuplicateName()
    {
        // Arrange
        var request = new CreateContainerTypeDto(_firstTestContainerType.Name, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ShouldNotCreateContainerTypeBecauseDuplicateNameCaseInsensitive()
    {
        // Arrange
        var request = new CreateContainerTypeDto(
            _firstTestContainerType.Name.ToUpper(), 
            "{\"test\":\"meta\"}"
        );

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("", "{\"test\":\"meta\"}")]
    [InlineData(null, "{\"test\":\"meta\"}")]
    public async Task ShouldNotCreateContainerTypeBecauseEmptyName(string name, string meta)
    {
        // Arrange
        var request = new CreateContainerTypeDto(name, meta);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotCreateContainerTypeBecauseNameTooLong()
    {
        // Arrange
        var tooLongName = new string('A', 101);
        var request = new CreateContainerTypeDto(tooLongName, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotCreateContainerTypeBecauseWhitespaceName()
    {
        // Arrange
        var request = new CreateContainerTypeDto("   ", "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task ShouldUpdateContainerType()
    {
        // Arrange
        var request = ContainerTypeData.UpdateTestContainerTypeDto();

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id.Value}", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerType = await response.ToResponseModel<ContainerTypeDto>();
        
        containerType.Name.Should().Be(request.Name);
        containerType.Meta.Should().Be(request.Meta);
        containerType.Id.Should().Be(_firstTestContainerType.Id.Value);

        var dbContainerType = await Context.ContainerTypes
            .AsNoTracking()
            .FirstAsync(ct => ct.Id == _firstTestContainerType.Id);
        dbContainerType.Name.Should().Be(request.Name);
        dbContainerType.Meta.Should().Be(request.Meta);
    }

    [Fact]
    public async Task ShouldUpdateContainerTypeWithSameName()
    {
        // Arrange
        var request = new UpdateContainerTypeDto(
            _firstTestContainerType.Name, 
            "{\"updated\":\"meta\"}"
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id.Value}", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ShouldUpdateContainerTypeToNullMeta()
    {
        // Arrange
        var request = new UpdateContainerTypeDto("Updated-Name", null);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id.Value}", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerType = await response.ToResponseModel<ContainerTypeDto>();
        containerType.Meta.Should().BeNull();
    }

    [Fact]
    public async Task ShouldUpdateContainerTypeWithMaxLengthName()
    {
        // Arrange
        var longName = new string('U', 100);
        var request = new UpdateContainerTypeDto(longName, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id.Value}", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ShouldNotUpdateContainerTypeBecauseNotFound()
    {
        // Arrange
        var request = ContainerTypeData.UpdateTestContainerTypeDto();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldNotUpdateContainerTypeBecauseDuplicateName()
    {
        // Arrange 
        var request = new UpdateContainerTypeDto(_secondTestContainerType.Name, "{\"updated\":\"meta\"}");

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id.Value}", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict); 
    }

    [Fact]
    public async Task ShouldNotUpdateContainerTypeBecauseDuplicateNameCaseInsensitive()
    {
        // Arrange
        var request = new UpdateContainerTypeDto(
            _secondTestContainerType.Name.ToLower(), 
            "{\"updated\":\"meta\"}"
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id.Value}", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("", "{\"test\":\"meta\"}")]
    [InlineData(null, "{\"test\":\"meta\"}")]
    public async Task ShouldNotUpdateContainerTypeBecauseEmptyName(string name, string meta)
    {
        // Arrange
        var request = new UpdateContainerTypeDto(name, meta);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id.Value}", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotUpdateContainerTypeBecauseNameTooLong()
    {
        // Arrange
        var tooLongName = new string('U', 101);
        var request = new UpdateContainerTypeDto(tooLongName, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id.Value}", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotUpdateContainerTypeBecauseEmptyGuid()
    {
        // Arrange
        var request = ContainerTypeData.UpdateTestContainerTypeDto();

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{Guid.Empty}", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task ShouldDeleteContainerType()
    {
        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_secondTestContainerType.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerType = await response.ToResponseModel<ContainerTypeDto>();
        containerType.Id.Should().Be(_secondTestContainerType.Id.Value);

        var dbContainerType = await Context.ContainerTypes
            .FirstOrDefaultAsync(ct => ct.Id == _secondTestContainerType.Id);
        dbContainerType.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotDeleteContainerTypeBecauseNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotDeleteContainerTypeBecauseHasContainers()
    {
        // Arrange
        var container = Domain.Containers.Container.New(
            "QR-TEST-001",
            "Test Container",
            50.0m,
            _firstTestContainerType.Id,
            null,
            new Guid("00000000-0000-0000-0000-000000000001")
        );
        await Context.Containers.AddAsync(container);
        await SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_firstTestContainerType.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        var dbContainerType = await Context.ContainerTypes
            .FirstOrDefaultAsync(ct => ct.Id == _firstTestContainerType.Id);
        dbContainerType.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldNotDeleteContainerTypeBecauseEmptyGuid()
    {
        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddRangeAsync(_firstTestContainerType, _secondTestContainerType);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Containers.RemoveRange(Context.Containers);
        Context.ContainerTypes.RemoveRange(Context.ContainerTypes);
        
        await SaveChangesAsync();
    }
}