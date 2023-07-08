using adrilight.DesktopDuplication;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace adrilight.Util
{
    internal class Gif : ViewModelBase, IParameterValue
    {

        public Gif(string path)
        {
            Path = path;
        }
        public Gif()
        {

        }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        [JsonIgnore]
        public ByteFrame[] Frames { get; set; }
        public object Lock { get; } = new object();
        public void LoadGifFromDisk(string path)
        {
            if (path == null || !File.Exists(path))
            {
                Frames = null;
                return;
            }

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (System.Drawing.Image imageToLoad = System.Drawing.Image.FromStream(fs))
                    {
                        var frameDim = new FrameDimension(imageToLoad.FrameDimensionsList[0]);
                        var frameCount = imageToLoad.GetFrameCount(frameDim);
                        var gifFrames = new ByteFrame[frameCount];
                        for (int i = 0; i < frameCount; i++)
                        {
                            imageToLoad.SelectActiveFrame(frameDim, i);

                            var resizedBmp = new Bitmap(imageToLoad, (int)imageToLoad.Width / 8, (int)imageToLoad.Height / 8);

                            var rect = new System.Drawing.Rectangle(0, 0, resizedBmp.Width, resizedBmp.Height);
                            System.Drawing.Imaging.BitmapData bmpData =
                                resizedBmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                resizedBmp.PixelFormat);

                            // Get the address of the first line.
                            IntPtr ptr = bmpData.Scan0;

                            // Declare an array to hold the bytes of the bitmap.
                            int bytes = Math.Abs(bmpData.Stride) * resizedBmp.Height;
                            byte[] rgbValues = new byte[bytes];

                            // Copy the RGB values into the array.
                            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
                            var frame = new ByteFrame();
                            frame.Frame = rgbValues;
                            frame.FrameWidth = resizedBmp.Width;
                            frame.FrameHeight = resizedBmp.Height;


                            gifFrames[i] = frame;
                            resizedBmp.UnlockBits(bmpData);

                        }
                        imageToLoad.Dispose();
                        fs.Close();
                        GC.Collect();

                        Frames = gifFrames;

                    }

                }

            }
            catch (Exception)
            {
                Frames = null;

            }

        }
    }
}
