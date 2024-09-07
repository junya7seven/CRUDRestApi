using CRUDRestApi.DataBase.Models;
using CRUDRestApi.Models;

namespace CRUDRestApi.Service.Interfaces
{
    public interface IUserService
    {
        Task<bool> InsertNewUser(UserModel model);
        Task<User> GetUserById(int id);
        Task<bool> DeleteUser(int id);
        Task<bool> UpdateUserField(int userId, string column, string newValue);
        Task<IEnumerable<UserHistoryChange>> GetUserChangesHistory(int id);
    }
}
