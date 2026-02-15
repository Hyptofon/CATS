using Api.Dtos;
using Domain.Products;

namespace Tests.Data.ProductTypes;

public static class ProductTypeData
{
    public static ProductType FirstTestProductType(string prefix = "Test") 
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return ProductType.New(
            $"{prefix}-{uniqueId}-Beer",
            30,
            null,
            "{\"description\":\"Пиво\",\"storageTemp\":\"2-6C\"}",
            new Guid("00000000-0000-0000-0000-000000000001")
        );
    }

    public static ProductType SecondTestProductType(string prefix = "Test") 
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return ProductType.New(
            $"{prefix}-{uniqueId}-Wine",
            365,
            null,
            "{\"description\":\"Вино\",\"storageTemp\":\"10-15C\"}",
            new Guid("00000000-0000-0000-0000-000000000001")
        );
    }

    public static CreateProductTypeDto CreateTestProductTypeDto(string prefix = "Test")
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return new CreateProductTypeDto(
            $"{prefix}-{uniqueId}-Kvass", 
            14,
            null,
            "{\"description\":\"Квас\"}" 
        );
    }

    public static UpdateProductTypeDto UpdateTestProductTypeDto(string prefix = "Updated")
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return new UpdateProductTypeDto(
            $"{prefix}-{uniqueId}-ProductType", 
            60,
            null,
            "{\"description\":\"Updated product type\"}"
        );
    }
}