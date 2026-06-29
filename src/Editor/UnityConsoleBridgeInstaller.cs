using UnityEditor;

namespace Appegy.UniLogger
{
    // Keeps the console row double-click handler registered for the whole editor session, including
    // after play-mode exit and across domain reloads, so any of our console entries stay navigable.
    internal static class UnityConsoleBridgeInstaller
    {
        [InitializeOnLoadMethod]
        private static void Install()
        {
            UnityConsoleBridge.EnsureDoubleClickHandlerRegistered();
        }
    }
}
