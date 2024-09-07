using CRUDRestApi.DataBase.Exceptions;
using CRUDRestApi.DataBase.Interfaces;
using CRUDRestApi.DataBase.Models;
using CRUDRestApi.Models;
using Dapper;
using Npgsql;
using System.Reflection;
using System.Threading.Channels;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace CRUDRestApi.DataBase
{
    public class DataBaseOperations : IDataBaseOperations
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataBaseOperations> _logger;   
        private string _connectionString {get;init;}
        private readonly DataAccessException _dataAccessException;
        public DataBaseOperations(IConfiguration configuration, ILogger<DataBaseOperations> logger, DataAccessException dataAccessException)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
            _dataAccessException = dataAccessException;
        }

        public void CheckConnectionString()
        {
            if(string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("No connection string provided");
                throw new ConnectionStringMissingException();
            }
        }

        public async Task<bool> InsertUser(User model)
        {
            CheckConnectionString();
            try
            {
                var sql = @"INSERT INTO users_info (first_name, last_name, user_name, email, user_hashpassword, date)
                    VALUES (@FirstName, @LastName, @UserName, @Email, @Password, @Date)";

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var result = await connection.ExecuteAsync(sql,model);
                    return result > 0;
                }
            }
            catch (Exception exception)
            {
                _dataAccessException.Handle(exception);
                return false;
            }
        }

        public async Task<bool> IsFieldInUse(string fieldName, string fieldValue)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = $"SELECT COUNT(1) FROM users_info WHERE {fieldName} = @FieldValue";
                var emailExists = await connection.ExecuteScalarAsync<int>(query, new { FieldValue = fieldValue });
                return emailExists > 0;
            }
        }

        public async Task<User> GetById(int id)
        {
            CheckConnectionString();

            if (id <= 0)
            {
                throw new InvalidIdException(id);
            }

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var sql = @"SELECT user_id AS User_Id, 
                               first_name AS FirstName, 
                               last_name AS LastName, 
                               user_name AS UserName, 
                               email, 
                               user_hashpassword AS Password, 
                               date 
                        FROM users_info 
                        WHERE user_id = @Id";

                    var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });

                    if (user == null)
                    {
                        _logger.LogWarning("User with ID {Id} not found.", id);
                        return null; 
                    }

                    _logger.LogInformation("User with ID {Id} retrieved successfully.", id);
                    return user; 
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while retrieving the user with ID {Id}.", id);
                throw; 
            }
        }
        public async Task<IEnumerable<UserHistoryChange>> GetByIdHistoyChanges(int id)
        {
            CheckConnectionString();
            if (id <= 0)
            {
                throw new InvalidIdException(id);
            }
            try
            {
                using(var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var sql = @"SELECT change_id AS Change_Id, 
                               User_Id AS User_Id, 
                               change_column AS ChangedColumn, 
                               old_value AS OldValue, 
                               new_value AS NewValue, 
                               change_date AS DateTime 
                        FROM user_changes_history 
                        WHERE User_Id = @User_Id
                        ORDER BY change_date DESC";
                    var result = await connection.QueryAsync<UserHistoryChange>(sql, new { Id = id });
                    if(!result.Any())
                    {
                        _logger.LogWarning($"No changes found for user with ID {id}.");
                        return null;
                    }
                    _logger.LogInformation($"Changes for user with ID {id} retrieved successfully.");
                    return result;
                }
                
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"An error occurred while retrieving changes for user with ID {id}.");
                throw;
            }
        }

        public async Task<bool> DeleteUser(int id)
        {
            if (id <= 0) throw new InvalidIdException(id);
            CheckConnectionString();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var sql = "DELETE FROM users_info WHERE user_id = @id";
                    await connection.ExecuteAsync(await DeleteUserFromHistory(id), new { Id = id }); // Delete from history changes
                    var result = await connection.ExecuteAsync(sql, new { Id = id });
                    return result > 0;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"An error occurred when deleting the user with ID {id}.");
                throw;
            }
        }
        public async Task<string> DeleteUserFromHistory(int id)
        {
            var sql = "DELETE FROM user_changes_history WHERE user_id = @Id";
            return sql;
        }

        public async Task<bool> ChangeUserValue(int userId, string newValue, string column)
        {
            CheckConnectionString();

            if (string.IsNullOrEmpty(newValue) || string.IsNullOrEmpty(column))
            {
                _logger.LogWarning("Invalid value or column");
                throw new Exception("Invalid value or column");
            }

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    if (column.Equals("email", StringComparison.OrdinalIgnoreCase) ||
                        column.Equals("user_name", StringComparison.OrdinalIgnoreCase))
                    {
                        if (await IsFieldInUse(column, newValue))
                        {
                            _logger.LogWarning($"{column} {newValue} is already in use.");
                            return false;
                        }
                    }

                    var oldValue = await connection.ExecuteScalarAsync<string>(
                        $"SELECT {column} FROM users_info WHERE user_id = @UserId",
                        new { UserId = userId });

                    if (oldValue == newValue)
                    {
                        _logger.LogInformation($"No changes detected for user ID {userId}. {column} remains the same.");
                        return false;
                    }

                    var sql = $"UPDATE users_info SET {column} = @NewValue WHERE user_id = @UserId";
                    var result = await connection.ExecuteAsync(sql, new { NewValue = newValue, UserId = userId });

                    if (result > 0)
                    {
                        _logger.LogInformation($"User with ID {userId} successfully updated. {column} changed to {newValue}.");
                        await HistoryChange(userId, column, newValue, oldValue); 
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"Update operation for user with ID {userId} affected 0 rows.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user information.");
                return false;
            }
        }


        public async Task HistoryChange(int userId, string column, string newValue, string oldValue)
        {
            CheckConnectionString();

            if (string.IsNullOrEmpty(newValue) || string.IsNullOrEmpty(column))
            {
                _logger.LogWarning("Invalid value or column");
                throw new Exception("Invalid value or column");
            }

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var userHistory = new UserHistoryChange
                    {
                        User_Id = userId,
                        ChangedColumn = column,
                        OldValue = oldValue,
                        NewValue = newValue,
                        DateTime = DateTime.Now
                    };

                    var sqlHistory = @"INSERT INTO user_changes_history (user_id, change_column, old_value, new_value, change_date)
                       VALUES (@user_id, @ChangedColumn, @OldValue, @NewValue, @DateTime)";

                    var result = await connection.ExecuteAsync(sqlHistory, userHistory);

                    if (result > 0)
                    {
                        _logger.LogInformation($"Change recorded in history for user ID {userId}. {column} changed from {oldValue} to {newValue}.");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to record change in history for user ID {userId}.");
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while recording the change history.");
            }
        }


    }
}
