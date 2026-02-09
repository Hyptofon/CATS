using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.ContainerTypes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.ContainerTypes;

namespace Api.Tests.Integration.ContainerTypes;

// Тести для створення типів контейнерів (POST endpoint)
public class ContainerTypesCreateTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "container-types";

    private readonly ContainerType _existingContainerType = ContainerTypeData.FirstTestContainerType();

    public ContainerTypesCreateTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Повинен створити новий тип контейнера
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
        containerTypeDto.DefaultUnit.Should().Be(request.DefaultUnit);
        containerTypeDto.Meta.Should().Be(request.Meta);
        containerTypeDto.Id.Should().BeGreaterThan(0);
        containerTypeDto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        var dbContainerType = await Context.ContainerTypes
            .FirstOrDefaultAsync(ct => ct.Id == containerTypeDto.Id);

        dbContainerType.Should().NotBeNull();
        dbContainerType!.Name.Should().Be(request.Name);
        dbContainerType.DefaultUnit.Should().Be(request.DefaultUnit);
    }

    // Повинен створити тип контейнера без мета-даних
    [Fact]
    public async Task ShouldCreateContainerTypeWithNullMeta()
    {
        // Arrange
        var request = new CreateContainerTypeDto("Test-NullMeta-Type", "кг", null);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerTypeDto = await response.ToResponseModel<ContainerTypeDto>();
        containerTypeDto.Meta.Should().BeNull();
    }

    // Повинен створити тип контейнера з максимальною довжиною назви
    [Fact]
    public async Task ShouldCreateContainerTypeWithMaxLengthName()
    {
        // Arrange
        var longName = new string('A', 100);
        var request = new CreateContainerTypeDto(longName, "л", "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var containerTypeDto = await response.ToResponseModel<ContainerTypeDto>();
        containerTypeDto.Name.Should().HaveLength(100);
    }

    // Не повинен створити тип контейнера з дублікатом назви
    [Fact]
    public async Task ShouldNotCreateContainerTypeBecauseDuplicateName()
    {
        // Arrange
        var request = new CreateContainerTypeDto(_existingContainerType.Name, "л", "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // Не повинен створити тип контейнера з дублікатом назви (case-insensitive)
    [Fact]
    public async Task ShouldNotCreateContainerTypeBecauseDuplicateNameCaseInsensitive()
    {
        // Arrange
        var request = new CreateContainerTypeDto(
            _existingContainerType.Name.ToUpper(),
            "л",
            "{\"test\":\"meta\"}"
        );

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // Не повинен створити тип контейнера з порожньою назвою
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ShouldNotCreateContainerTypeBecauseEmptyName(string? name)
    {
        // Arrange
        var request = new CreateContainerTypeDto(name!, "л", "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен створити тип контейнера з порожньою одиницею виміру
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ShouldNotCreateContainerTypeBecauseEmptyDefaultUnit(string? unit)
    {
        // Arrange
        var request = new CreateContainerTypeDto("Valid-Name", unit!, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен створити тип контейнера з занадто довгою назвою
    [Fact]
    public async Task ShouldNotCreateContainerTypeBecauseNameTooLong()
    {
        // Arrange
        var tooLongName = new string('A', 101);
        var request = new CreateContainerTypeDto(tooLongName, "л", "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Не повинен створити тип контейнера з назвою з пробілів
    [Fact]
    public async Task ShouldNotCreateContainerTypeBecauseWhitespaceName()
    {
        // Arrange
        var request = new CreateContainerTypeDto("   ", "л", "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public async Task InitializeAsync()
    {
        await Context.ContainerTypes.AddAsync(_existingContainerType);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.ContainerTypes.RemoveRange(Context.ContainerTypes);

        await SaveChangesAsync();
    }
}
