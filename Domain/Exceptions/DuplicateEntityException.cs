namespace CONEX_APP.Domain.Exceptions;

public class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(string message) : base(message)
    {
    }
}
