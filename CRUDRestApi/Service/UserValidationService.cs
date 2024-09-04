using CRUDRestApi.DataBase.Interfaces;
using CRUDRestApi.Service.Interfaces;

namespace CRUDRestApi.Service
{
    public class UserValidationService : IUserValidatiorService
    {
        private readonly IDataBaseOperations _db;
        private readonly ILogger<UserValidationService> _logger;

        public UserValidationService(IDataBaseOperations db, ILogger<UserValidationService> logger)
        {
            _db = db;
            _logger = logger;
        }
        public async Task<bool> isValueInUse(string fieldName, string fieldValue)
        {
            try
            {
                switch (fieldName)
                {
                    case "email":
                        if(await _db.IsFieldInUse(fieldName,fieldValue))
                        {
                            _logger.LogWarning($"Email {fieldValue} is already in use.");

                            return true;
                        }
                        return false;
                    case "user_name":
                        if (await _db.IsFieldInUse(fieldName, fieldValue))
                        {
                            _logger.LogWarning($"User name {fieldValue} is already in use.");
                            return true;
                        }
                        return false;
                    default:
                        return true;
                }

            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while checking if the email is in use.");
                throw;
            }
        }
    }
}
