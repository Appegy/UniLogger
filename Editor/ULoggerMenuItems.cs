using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Appegy.UniLogger
{
    public static class ULoggerMenuItems
    {
        [MenuItem("Tools/UniLogger/Show Logs", false, 1)]
        public static void OpenLogsFolder()
        {
            var path = $"{Application.persistentDataPath}/Logs";

            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                Process.Start(Application.persistentDataPath);
            }
        }
    }
}