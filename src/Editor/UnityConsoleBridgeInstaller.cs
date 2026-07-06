using UnityEditor;

namespace Appegy.UniLogger
{
    internal static class UnityConsoleBridgeInstaller
    {
        [InitializeOnLoadMethod]
        private static void Install()
        {
            EditorReflection.Ensure();
            UnityConsoleBridge.EnsureDoubleClickHandlerRegistered();
            EditorApplication.update -= UnityConsoleBridge.DrainPending;
            EditorApplication.update += UnityConsoleBridge.DrainPending;
        }
    }
}
