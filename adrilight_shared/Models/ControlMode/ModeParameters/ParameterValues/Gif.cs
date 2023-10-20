using adrilight_shared.Models.FrameData;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
        private BitmapSource _bitmap;
        private bool _isVisible = true;
        [JsonIgnore]
        public bool IsVisible { get => _isVisible; set { Set(() => IsVisible, ref _isVisible, value); } }
        [JsonIgnore]
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsChecked, ref _isDeleteable, value); } }
        [JsonIgnore]
        public ByteFrame[] Frames { get; set; }
        private bool _isChecked = false;
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        public string LocalPath { get; set; }
        [JsonIgnore]
        public BitmapSource Bitmap { get => _bitmap; set { Set(() => Bitmap, ref _bitmap, value); } }
        [JsonIgnore]
        public string InfoPath { get; set; }
        [JsonIgnore]
        public object Lock { get; } = new object();
        private GifBitmapDecoder Decoder;
        private CancellationTokenSource cancellationTokenSource;
        private bool _isRunning;
        /// <summary>
        /// this function play gif single time
        /// </summary>
        public async Task PlayGif()
        {

            if (_isRunning)
                return;
            if (!File.Exists(LocalPath))
                return;
            Stream imageStreamSource = new FileStream(LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            Decoder = new GifBitmapDecoder(imageStreamSource, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad);
            Bitmap = Decoder.Frames[0];
            Bitmap.Freeze();
            cancellationTokenSource = new CancellationTokenSource(5000);
            try
            {
                await RunGif(cancellationTokenSource.Token, Decoder);
            }
            catch (TaskCanceledException ex)
            {
                // Console.WriteLine($"{ex.Message}");
            }


        }
        private async Task RunGif(CancellationToken token, GifBitmapDecoder decoder)
        {
            _isRunning = true;
            int frameCounter = 0;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    decoder.Frames[frameCounter].Freeze();
                    Bitmap = decoder.Frames[frameCounter];
                    await (Task.Delay(TimeSpan.FromMilliseconds(30)));
                    if (frameCounter < decoder.Frames.Count)
                        frameCounter++;
                    if (frameCounter == decoder.Frames.Count)
                        frameCounter = 0;
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                Bitmap = null;
                decoder = null;
                GC.Collect();

            }
        }
        /// <summary>
        /// this function return original first gif image
        /// </summary>
        public void DisposeGif()
        {
            // Decoder
            _isRunning = false;
            cancellationTokenSource.Cancel();
            cancellationTokenSource = null;
            Decoder = null;
            GC.Collect();
        }
        /// <summary>
        /// this function load static image to display
        /// </summary>
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

                            var rect = new System.Drawing.Rectangle(0, 0, resizedBmp.Width, resizedBmp.Height);
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
