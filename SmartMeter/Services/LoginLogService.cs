using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.Models;

namespace SmartMeter.Services
{
    public class LoginLogService : ILoginLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Basic logging methods (unchanged)
        public async Task LogUserLoginAttemptAsync(string username, string attemptResult, long? userId = null, string? ipAddress = null, string? userAgent = null, string? additionalInfo = null)
        {
            var log = new LoginLog
            {
                UserType = "User",
                UserId = userId,
                Identifier = username,
                AttemptResult = attemptResult,
                IpAddress = ipAddress ?? GetClientIpAddress(),
                UserAgent = userAgent ?? GetUserAgent(),
                AttemptTime = DateTime.UtcNow,
                AdditionalInfo = additionalInfo
            };

            _context.LoginLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogConsumerLoginAttemptAsync(string email, string attemptResult, long? consumerId = null, string? ipAddress = null, string? userAgent = null, string? additionalInfo = null)
        {
            var log = new LoginLog
            {
                UserType = "Consumer",
                ConsumerId = consumerId,
                Identifier = email,
                AttemptResult = attemptResult,
                IpAddress = ipAddress ?? GetClientIpAddress(),
                UserAgent = userAgent ?? GetUserAgent(),
                AttemptTime = DateTime.UtcNow,
                AdditionalInfo = additionalInfo
            };

            _context.LoginLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        // Comprehensive query methods
        public async Task<List<LoginLog>> GetLoginLogsAsync(string? userType = null, string? identifier = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.LoginLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userType))
                query = query.Where(l => l.UserType == userType);

            if (!string.IsNullOrEmpty(identifier))
                query = query.Where(l => l.Identifier == identifier);

            if (fromDate.HasValue)
                query = query.Where(l => l.AttemptTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.AttemptTime <= toDate.Value);

            return await query
                .OrderByDescending(l => l.AttemptTime)
                .ToListAsync();
        }

        public async Task<List<LoginLog>> GetRecentFailedLoginsAsync(int hours = 24)
        {
            var fromDate = DateTime.UtcNow.AddHours(-hours);
            return await _context.LoginLogs
                .Where(l => l.AttemptTime >= fromDate && l.AttemptResult != "Success")
                .OrderByDescending(l => l.AttemptTime)
                .ToListAsync();
        }

        public async Task<object> GetUserLoginHistoryAsync(string identifier)
        {
            var logs = await _context.LoginLogs
                .Where(l => l.Identifier == identifier && l.UserType == "User")
                .OrderByDescending(l => l.AttemptTime)
                .ToListAsync();

            return new
            {
                UserType = "User",
                Identifier = identifier,
                TotalAttempts = logs.Count,
                SuccessfulAttempts = logs.Count(l => l.AttemptResult == "Success"),
                FailedAttempts = logs.Count(l => l.AttemptResult != "Success"),
                LastAttempt = logs.FirstOrDefault()?.AttemptTime,
                LoginHistory = logs.Take(100).ToList() // Last 100 attempts
            };
        }

        public async Task<object> GetConsumerLoginHistoryAsync(string email)
        {
            var logs = await _context.LoginLogs
                .Where(l => l.Identifier == email && l.UserType == "Consumer")
                .OrderByDescending(l => l.AttemptTime)
                .ToListAsync();

            return new
            {
                UserType = "Consumer",
                Identifier = email,
                TotalAttempts = logs.Count,
                SuccessfulAttempts = logs.Count(l => l.AttemptResult == "Success"),
                FailedAttempts = logs.Count(l => l.AttemptResult != "Success"),
                LastAttempt = logs.FirstOrDefault()?.AttemptTime,
                LoginHistory = logs.Take(100).ToList() // Last 100 attempts
            };
        }

        // NEW: Get all user logins
        public async Task<List<LoginLog>> GetAllUserLoginsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.LoginLogs
                .Where(l => l.UserType == "User");

            if (fromDate.HasValue)
                query = query.Where(l => l.AttemptTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.AttemptTime <= toDate.Value);

            return await query
                .OrderByDescending(l => l.AttemptTime)
                .ToListAsync();
        }

        // NEW: Get all consumer logins
        public async Task<List<LoginLog>> GetAllConsumerLoginsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.LoginLogs
                .Where(l => l.UserType == "Consumer");

            if (fromDate.HasValue)
                query = query.Where(l => l.AttemptTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.AttemptTime <= toDate.Value);

            return await query
                .OrderByDescending(l => l.AttemptTime)
                .ToListAsync();
        }

        // NEW: Get failed user logins
        public async Task<List<LoginLog>> GetFailedUserLoginsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.LoginLogs
                .Where(l => l.UserType == "User" && l.AttemptResult != "Success");

            if (fromDate.HasValue)
                query = query.Where(l => l.AttemptTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.AttemptTime <= toDate.Value);

            return await query
                .OrderByDescending(l => l.AttemptTime)
                .ToListAsync();
        }

        // NEW: Get failed consumer logins
        public async Task<List<LoginLog>> GetFailedConsumerLoginsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.LoginLogs
                .Where(l => l.UserType == "Consumer" && l.AttemptResult != "Success");

            if (fromDate.HasValue)
                query = query.Where(l => l.AttemptTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.AttemptTime <= toDate.Value);

            return await query
                .OrderByDescending(l => l.AttemptTime)
                .ToListAsync();
        }

        // Statistics method
        public async Task<object> GetLoginStatsAsync(int hours = 24)
        {
            var fromDate = DateTime.UtcNow.AddHours(-hours);
            var logs = await _context.LoginLogs
                .Where(l => l.AttemptTime >= fromDate)
                .ToListAsync();

            var userLogs = logs.Where(l => l.UserType == "User").ToList();
            var consumerLogs = logs.Where(l => l.UserType == "Consumer").ToList();

            return new
            {
                TimePeriodHours = hours,
                TotalAttempts = logs.Count,

                UserStats = new
                {
                    TotalAttempts = userLogs.Count,
                    Successful = userLogs.Count(l => l.AttemptResult == "Success"),
                    Failed = userLogs.Count(l => l.AttemptResult != "Success"),
                    FailureRate = userLogs.Count > 0 ? (double)userLogs.Count(l => l.AttemptResult != "Success") / userLogs.Count * 100 : 0
                },

                ConsumerStats = new
                {
                    TotalAttempts = consumerLogs.Count,
                    Successful = consumerLogs.Count(l => l.AttemptResult == "Success"),
                    Failed = consumerLogs.Count(l => l.AttemptResult != "Success"),
                    FailureRate = consumerLogs.Count > 0 ? (double)consumerLogs.Count(l => l.AttemptResult != "Success") / consumerLogs.Count * 100 : 0
                },

                CommonFailReasons = logs
                    .Where(l => l.AttemptResult != "Success")
                    .GroupBy(l => new { l.UserType, l.AttemptResult })
                    .Select(g => new
                    {
                        UserType = g.Key.UserType,
                        Reason = g.Key.AttemptResult,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList()
            };
        }

        private string? GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
        }
    }
}