using adrilight_shared.Models.FrameData;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues
{
    public class Gif : ViewModelBase, IParameterValue
    {

        public Gif(string path)
        {
            LocalPath = path;
        }
        public Gif()
        {

        }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        private bool _isDeleteable = true;
        [JsonIgnore]
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsChecked, ref _isDeleteable, value); } }
        [JsonIgnore]
        public ByteFrame[] Frames { get; set; }
        private bool _isChecked = false;
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        public string LocalPath { get; set; }
        [JsonIgnore]
        public string InfoPath { get; set; }
        [JsonIgnore]
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
                    using (Image imageToLoad = Image.FromStream(fs))
                    {
                        var frameDim = new FrameDimension(imageToLoad.FrameDimensionsList[0]);
                        var frameCount = imageToLoad.GetFrameCount(frameDim);
                        var gifFrames = new ByteFrame[frameCount];
                        for (int i = 0; i < frameCount; i++)
                        {
                            imageToLoad.SelectActiveFrame(frameDim, i);

                            var resizedBmp = new Bitmap(imageToLoad, imageToLoad.Width / 8, imageToLoad.Height / 8);

                            var rect = new Rectangle(0, 0, resizedBmp.Width, resizedBmp.Height);
                            BitmapData bmpData =
                                resizedBmp.LockBits(rect, ImageLockMode.ReadWrite,
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
