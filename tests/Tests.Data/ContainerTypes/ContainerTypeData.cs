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
            "л",
            "{\"material\":\"plastic\",\"description\":\"Пластиковий кег 50 літрів\"}",
            new Guid("00000000-0000-0000-0000-000000000001")
        );
    }

    public static ContainerType SecondTestContainerType(string prefix = "Test") 
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return ContainerType.New(
            $"{prefix}-{uniqueId}-Barrel-Oak",
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
            "л",
            "{\"description\":\"Металевий резервуар 100 літрів\"}"
        );
    }
    
    public static UpdateContainerTypeDto UpdateTestContainerTypeDto(string prefix = "UpdatedTest")
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return new UpdateContainerTypeDto(
            $"{prefix}-{uniqueId}-Tank-200L",
            "л",
            "{\"description\":\"Оновлений металевий резервуар 200 літрів\"}"
        );
    }
}
