
using adrilight_effect_analyzer.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace adrilight_effect_analyzer.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            SetupCommand();
            WinApi.TimeBeginPeriod(1);
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1);
            FrameCounter = 0;
            dispatcherTimer.Start();

        }

        public ICommand SelectFrameDataFolderCommand { get; set; }
        public ICommand PlayCurrentMotionCommand { get; set; }
        private int FrameCounter = 0;
        public void SetupCommand()
        {
            
            SelectFrameDataFolderCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SelectFrameDataFolder();
            }

        );
            PlayCurrentMotionCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (dispatcherTimer.IsEnabled)
                    dispatcherTimer.Start();
            }
            );
        }
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private Motion _motion;
        public Motion Motion
        {
            get { return _motion; }
            set
            {
                _motion = value;
                RaisePropertyChanged(nameof(Motion));
            }
        }
        private WriteableBitmap PreviewBitmap;
        private void SelectFrameDataFolder()
        {
            System.Windows.Forms.OpenFileDialog Import = new System.Windows.Forms.OpenFileDialog();
            Import.Title = "Chọn Frame";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "Image files (*.png)|*.Png";
            Import.FilterIndex = 2;
            Import.Multiselect = true;

            Import.ShowDialog();


            Motion = new Motion();
            int frameCounter = 0;
            Motion.FrameData = new Frame[Import.FileNames.Length];
            foreach (var filename in Import.FileNames)
            {
                BitmapData bitmapData = new BitmapData();
                Bitmap curentFrame = new Bitmap(filename);
                var frameWidth = curentFrame.Width;
                var frameHeight = curentFrame.Height;

                var rectSet = BuildMatrix(frameWidth, frameHeight, 256, 1);
                //lock frame
                try
                {
                    curentFrame.LockBits(new Rectangle(0, 0, frameWidth, frameHeight), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb, bitmapData);
                }

                catch (System.ArgumentException)
                {
                    //usually the rectangle is jumping out of the image due to new profile, we recreate the rectangle based on the scale
                    // or simply dispose the image and let GetNextFrame handle the rectangle recreation
                    curentFrame = null;
                    // continue;
                }

                Motion.FrameData[frameCounter] = new Frame();
                int pixelCount = 0;
                Motion.FrameData[frameCounter].PixelData = new int[rectSet.Length];
                foreach (var rect in rectSet)
                {
                    const int numberOfSteps = 15;
                    int stepx = Math.Max(1, rect.Width / numberOfSteps);
                    int stepy = Math.Max(1, rect.Height / numberOfSteps);
                    GetAverageColorOfRectangularRegion(rect, stepy, stepx, bitmapData, out int sumR, out int sumG, out int sumB, out int count);
                    var countInverse = 1f / count;
                    // System.Windows.Media.Color pixelColor = new System.Windows.Media.Color();
                    // pixelColor = System.Windows.Media.Color.FromRgb((byte)(sumR * countInverse), (byte)(sumG * countInverse), (byte)(sumB * countInverse));
                    Motion.FrameData[frameCounter].PixelData[pixelCount++] = (byte)(sumR * countInverse);
                    Motion.FrameData[frameCounter].PixelCount++;


                }
                Motion.FrameCount++;
                frameCounter++;
                //display frame at preview
                //store current frame to json file
            }
            //splice bitmap into 256 led width and 1 led height
            // so the rectangle is width : 4 and height:4 with top is 0 and left is i*4
            ExportCurrentLayer();


        }
        private unsafe void GetAverageColorOfRectangularRegion(Rectangle spotRectangle, int stepy, int stepx, BitmapData bitmapData, out int sumR, out int sumG,
       out int sumB, out int count)
        {
            sumR = 0;
            sumG = 0;
            sumB = 0;
            count = 0;

            var stepCount = spotRectangle.Width / stepx;
            var stepxTimes4 = stepx * 4;
            for (var y = spotRectangle.Top; y < spotRectangle.Bottom; y += stepy)
            {
                byte* pointer = (byte*)bitmapData.Scan0 + bitmapData.Stride * y + 4 * spotRectangle.Left;
                for (int i = 0; i < stepCount; i++)
                {
                    sumB += pointer[0];
                    sumG += pointer[1];
                    sumR += pointer[2];

                    pointer += stepxTimes4;
                }
                count += stepCount;
            }
        }

        public WriteableBitmap _previewMotion;

        public WriteableBitmap PreviewMotion
        {
            get => _previewMotion;
            set
            {
                _previewMotion = value;
                RaisePropertyChanged(nameof(PreviewMotion));


            }
        }


        private Rectangle[] BuildMatrix(int rectwidth, int rectheight, int spotsX, int spotsY)
        {
            int spacing = 0;
            if (spotsX == 0)
                spotsX = 1;
            if (spotsY == 0)
                spotsY = 1;
            Rectangle[] rectangleSet = new Rectangle[spotsX * spotsY];
            var rectWidth = (rectwidth - (spacing * (spotsX + 1))) / spotsX;
            var rectHeight = (rectheight - (spacing * (spotsY + 1))) / spotsY;



            //var startPoint = (Math.Max(rectheight,rectwidth) - spotSize * Math.Min(spotsX, spotsY))/2;
            var counter = 0;




            for (var j = 0; j < spotsY; j++)
            {
                for (var i = 0; i < spotsX; i++)
                {
                    var x = spacing * i + (rectwidth - (spotsX * rectWidth) - spacing * (spotsX - 1)) / 2 + i * rectWidth;
                    var y = spacing * j + (rectheight - (spotsY * rectHeight) - spacing * (spotsY - 1)) / 2 + j * rectHeight;
                    var index = counter;

                    rectangleSet[index] = new Rectangle(x, y, rectWidth, rectHeight);
                    counter++;

                }
            }

            return rectangleSet;

        }
        private void RunMotion(Frame frame)
        {
            if (Motion != null)
            {
                if (PreviewBitmap == null)
                    PreviewBitmap = new WriteableBitmap(256, 1, 96, 96, PixelFormats.Bgra32, null);
                PreviewBitmap.Lock();
                IntPtr pixelAddress = PreviewBitmap.BackBuffer;
                var currentFrame = new byte[256 * 4];
                for (int i = 0; i < 256; i += 4)
                {
                    currentFrame[i] = 255;
                    currentFrame[i + 1] = 255;
                    currentFrame[i + 2] = 255;
                    currentFrame[i + 3] = (byte)frame.PixelData[i];
                }
                Marshal.Copy(currentFrame, 0, pixelAddress, 256 * 4);

                PreviewBitmap.AddDirtyRect(new Int32Rect(0, 0, 256, 1));
                PreviewMotion = PreviewBitmap;

                PreviewBitmap.Unlock();



            }

        }
        private static int MakeRgb(byte red, byte green, byte blue)
        {
            return ((red * 0x10000) + (green * 0x100) + blue);
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (Motion != null)
            {
                RunMotion(Motion.FrameData[FrameCounter++]);
                if (FrameCounter > Motion.FrameCount - 1)
                    FrameCounter = 0;

            }


        }
        private void ExportCurrentLayer()
        {
            Microsoft.Win32.SaveFileDialog Export = new Microsoft.Win32.SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;

            Export.Title = "Xuất dữ liệu";
            Export.FileName = "Layer";
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.DefaultExt = "AML";
            Export.Filter = "All files (*.*)|*.*";
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;

            var layerJson = JsonConvert.SerializeObject(Motion, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if (Export.ShowDialog() == true)
            {

                File.WriteAllText(Export.FileName, layerJson);

            }
        }

    }
}
