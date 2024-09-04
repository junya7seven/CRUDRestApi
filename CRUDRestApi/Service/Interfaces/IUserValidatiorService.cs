namespace CRUDRestApi.Service.Interfaces
{
    public interface IUserValidatiorService
    {
        Task<bool> isValueInUse(string fieldName, string fieldValue);
    }
}
