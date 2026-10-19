using System.Diagnostics;
using Debug = UnityEngine.Debug;

public static class GameLogger
{
    [Conditional("ENABLE_LOG")]
    public static void Log(object message)
    {
        Debug.Log(message);
    }

    [Conditional("ENABLE_LOG")]
    public static void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }

    [Conditional("ENABLE_LOG")]
    public static void LogError(object message)
    {
        Debug.LogError(message);
    }
}