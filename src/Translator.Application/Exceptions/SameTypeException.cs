namespace Translator.Application.Exceptions;

public class SameTypeException : ApplicationLayerException
{
    public SameTypeException() 
        : base(ErrorCodes.BadRequest, $"Parent and Child Categories Cannot Have Same Type.") { }
}