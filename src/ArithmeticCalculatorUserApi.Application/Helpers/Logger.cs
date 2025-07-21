using Serilog;

namespace ArithmeticCalculatorUserApi.Application.Helpers
{
    public static class Logger
    {
        private static readonly ILogger _logger;

        static Logger()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }

        public static void Initialize()
        {
            Log.Logger = _logger;
        }

        public static void LogInformation(string message, params object[] args)
        {
            _logger.Information(message, args);
        }

        public static void LogWarning(string message, params object[] args)
        {
            _logger.Warning(message, args);
        }

        public static void LogError(string message, params object[] args)
        {
            _logger.Error(message, args);
        }

        public static void LogError(Exception ex, string message, params object[] args)
        {
            _logger.Error(ex, message, args);
        }
    }
}
