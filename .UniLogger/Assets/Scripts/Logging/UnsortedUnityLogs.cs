using UnityEngine;

namespace Appegy.UniLogger.Example
{
    public class UnsortedUnityLogs : MonoBehaviour
    {
        private void Start()
        {
            CallLogsInMethod();
        }

        private void CallLogsInMethod()
        {
            Debug.Log("Default log (Unity)", this);
            Debug.LogWarning("Default warning (Unity)", this);
            Debug.LogError("Default error (Unity)", this);
            Debug.LogAssertion("Default assertion (Unity)", this);
        }
    }
}