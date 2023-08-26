using Rectangle = SharpDX.Mathematics.Interop.RawRectangle;

namespace adrilight_shared.Extensions
{
    public static class RawRectangleExtensions
    {
        public static int GetWidth(this Rectangle rect)
        {
            return rect.Right - rect.Left;
        }
        public static int GetHeight(this Rectangle rect)
        {
            return rect.Bottom - rect.Top;
        }
    }
}
