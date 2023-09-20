using UnityEngine;
using UnityEngine.Assertions;

namespace Appegy.UniLogger.Example
{
    public class ExceptionInAssert : MonoBehaviour
    {
        private void Start()
        {
            Assert.IsTrue(false, "Test Assert");
        }
    }
}