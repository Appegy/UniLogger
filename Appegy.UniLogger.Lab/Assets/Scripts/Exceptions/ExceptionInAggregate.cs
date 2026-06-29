using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Appegy.UniLogger.Example
{
    public class ExceptionInAggregate : MonoBehaviour
    {
        private void Start()
        {
            ULogger.LogException(BuildAggregate());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private AggregateException BuildAggregate()
        {
            try
            {
                ThrowInner();
            }
            catch (Exception inner)
            {
                return new AggregateException("Wrapped by aggregate", inner);
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowInner()
        {
            throw new InvalidOperationException("Real cause inside aggregate");
        }
    }
}
