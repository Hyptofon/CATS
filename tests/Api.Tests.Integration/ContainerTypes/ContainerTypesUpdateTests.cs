using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.ContainerTypes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.ContainerTypes;

namespace Api.Tests.Integration.ContainerTypes;

// Тести для оновлення типів контейнерів (PUT endpoint)
public class ContainerTypesUpdateTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "container-types";

    private readonly ContainerType _firstTestContainerType = ContainerTypeData.FirstTestContainerType();
    private readonly ContainerType _secondTestContainerType = ContainerTypeData.SecondTestContainerType();

    public ContainerTypesUpdateTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен оновити тип контейнера
    [Fact]
    public async Task ShouldUpdateContainerType()
    {
        // Arrange
        var request = ContainerTypeData.UpdateTestContainerTypeDto();

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerType = await response.ToResponseModel<ContainerTypeDto>();

        containerType.Name.Should().Be(request.Name);
        containerType.DefaultUnit.Should().Be(request.DefaultUnit);
        containerType.Meta.Should().Be(request.Meta);
        containerType.Id.Should().Be(_firstTestContainerType.Id);

        var dbContainerType = await Context.ContainerTypes
            .AsNoTracking()
            .FirstAsync(ct => ct.Id == _firstTestContainerType.Id);
        dbContainerType.Name.Should().Be(request.Name);
        dbContainerType.DefaultUnit.Should().Be(request.DefaultUnit);
    }

    // Повинен оновити тип контейнера з тією ж назвою
    [Fact]
    public async Task ShouldUpdateContainerTypeWithSameName()
    {
        // Arrange
        var request = new UpdateContainerTypeDto(
            _firstTestContainerType.Name,
            "TEST-UPD",
            "кг",
            "{\"updated\":\"meta\"}",
            AllowedProductTypeIds: new List<int>()
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Повинен оновити Meta на null
    [Fact]
    public async Task ShouldUpdateContainerTypeToNullMeta()
    {
        // Arrange
        var request = new UpdateContainerTypeDto("Updated-Name", "TEST-NM", "л", null,
            AllowedProductTypeIds: new List<int>()
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerType = await response.ToResponseModel<ContainerTypeDto>();
        containerType.Meta.Should().BeNull();
    }

    // Повинен оновити тип контейнера з максимальною довжиною назви
    [Fact]
    public async Task ShouldUpdateContainerTypeWithMaxLengthName()
    {
        // Arrange
        var longName = new string('U', 100);
        var request = new UpdateContainerTypeDto(longName, "TEST-LN", "л", "{\"test\":\"meta\"}",
            AllowedProductTypeIds: new List<int>()
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Не повинен оновити неіснуючий тип контейнера
    [Fact]
    public async Task ShouldNotUpdateContainerTypeBecauseNotFound()
    {
        // Arrange
        var request = ContainerTypeData.UpdateTestContainerTypeDto();
        var nonExistentId = 999999;

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Не повинен оновити тип контейнера з дублікатом назви
    [Fact]
    public async Task ShouldNotUpdateContainerTypeBecauseDuplicateName()
    {
        // Arrange
        var request = new UpdateContainerTypeDto(_secondTestContainerType.Name, "TEST-DUP", "л", "{\"updated\":\"meta\"}",
            AllowedProductTypeIds: new List<int>()
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // Не повинен оновити тип контейнера з дублікатом назви (case-insensitive)
    [Fact]
    public async Task ShouldNotUpdateContainerTypeBecauseDuplicateNameCaseInsensitive()
    {
        // Arrange
        var request = new UpdateContainerTypeDto(
            _secondTestContainerType.Name.ToLower(),
            "TEST-DUP-CI",
            "л",
            "{\"updated\":\"meta\"}",
            AllowedProductTypeIds: new List<int>()
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // Не повинен оновити тип контейнера з порожньою назвою
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ShouldNotUpdateContainerTypeBecauseEmptyName(string? name)
    {
        // Arrange
        var request = new UpdateContainerTypeDto(name!, "TEST-EN", "л", "{\"test\":\"meta\"}",
            AllowedProductTypeIds: new List<int>()
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен оновити тип контейнера з порожньою одиницею виміру
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ShouldNotUpdateContainerTypeBecauseEmptyDefaultUnit(string? unit)
    {
        // Arrange
        var request = new UpdateContainerTypeDto("Valid-Name", "TEST-VU", unit!, "{\"test\":\"meta\"}",
            AllowedProductTypeIds: new List<int>()
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен оновити тип контейнера з занадто довгою назвою
    [Fact]
    public async Task ShouldNotUpdateContainerTypeBecauseNameTooLong()
    {
        // Arrange
        var tooLongName = new string('U', 101);
        var request = new UpdateContainerTypeDto(tooLongName, "TEST-LG", "л", "{\"test\":\"meta\"}",
            AllowedProductTypeIds: new List<int>()
        );

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestContainerType.Id}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен оновити тип контейнера з ID = 0
    [Fact]
    public async Task ShouldNotUpdateContainerTypeBecauseZeroId()
    {
        // Arrange
        var request = ContainerTypeData.UpdateTestContainerTypeDto();

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/0",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
