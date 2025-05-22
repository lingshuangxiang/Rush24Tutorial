using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace TwentyFour.Scripts.Utilities
{
    public class Logger : ILogger
    {
        private static readonly Logger LoggerInstance = new Logger();

        public void Log(LogType logType, object message)
        {
            switch (logType)
            {
                case LogType.Log:
                    Debug.Log(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Error:
                    Debug.LogError(message);
                    break;
            }
        }

        private void LogExceptionInternal(System.Exception ex)
        {
            Debug.LogException(ex);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void Log(object message)
        {
            LoggerInstance.Log(LogType.Log, message);
        }

        public static void LogInfo(object message)
        {
            LoggerInstance.Log(LogType.Log, message);
        }

        public static void LogWarning(object message)
        {
            LoggerInstance.Log(LogType.Warning, message);
        }

        public static void LogError(object message)
        {
            LoggerInstance.Log(LogType.Error, message);
        }

        public static void LogException(System.Exception ex)
        {
            LoggerInstance.LogExceptionInternal(ex);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void LogAssertions(object message)
        {
            Debug.LogAssertion(message);
        }
    }
}