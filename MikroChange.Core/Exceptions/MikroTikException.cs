namespace MikroChange.Core.Exceptions;


public class MikroTikException : Exception
{
    public MikroTikException(string message) : base(message)
    {
    }

    public MikroTikException(string message, Exception inner) : base(message, inner)
    {
    }
}
