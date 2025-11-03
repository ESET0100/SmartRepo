using SmartMeter.Models;

namespace SmartMeter.Services
{
    public interface ILoginLogService
    {
        // Basic logging methods
        Task LogUserLoginAttemptAsync(string username, string attemptResult, long? userId = null, string? ipAddress = null, string? userAgent = null, string? additionalInfo = null);
        Task LogConsumerLoginAttemptAsync(string email, string attemptResult, long? consumerId = null, string? ipAddress = null, string? userAgent = null, string? additionalInfo = null);

        // Comprehensive query methods
        Task<List<LoginLog>> GetLoginLogsAsync(string? userType = null, string? identifier = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<LoginLog>> GetRecentFailedLoginsAsync(int hours = 24);
        Task<object> GetUserLoginHistoryAsync(string identifier);
        Task<object> GetConsumerLoginHistoryAsync(string email);

        // New specialized methods
        Task<List<LoginLog>> GetAllUserLoginsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<LoginLog>> GetAllConsumerLoginsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<LoginLog>> GetFailedUserLoginsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<LoginLog>> GetFailedConsumerLoginsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // Statistics
        Task<object> GetLoginStatsAsync(int hours = 24);
    }
}