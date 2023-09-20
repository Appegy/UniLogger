using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Appegy.UniLogger.Example
{
    public class ExceptionInThread : MonoBehaviour
    {
        private static readonly ULogger _logger = ULogger.GetLogger("ExceptionInThread");

        [SerializeField]
        private float _throwAfter = 3f;

        private void Start()
        {
            Task.Run(ThrowTask);
        }

        private async Task ThrowTask()
        {
            await Task.Delay(TimeSpan.FromSeconds(_throwAfter));
            _logger.Log("Throwing from threaded task");
            
            // Won't be showed by Unity's default logger
            throw new Exception("Exception from threaded task");
        }
    }
}