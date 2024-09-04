namespace CRUDRestApi.DataBase.Exceptions
{
    public class InvalidIdException : Exception
    {
        public InvalidIdException(int id)
            : base($"Provided ID ({id}) is not valid. It must be greater than 0.") { }
        
    }

    public class ConnectionStringMissingException : Exception
    {
        public ConnectionStringMissingException()
            : base("No connection string provided.") { }
    }

    public class FieldInUseException : Exception
    {
        public FieldInUseException(string fieldName, string fieldValue)
            : base($"{fieldName} '{fieldValue}' is already in use.") { }
    }
}
