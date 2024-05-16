using System.Runtime.InteropServices;
using CppInator.Runtime;
using UnityEngine;

public class ValidateMangedExport : MonoBehaviour
{
    [StructLayout(LayoutKind.Auto)]
    struct TestStruct
    {
        public int a;
        public bool x;
        public int b;
    }
    
    [DllImport("__Internal")]
    static extern int someExtertedFunction(TestStruct test);
 
    private void Awake()
    {
        var result = Native.Invoke(someExtertedFunction, new TestStruct
        {
            a = 69, b = 420, x = true
        });
        
        Debug.Log($"Result: {result}");
    }
}
