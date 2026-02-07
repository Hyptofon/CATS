using Api.Dtos;
using Domain.Containers;

namespace Tests.Data.Containers;

public static class ContainerData
{
    public static Container FirstTestContainer(int containerTypeId)
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return Container.New(
            $"QR-{uniqueId}-001",
            $"Test-Container-{uniqueId}-A",
            50.0m,
            "л",
            containerTypeId,
            "{\"location\":\"Warehouse A\",\"notes\":\"Test container\"}",
            new Guid("00000000-0000-0000-0000-000000000001")
        );
    }

    public static Container SecondTestContainer(int containerTypeId)
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return Container.New(
            $"QR-{uniqueId}-002",
            $"Test-Container-{uniqueId}-B",
            75.5m,
            "л",
            containerTypeId,
            "{\"location\":\"Warehouse B\"}",
            new Guid("00000000-0000-0000-0000-000000000001")
        );
    }

    public static CreateContainerDto CreateTestContainerDto(int containerTypeId)
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return new CreateContainerDto(
            $"QR-{uniqueId}-NEW",
            $"Test-New-Container-{uniqueId}",
            100.0m,
            "л",
            containerTypeId,
            "{\"location\":\"Warehouse C\"}"
        );
    }
    
    public static CreateContainerDto CreateTestContainerDtoWithAutoCode(int containerTypeId)
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return new CreateContainerDto(
            null,
            $"Test-AutoCode-Container-{uniqueId}",
            100.0m,
            "л",
            containerTypeId,
            "{\"location\":\"Warehouse D\"}"
        );
    }
}
