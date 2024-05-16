using System;

namespace CppInator.Runtime
{
    public interface IMarshalled
    {
        void ToManaged(IntPtr ptr);
        
        IntPtr ToUnmanaged();
    }
}
