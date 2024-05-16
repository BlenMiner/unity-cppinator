using System.Runtime.InteropServices;
using UnityEngine;

namespace CppInator.Runtime
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeV3
    {
        public float x;
        public float y;
        public float z;
        
        public NativeV3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        public NativeV3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
        
        public static implicit operator Vector3(NativeV3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
        
        public static implicit operator NativeV3(Vector3 v)
        {
            return new NativeV3(v);
        }
        
        public static NativeV3 operator +(NativeV3 a, NativeV3 b)
        {
            return new NativeV3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        
        public static NativeV3 operator -(NativeV3 a, NativeV3 b)
        {
            return new NativeV3(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        
        public static NativeV3 operator *(NativeV3 a, float b)
        {
            return new NativeV3(a.x * b, a.y * b, a.z * b);
        }
        
        public static NativeV3 operator *(float a, NativeV3 b)
        {
            return new NativeV3(a * b.x, a * b.y, a * b.z);
        }
        
        public static NativeV3 operator /(NativeV3 a, float b)
        {
            return new NativeV3(a.x / b, a.y / b, a.z / b);
        }
        
        public static NativeV3 operator -(NativeV3 a)
        {
            return new NativeV3(-a.x, -a.y, -a.z);
        }
        
        public static bool operator ==(NativeV3 a, NativeV3 b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }
        
        public static bool operator !=(NativeV3 a, NativeV3 b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is NativeV3 v3)
            {
                return this == v3;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }
        
        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
}
