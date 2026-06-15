using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Appegy.UniLogger.Example
{
    public class ExceptionInTask : MonoBehaviour
    {
        private static readonly ULogger _logger = ULogger.GetLogger("ExceptionInThread");

        [SerializeField]
        private float _throwAfter = 2f;

        private async void Start()
        {
            await ThrowTask();
        }

        private async Task ThrowTask()
        {
            await Task.Delay(TimeSpan.FromSeconds(_throwAfter));
            _logger.Log("Throwing from async task");
            throw new Exception("Exception from async task");
        }
    }
}