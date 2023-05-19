using System.Runtime.InteropServices;

namespace AssetRenderer.Helper
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Color24
    {
        [FieldOffset(0)]
        public int rgb;
        [FieldOffset(0)]
        public byte r;
        [FieldOffset(1)]
        public byte g;
        [FieldOffset(2)]
        public byte b;
    }
}