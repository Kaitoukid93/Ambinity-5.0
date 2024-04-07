using System;

namespace adrilight.Services.CaptureEngine.ScreenCapture
{
    [Serializable]
    public class DesktopDuplicationException : Exception
    {
        public DesktopDuplicationException(string message)
            : base(message) { }
        public DesktopDuplicationException(string message, Exception innerException)
                    : base(message, innerException) { }


    }
}
