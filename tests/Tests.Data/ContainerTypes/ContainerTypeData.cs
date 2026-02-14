using Api.Dtos;
using Domain.ContainerTypes;

namespace Tests.Data.ContainerTypes;

public static class ContainerTypeData
{
    public static ContainerType FirstTestContainerType(string prefix = "Test") 
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return ContainerType.New(
            $"{prefix}-{uniqueId}-Keg-50L",
            "TEST",
            "л",
            "{\"material\":\"plastic\",\"description\":\"Пластиковий кег 50 літрів\",\"allowedProductTypes\":[]}",
            new Guid("00000000-0000-0000-0000-000000000001")
        );
    }

    public static ContainerType SecondTestContainerType(string prefix = "Test") 
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return ContainerType.New(
            $"{prefix}-{uniqueId}-Barrel-Oak",
            "TEST",
            "л",
            "{\"material\":\"oak\",\"description\":\"Дубова бочка\"}",
            new Guid("00000000-0000-0000-0000-000000000001")
        );
    }

    public static CreateContainerTypeDto CreateTestContainerTypeDto(string prefix = "Test")
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return new CreateContainerTypeDto(
            $"{prefix}-{uniqueId}-Tank-100L",
            "TEST",
            "л",
            "{\"description\":\"Металевий резервуар 100 літрів\"}",
            AllowedProductTypeIds: new List<int>()
        );
    }
    
    public static UpdateContainerTypeDto UpdateTestContainerTypeDto(string prefix = "UpdatedTest")
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return new UpdateContainerTypeDto(
            $"{prefix}-{uniqueId}-Tank-200L",
            "TEST-UPD",
            "л",
            "{\"description\":\"Оновлений металевий резервуар 200 літрів\"}",
            AllowedProductTypeIds: new List<int>()
        );
    }
}
