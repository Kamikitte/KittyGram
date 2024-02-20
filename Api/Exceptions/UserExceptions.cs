namespace Api.Exceptions;

public class NotFoundException : Exception
{
    public string? Model { get; set; }
    public override string Message => $"{Model} is not found";
}

public sealed class UserNotFoundException : NotFoundException
{
    public UserNotFoundException()
    {
        Model = "User";
    }
}

public sealed class PostNotFoundException : NotFoundException
{
    public PostNotFoundException()
    {
        Model = "Post";
    }
}