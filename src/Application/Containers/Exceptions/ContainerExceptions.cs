namespace Application.Containers.Exceptions;

public abstract class ContainerException(
    int containerId, 
    string message, 
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public int ContainerId { get; } = containerId;
}

public class ContainerAlreadyExistException(int containerId) 
    : ContainerException(containerId, $"Container already exists under id {containerId}");

public class ContainerNotFoundException(int containerId) 
    : ContainerException(containerId, $"Container not found under id {containerId}");

public class ContainerTypeNotFoundForContainerException(int containerTypeId)
    : ContainerException(
        0, 
        $"Container type with id {containerTypeId} not found for container operation");

public class UnhandledContainerException(
    int containerId, 
    Exception? innerException = null)
    : ContainerException(containerId, "Unexpected error occurred", innerException);

public sealed class ContainerNotEmptyException(int containerId) 
    : ContainerException(containerId, "Container is not empty");

public sealed class ContainerNotFullException(int containerId) 
    : ContainerException(containerId, "Container is not full");

public sealed class ContainerOverfillException(int containerId, decimal quantity, decimal volume) 
    : ContainerException(containerId, $"Quantity ({quantity}) exceeds container volume ({volume})");

public sealed class ContainerFillNotFoundException(int fillId) :
    ContainerException(0, $"Container fill with id {fillId} not found", null)
{
    public int FillId { get; } = fillId;
}
