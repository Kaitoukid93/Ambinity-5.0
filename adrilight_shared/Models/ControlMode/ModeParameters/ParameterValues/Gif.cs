﻿using adrilight_shared.Models.FrameData;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
        private GifBitmapDecoder Decoder { get; set; }
        /// <summary>
        /// this function play gif single time
        /// </summary>
        public async Task PlayGif()
        {
            if (!File.Exists(LocalPath))
                return;
            if (Decoder == null)
            {
                InitGif();
            }

            for (int i = 0; i < Decoder.Frames.Count; i++)
            {
                Decoder.Frames[i].Freeze();
                Bitmap = Decoder.Frames[i];
                await (Task.Delay(TimeSpan.FromMilliseconds(30)));
            }

        }
        /// <summary>
        /// this function return original first gif image
        /// </summary>
        public void DisposeGif()
        {
            // Decoder
        }
        /// <summary>
        /// this function load static image to display
        /// </summary>
        public void InitGif()
        {

            Stream imageStreamSource = new FileStream(LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            Decoder = new GifBitmapDecoder(imageStreamSource, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);
            Bitmap = Decoder.Frames[0];

        }
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
