using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Appegy.UniLogger.Example
{
    public class ExceptionInMethod : MonoBehaviour
    {
        private void Start()
        {
            ThrowMethod();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowMethod()
        {
            throw new Exception("Throw exception in method");
        }
    }
}