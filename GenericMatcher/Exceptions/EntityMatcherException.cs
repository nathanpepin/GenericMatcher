namespace GenericMatcher.Exceptions;

public class EntityMatcherException : Exception
{
    protected EntityMatcherException(string message) : base(message)
    {
    }

    public EntityMatcherException(string message, Exception inner) : base(message, inner)
    {
    }
}