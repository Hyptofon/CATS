using Domain.ContainerTypes;

namespace Application.ContainerTypes.Exceptions;

public abstract class ContainerTypeException(
    ContainerTypeId containerTypeId, 
    string message, 
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public ContainerTypeId ContainerTypeId { get; } = containerTypeId;
}

public class ContainerTypeAlreadyExistException(ContainerTypeId containerTypeId) 
    : ContainerTypeException(containerTypeId, $"Container type already exists under id {containerTypeId}");

public class ContainerTypeNotFoundException(ContainerTypeId containerTypeId) 
    : ContainerTypeException(containerTypeId, $"Container type not found under id {containerTypeId}");

public class ContainerTypeCannotBeDeletedException(ContainerTypeId containerTypeId)
    : ContainerTypeException(containerTypeId, $"Container type {containerTypeId} cannot be deleted because it contains containers.");

public class UnhandledContainerTypeException(
    ContainerTypeId containerTypeId, 
    Exception? innerException = null)
    : ContainerTypeException(containerTypeId, "Unexpected error occurred", innerException);