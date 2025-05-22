namespace TwentyFour.Scripts.Utilities
{
    public enum LogType
    {
        Error,
        Warning,
        Log,
    }

    public interface ILogger
    {
        void Log(LogType logType, object message);
    }
}
