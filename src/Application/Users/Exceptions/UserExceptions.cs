namespace Application.Users.Exceptions;

public abstract class UserException : Exception
{
    protected UserException(string message) : base(message) { }
}

public class UserNotFoundException : UserException
{
    public UserNotFoundException(Guid userId)
        : base($"User with ID {userId} was not found.") { }
        
    public UserNotFoundException(string email)
        : base($"User with email {email} was not found.") { }
}

public class UserAlreadyExistsException : UserException
{
    public UserAlreadyExistsException(string email)
        : base($"User with email {email} already exists.") { }
}

public class UserNotActiveException : UserException
{
    public UserNotActiveException(Guid userId)
        : base($"User {userId} is not active.") { }
}

public class InvitationAlreadyExistsException : UserException
{
    public InvitationAlreadyExistsException(string email)
        : base($"An active invitation for {email} already exists.") { }
}

public class SelfDeactivationException : UserException
{
    public SelfDeactivationException()
        : base("You cannot deactivate your own account.") { }
}
