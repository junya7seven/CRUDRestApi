using CRUDRestApi.DataBase.Interfaces;
using CRUDRestApi.DataBase.Models;
using CRUDRestApi.Models;
using CRUDRestApi.Service.Interfaces;

namespace CRUDRestApi.Service
{
    public class UserSerivce : IUserService
    {
        private readonly IDataBaseOperations _db;
        private readonly IUserValidatiorService _userValidatior;
        private readonly ILogger<UserSerivce> _logger;

        public UserSerivce(IDataBaseOperations db, IUserValidatiorService userValidatior, ILogger<UserSerivce> logger)
        {
            _db = db;
            _userValidatior = userValidatior;
            _logger = logger;
        }

        public async Task<bool> InsertNewUser(UserModel model)
        {
            if (await _userValidatior.isValueInUse("email", model.UserName))
            {
                _logger.LogWarning($"Email {model.UserName} is already in use.");
                return false;
            }
            if (await _userValidatior.isValueInUse("user_name", model.UserName))
            {
                _logger.LogWarning($"User Name {model.UserName} is already in use.");
                return false;
            }

            var user = new User
            {
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                date = DateTime.Now,
            };
            var result = await _db.InsertUser(user);
            if (result)
            {
                _logger.LogInformation($"User {model.UserName} was successfully inserted.");
            }
            return result;
        }

        public async Task<User> GetUserById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid user ID provided: {Id}", id);
                return null;
            }
            try
            {
                var result = await _db.GetById(id); 

                if (result == null)
                {
                    _logger.LogWarning("User with ID {Id} not found.", id);
                    return null;
                }

                return result; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the user with ID {Id}.", id);
                throw;
            }
        }
        public async Task<bool> DeleteUser(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid user ID provided: {Id}", id);
                return false;
            }
            try
            {
                var searchId = await _db.GetById(id);

                if (searchId == null)
                {
                    _logger.LogWarning("User with ID {Id} not found.", id);
                    return false;
                }
                var result = await _db.DeleteUser(id);
                if(!result)
                {
                    _logger.LogWarning("User could not be deleted");
                    return false;
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the user with ID {Id}.", id);
                throw;
            }
        }
        public async Task<bool> UpdateUserField(int userId, string column, string newValue)
        {
            if (string.IsNullOrEmpty(newValue) || string.IsNullOrEmpty(column))
            {
                _logger.LogWarning("Invalid value or column");
                return false;
            }

            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID provided: {UserId}", userId);
                return false;
            }

            return await _db.ChangeUserValue(userId, newValue, column);
        }

    }
}
