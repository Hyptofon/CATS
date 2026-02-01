using Domain.Containers;
using Domain.ContainerTypes;

namespace Application.Containers.Exceptions;

public abstract class ContainerException(
    ContainerId containerId, 
    string message, 
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public ContainerId ContainerId { get; } = containerId;
}

public class ContainerAlreadyExistException(ContainerId containerId) 
    : ContainerException(containerId, $"Container already exists under id {containerId}");

public class ContainerNotFoundException(ContainerId containerId) 
    : ContainerException(containerId, $"Container not found under id {containerId}");

public class ContainerTypeNotFoundForContainerException(ContainerTypeId containerTypeId)
    : ContainerException(
        ContainerId.Empty(), 
        $"Container type with id {containerTypeId} not found for container operation");

public class UnhandledContainerException(
    ContainerId containerId, 
    Exception? innerException = null)
    : ContainerException(containerId, "Unexpected error occurred", innerException);