using System;
using UnityEngine;

namespace Appegy.UniLogger.Example
{
    public class ExceptionLogging : MonoBehaviour
    {
        private static readonly ULogger _logger1 = ULogger.GetLogger(ExampleLogs.Tag1);
        
        private void Start()
        {
            ThrowMethod();
        }

        private void ThrowMethod()
        {
            Debug.LogException(new Exception("Custom exception"), this);
            try
            {
                throw new Exception("Throw, catch and log exception");
            }
            catch (Exception e)
            {
                _logger1.LogException(e, this);
                Debug.LogException(e, this);
            }
            
            throw new Exception("Throw exception");
        }
    }
}