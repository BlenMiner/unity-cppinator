using System.Runtime.InteropServices;
using UnityEngine;

namespace CppInator.Runtime
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeV2
    {
        public float x;
        public float y;
        
        public NativeV2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        
        public NativeV2(Vector2 v)
        {
            x = v.x;
            y = v.y;
        }
        
        public static implicit operator Vector2(NativeV2 v)
        {
            return new Vector2(v.x, v.y);
        }
        
        public static implicit operator NativeV2(Vector2 v)
        {
            return new NativeV2(v);
        }
        
        public static NativeV2 operator +(NativeV2 a, NativeV2 b)
        {
            return new NativeV2(a.x + b.x, a.y + b.y);
        }
        
        public static NativeV2 operator -(NativeV2 a, NativeV2 b)
        {
            return new NativeV2(a.x - b.x, a.y - b.y);
        }
        
        public static NativeV2 operator *(NativeV2 a, float b)
        {
            return new NativeV2(a.x * b, a.y * b);
        }
        
        public static NativeV2 operator *(float a, NativeV2 b)
        {
            return new NativeV2(a * b.x, a * b.y);
        }
        
        public static NativeV2 operator /(NativeV2 a, float b)
        {
            return new NativeV2(a.x / b, a.y / b);
        }
        
        public static NativeV2 operator -(NativeV2 a)
        {
            return new NativeV2(-a.x, -a.y);
        }
        
        public static bool operator ==(NativeV2 a, NativeV2 b)
        {
            return a.x == b.x && a.y == b.y;
        }
        
        public static bool operator !=(NativeV2 a, NativeV2 b)
        {
            return a.x != b.x || a.y != b.y;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is NativeV2 v2)
            {
                return this == v2;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }
        
        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}