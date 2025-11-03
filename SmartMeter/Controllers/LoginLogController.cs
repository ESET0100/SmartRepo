using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeter.Services;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")] // Only users can view logs
    public class LoginLogController : ControllerBase
    {
        private readonly ILoginLogService _loginLogService;

        public LoginLogController(ILoginLogService loginLogService)
        {
            _loginLogService = loginLogService;
        }

        // 1. Get all login history (both users and consumers)
        [HttpGet]
        public async Task<IActionResult> GetAllLoginLogs(
            [FromQuery] string? userType = null,
            [FromQuery] string? identifier = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var logs = await _loginLogService.GetLoginLogsAsync(userType, identifier, fromDate, toDate);

            // Simple pagination
            var totalCount = logs.Count;
            var pagedLogs = logs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Filters = new { userType, identifier, fromDate, toDate },
                Logs = pagedLogs
            });
        }

        // 2. Get all user logins only
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUserLogins(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var logs = await _loginLogService.GetAllUserLoginsAsync(fromDate, toDate);

            var totalCount = logs.Count;
            var pagedLogs = logs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                UserType = "User",
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Filters = new { fromDate, toDate },
                Logs = pagedLogs
            });
        }

        // 3. Get all consumer logins only
        [HttpGet("consumers")]
        public async Task<IActionResult> GetAllConsumerLogins(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var logs = await _loginLogService.GetAllConsumerLoginsAsync(fromDate, toDate);

            var totalCount = logs.Count;
            var pagedLogs = logs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                UserType = "Consumer",
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Filters = new { fromDate, toDate },
                Logs = pagedLogs
            });
        }

        // 4. Get failed user logins only
        [HttpGet("users/failed")]
        public async Task<IActionResult> GetFailedUserLogins(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var logs = await _loginLogService.GetFailedUserLoginsAsync(fromDate, toDate);

            var totalCount = logs.Count;
            var pagedLogs = logs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                UserType = "User",
                AttemptResult = "Failed",
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Filters = new { fromDate, toDate },
                Logs = pagedLogs
            });
        }

        // 5. Get failed consumer logins only
        [HttpGet("consumers/failed")]
        public async Task<IActionResult> GetFailedConsumerLogins(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var logs = await _loginLogService.GetFailedConsumerLoginsAsync(fromDate, toDate);

            var totalCount = logs.Count;
            var pagedLogs = logs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                UserType = "Consumer",
                AttemptResult = "Failed",
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Filters = new { fromDate, toDate },
                Logs = pagedLogs
            });
        }

        // 6. Get login history for specific user
        [HttpGet("users/{identifier}")]
        public async Task<IActionResult> GetUserLoginHistory(string identifier)
        {
            var history = await _loginLogService.GetUserLoginHistoryAsync(identifier);
            return Ok(history);
        }

        // 7. Get login history for specific consumer
        [HttpGet("consumers/{email}")]
        public async Task<IActionResult> GetConsumerLoginHistory(string email)
        {
            var history = await _loginLogService.GetConsumerLoginHistoryAsync(email);
            return Ok(history);
        }

        // 8. Get recent failed logins (both users and consumers)
        [HttpGet("recent-failures")]
        public async Task<IActionResult> GetRecentFailedLogins([FromQuery] int hours = 24)
        {
            var failedLogs = await _loginLogService.GetRecentFailedLoginsAsync(hours);

            return Ok(new
            {
                TimePeriodHours = hours,
                TotalFailedAttempts = failedLogs.Count,
                UserFailed = failedLogs.Count(l => l.UserType == "User"),
                ConsumerFailed = failedLogs.Count(l => l.UserType == "Consumer"),
                FailedLogs = failedLogs
            });
        }

        // 9. Get comprehensive login statistics
        [HttpGet("stats")]
        public async Task<IActionResult> GetLoginStats([FromQuery] int hours = 24)
        {
            var stats = await _loginLogService.GetLoginStatsAsync(hours);
            return Ok(stats);
        }

        // 10. Search logs by identifier with user type filter
        [HttpGet("search")]
        public async Task<IActionResult> SearchLogs(
            [FromQuery] string identifier,
            [FromQuery] string? userType = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var logs = await _loginLogService.GetLoginLogsAsync(userType, identifier, fromDate, toDate);

            return Ok(new
            {
                SearchIdentifier = identifier,
                UserType = userType ?? "All",
                TotalResults = logs.Count,
                Filters = new { userType, fromDate, toDate },
                Results = logs
            });
        }
    }
}