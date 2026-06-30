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

        private async void Start()
        {
            // ThrowTask runs on a thread-pool thread (the log inside it comes from a background thread).
            // Awaiting it observes the faulted task and surfaces the exception on the main thread right
            // away, instead of waiting for the GC to finalize an unobserved task.
            await Task.Run(ThrowTask);
        }

        private async Task ThrowTask()
        {
            await Task.Delay(TimeSpan.FromSeconds(_throwAfter));
            _logger.Log("Throwing from threaded task");
            throw new Exception("Exception from threaded task");
        }
    }
}
