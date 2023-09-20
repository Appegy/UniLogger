using System;
using UnityEngine;

namespace Appegy.UniLogger.Example
{
    public class ExceptionInMethod : MonoBehaviour
    {
        private void Start()
        {
            ThrowMethod();
        }

        private void ThrowMethod()
        {
            throw new Exception("Throw exception in method");
        }
    }
}