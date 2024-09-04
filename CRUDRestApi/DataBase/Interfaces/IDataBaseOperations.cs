using CRUDRestApi.DataBase.Models;
using CRUDRestApi.Models;

namespace CRUDRestApi.DataBase.Interfaces
{
    public interface IDataBaseOperations
    {
        
        public Task<bool> InsertUser(User user);
        public Task<User> GetById(int id);
        public Task<bool> DeleteUser(int id);
        public Task<bool> ChangeUserValue(int userId, string newValue, string column);
        public Task HistoryChange(int id, string oldValue, string newValue);
        public Task<bool> IsFieldInUse(string fieldName, string fieldValue);

    }
}
