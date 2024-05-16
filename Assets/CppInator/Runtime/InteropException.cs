using System;

namespace CppInator.Runtime
{
    public class InteropException : Exception
    {
        readonly System.Diagnostics.StackTrace _trace;

        public InteropException(string message) : base(message) 
        {
#if UNITY_EDITOR
            _trace = new System.Diagnostics.StackTrace(4, true);
#else
            _trace = new System.Diagnostics.StackTrace(3, true);
#endif
        }
        public override string ToString() => Message;

        public override string StackTrace => _trace.ToString();
    }
}
