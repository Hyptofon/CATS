namespace Application.ContainerTypes.Exceptions;

public abstract class ContainerTypeException(
    int containerTypeId, 
    string message, 
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public int ContainerTypeId { get; } = containerTypeId;
}

public class ContainerTypeAlreadyExistException(int containerTypeId) 
    : ContainerTypeException(containerTypeId, $"Container type already exists under id {containerTypeId}");

public class ContainerTypeNotFoundException(int containerTypeId) 
    : ContainerTypeException(containerTypeId, $"Container type not found under id {containerTypeId}");

public class ContainerTypeCannotBeDeletedException(int containerTypeId)
    : ContainerTypeException(containerTypeId, $"Container type {containerTypeId} cannot be deleted because it contains containers.");

public class UnhandledContainerTypeException(
    int containerTypeId, 
    Exception? innerException = null)
    : ContainerTypeException(containerTypeId, "Unexpected error occurred", innerException);