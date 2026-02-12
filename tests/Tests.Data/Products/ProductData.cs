using Api.Dtos;
using Domain.Products;

namespace Tests.Data.Products;

public static class ProductData
{
    public static Product FirstTestProduct(int productTypeId)
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return Product.New(
            $"Test-Product-{uniqueId}-A",
            "Test product description A",
            productTypeId,
            null,
            new Guid("00000000-0000-0000-0000-000000000001")
        );
    }

    public static Product SecondTestProduct(int productTypeId)
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return Product.New(
            $"Test-Product-{uniqueId}-B",
            "Test product description B",
            productTypeId,
            null,
            new Guid("00000000-0000-0000-0000-000000000001")
        );
    }

    public static CreateProductDto CreateTestProductDto(int productTypeId)
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return new CreateProductDto
        {
            Name = $"Test-New-Product-{uniqueId}",
            Description = "New test product description",
            ProductTypeId = productTypeId
        };
    }

    public static CreateProductDto CreateTestProductDtoWithNullDescription(int productTypeId)
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return new CreateProductDto
        {
            Name = $"Test-Product-NoDesc-{uniqueId}",
            Description = null,
            ProductTypeId = productTypeId,
            ShelfLifeDays = null
        };
    }

    public static UpdateProductDto UpdateTestProductDto(int productTypeId)
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return new UpdateProductDto
        {
            Name = $"Updated-Product-{uniqueId}",
            Description = "Updated product description",
            ProductTypeId = productTypeId,
            ShelfLifeDays = null
        };
    }
}
