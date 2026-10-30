using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Game.Runtime
{
    public static class GameLog
    {
        // [Conditional] removes the call *and its string argument* when ENABLE_LOGS is not defined.
        [Conditional("ENABLE_LOGS"), Conditional("UNITY_EDITOR")]
        public static void Info(string system, string message) => Debug.Log($"[{system}] {message}");

        [Conditional("ENABLE_LOGS"), Conditional("UNITY_EDITOR")]
        public static void Warn(string system, string message) => Debug.LogWarning($"[{system}] {message}");

        // Errors are always logged — they feed crash reporting.
        public static void Error(string system, string message) => Debug.LogError($"[{system}] {message}");
    }
}
