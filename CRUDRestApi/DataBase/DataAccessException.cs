using System.Net.Mail;

namespace CRUDRestApi.DataBase
{
    public class DataAccessException
    {
        private readonly ILogger<DataAccessException> _logger;
        public DataAccessException(ILogger<DataAccessException> logger)
        {
            _logger = logger;
        }

        public void Handle(Exception ex, string message = null, LogLevel severityLevel = LogLevel.Error)
        {
            LogException(ex, message, severityLevel);
        }
        private void LogException(Exception ex, string progmessage, LogLevel severityLevel)
        {
            var message = progmessage ?? "An unexpected error occurred.";

            switch (ex)
            {
                case ArgumentNullException _:
                case ArgumentException _:
                    _logger.LogWarning(ex, message);
                    break;

                case InvalidOperationException _:
                    _logger.LogError(ex, message);
                    break;

                case Npgsql.NpgsqlException _:
                    _logger.LogError(ex, "A database error occurred. " + message);
                    break;

                default:
                    _logger.LogError(ex, message);
                    break;
            }
        }

    }
}
