using System;
using System.Collections;
using UnityEngine;

namespace Appegy.UniLogger.Example
{
    public class ExceptionInCoroutine : MonoBehaviour
    {
        private static readonly ULogger _logger = ULogger.GetLogger("ExceptionInCoroutine");

        [SerializeField]
        private float _throwAfter = 1f;

        private void Start()
        {
            StartCoroutine(ThrowRoutine());
        }

        private IEnumerator ThrowRoutine()
        {
            yield return new WaitForSeconds(_throwAfter);
            _logger.Log("Throwing from coroutine");
            throw new Exception("Exception from coroutine");
        }
    }
}