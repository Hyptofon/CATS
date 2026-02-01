using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Products;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.ProductTypes;

namespace Api.Tests.Integration.ProductTypes;

public class ProductTypesControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "product-types";
    
    private readonly ProductType _firstTestProductType = ProductTypeData.FirstTestProductType();
    private readonly ProductType _secondTestProductType = ProductTypeData.SecondTestProductType();

    public ProductTypesControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region GET Tests

    [Fact]
    public async Task ShouldGetAllProductTypes()
    {
        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productTypes = await response.ToResponseModel<List<ProductTypeDto>>();
        productTypes.Should().HaveCount(2);
        productTypes.Should().Contain(pt => pt.Id == _firstTestProductType.Id.Value);
        productTypes.Should().Contain(pt => pt.Id == _secondTestProductType.Id.Value);
    }

    [Fact]
    public async Task ShouldGetAllProductTypesOrderedByName()
    {
        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productTypes = await response.ToResponseModel<List<ProductTypeDto>>();
        productTypes.Should().BeInAscendingOrder(pt => pt.Name);
    }

    [Fact]
    public async Task ShouldGetProductTypeById()
    {
        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{_firstTestProductType.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productType = await response.ToResponseModel<ProductTypeDto>();
        productType.Id.Should().Be(_firstTestProductType.Id.Value);
        productType.Name.Should().Be(_firstTestProductType.Name);
        productType.ShelfLifeDays.Should().Be(_firstTestProductType.ShelfLifeDays);
        productType.Meta.Should().Be(_firstTestProductType.Meta);
    }

    [Fact]
    public async Task ShouldNotGetProductTypeByIdBecauseNotFound()
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
    public async Task ShouldCreateProductType()
    {
        // Arrange
        var request = ProductTypeData.CreateTestProductTypeDto();

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productTypeDto = await response.ToResponseModel<ProductTypeDto>();
        
        productTypeDto.Name.Should().Be(request.Name);
        productTypeDto.ShelfLifeDays.Should().Be(request.ShelfLifeDays);
        productTypeDto.Meta.Should().Be(request.Meta);
        productTypeDto.Id.Should().NotBeEmpty();

        var productTypeId = new ProductTypeId(productTypeDto.Id);
        var dbProductType = await Context.ProductTypes
            .FirstOrDefaultAsync(pt => pt.Id == productTypeId);
            
        dbProductType.Should().NotBeNull();
        dbProductType!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task ShouldCreateProductTypeWithNullShelfLifeDays()
    {
        // Arrange
        var request = new CreateProductTypeDto("Test-NoShelfLife", null, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productTypeDto = await response.ToResponseModel<ProductTypeDto>();
        productTypeDto.ShelfLifeDays.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotCreateProductTypeBecauseDuplicateName()
    {
        // Arrange
        var request = new CreateProductTypeDto(_firstTestProductType.Name, 30, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("", 30, "{\"test\":\"meta\"}")]
    [InlineData(null, 30, "{\"test\":\"meta\"}")]
    public async Task ShouldNotCreateProductTypeBecauseEmptyName(string name, int shelfLifeDays, string meta)
    {
        // Arrange
        var request = new CreateProductTypeDto(name, shelfLifeDays, meta);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotCreateProductTypeBecauseNegativeShelfLifeDays()
    {
        // Arrange
        var request = new CreateProductTypeDto("Test-Negative", -1, "{\"test\":\"meta\"}");

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task ShouldUpdateProductType()
    {
        // Arrange
        var request = ProductTypeData.UpdateTestProductTypeDto();

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestProductType.Id.Value}", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productType = await response.ToResponseModel<ProductTypeDto>();
        
        productType.Name.Should().Be(request.Name);
        productType.ShelfLifeDays.Should().Be(request.ShelfLifeDays);
        productType.Meta.Should().Be(request.Meta);
    }

    [Fact]
    public async Task ShouldNotUpdateProductTypeBecauseNotFound()
    {
        // Arrange
        var request = ProductTypeData.UpdateTestProductTypeDto();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotUpdateProductTypeBecauseDuplicateName()
    {
        // Arrange 
        var request = new UpdateProductTypeDto(_secondTestProductType.Name, 60, "{\"updated\":\"meta\"}");

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{BaseRoute}/{_firstTestProductType.Id.Value}", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task ShouldDeleteProductType()
    {
        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{_secondTestProductType.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var productType = await response.ToResponseModel<ProductTypeDto>();
        productType.Id.Should().Be(_secondTestProductType.Id.Value);

        var dbProductType = await Context.ProductTypes
            .FirstOrDefaultAsync(pt => pt.Id == _secondTestProductType.Id);
        dbProductType.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotDeleteProductTypeBecauseNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    public async Task InitializeAsync()
    {
        await Context.ProductTypes.AddRangeAsync(_firstTestProductType, _secondTestProductType);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.ProductTypes.RemoveRange(Context.ProductTypes);
        
        await SaveChangesAsync();
    }
}