using Microsoft.Extensions.Logging;

namespace ERDMCore.Infrastructure.Logging
{
    public class SerilogLoggerService : ILoggerService
    {
        private readonly ILogger<SerilogLoggerService> _logger;

        public SerilogLoggerService(ILogger<SerilogLoggerService> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message, params object[] args)
            => _logger.LogInformation(message, args);

        public void LogWarning(string message, params object[] args)
            => _logger.LogWarning(message, args);

        public void LogError(Exception exception, string message, params object[] args)
            => _logger.LogError(exception, message, args);

        public void LogDebug(string message, params object[] args)
            => _logger.LogDebug(message, args);

        public void LogCritical(Exception exception, string message, params object[] args)
            => _logger.LogCritical(exception, message, args);

        public IDisposable BeginScope<TState>(TState state)
            => _logger.BeginScope(state);
    }
}
