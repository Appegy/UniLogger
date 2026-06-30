using UnityEditor;

namespace Appegy.UniLogger
{
    // Keeps the console row double-click handler registered for the whole editor session (surviving
    // play-mode exit and domain reloads), and pumps background-thread logs onto the main thread so they
    // become real, double-clickable console entries.
    internal static class UnityConsoleBridgeInstaller
    {
        [InitializeOnLoadMethod]
        private static void Install()
        {
            UnityConsoleBridge.EnsureDoubleClickHandlerRegistered();
            EditorApplication.update -= UnityConsoleBridge.DrainPending;
            EditorApplication.update += UnityConsoleBridge.DrainPending;
        }
    }
}
