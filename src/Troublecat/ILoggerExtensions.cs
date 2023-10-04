using Microsoft.Extensions.Logging;

namespace Troublecat;

public class LoggerVerifyException : Exception {
    public LoggerVerifyException(string? message, Exception? innerException = null) : base(message, innerException) {}
}

public static class ILoggerExtensions {
    public static void Verify(this ILogger logger, bool condition, string? messageIfFalse = null, params object?[] args) {
        if (!condition) {
            messageIfFalse = messageIfFalse ?? "LoggerVerifyError condition failed during Verify call.";
            logger.LogError(messageIfFalse, args);
            throw new LoggerVerifyException(messageIfFalse);
        }
    }
}
