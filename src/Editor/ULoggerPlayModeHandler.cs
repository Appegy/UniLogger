using UnityEditor;
using UnityEngine;

namespace Appegy.UniLogger
{
    public static class ULoggerPlayModeHandler
    {
        [InitializeOnLoadMethod]
        private static void Subscribe()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    ULogger.Terminate();
                    break;
            }
        }
    }
}