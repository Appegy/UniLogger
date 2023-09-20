using System.Runtime.Serialization;
using UnityEngine;

namespace Appegy.UniLogger.Example
{
    [LoggerTagsContainer("Example")]
    public enum ExampleLogs
    {
        Tag1,
        [EnumMember(Value = "RenamedTag2")]
        Tag2,
    }
    
    public class CategorizedLogs : MonoBehaviour
    {
        private static readonly ULogger _logger1 = ULogger.GetLogger(ExampleLogs.Tag1);
        private static readonly ULogger _logger2 = ULogger.GetLogger(ExampleLogs.Tag2);
        private static readonly ULogger _logger3 = ULogger.GetLogger("MyCat", "Custom"); 
        
        private void Start()
        {
            CallLogsInMethod();
        }

        private void CallLogsInMethod()
        {
            _logger1.Log("Example categorized log", this);
            _logger2.Log("Example categorized log", this);
            _logger3.Log("Example custom log in default category", this);
        }
    }
}