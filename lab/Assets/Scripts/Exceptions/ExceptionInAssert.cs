using UnityEngine;
using UnityEngine.Assertions;

namespace Appegy.UniLogger.Example
{
    public class ExceptionInAssert : MonoBehaviour
    {
        private void Start()
        {
            Debug.Assert(false, "Test Assert");
            Assert.IsTrue(false, "Test Assert");
        }
    }
}