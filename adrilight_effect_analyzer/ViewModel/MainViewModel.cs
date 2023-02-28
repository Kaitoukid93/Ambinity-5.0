using adrilight_effect_analyzer.Model;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;


namespace adrilight_effect_analyzer.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            SetupCommand();
        }

        public ICommand SelectFrameDataFolderCommand { get; set; }

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
        }
        private Frame _currentFrame;
        public Frame CurrentFrame
        {
            get
            {
                return _currentFrame;
            }
            set
            {
                _currentFrame = value;
                RaisePropertyChanged(nameof(CurrentFrame));
            }
        }
        private Motion _layer;
        public Motion Layer
        {
            get { return _layer; }
            set
            {
                _layer = value;
                RaisePropertyChanged(nameof(Layer));
            }
        }

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


            Layer = new Motion(Import.FileNames.Length);
            for(int i=0;i<Import.FileNames.Length;i++)
            {
                BitmapData bitmapData = new BitmapData();
                Bitmap curentFrame = new Bitmap(Import.FileNames[i]);
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
                byte[] brightnessMap = new byte[rectSet.Length];
                int pixelCount = 0;
                CurrentFrame = new Frame(256);
                foreach (var rect in rectSet)
                {
                    const int numberOfSteps = 15;
                    int stepx = Math.Max(1, rect.Width / numberOfSteps);
                    int stepy = Math.Max(1, rect.Height / numberOfSteps);
                    GetAverageColorOfRectangularRegion(rect, stepy, stepx, bitmapData, out int sumR, out int sumG, out int sumB, out int count);
                    var countInverse = 1f / count;
                    brightnessMap[pixelCount++] = (byte)sumR;
                    System.Windows.Media.Color pixelColor = new System.Windows.Media.Color();
                    pixelColor = System.Windows.Media.Color.FromRgb((byte)(sumR * countInverse), (byte)(sumG * countInverse), (byte)(sumB * countInverse));
                    //add displaypixel to current frame
                    
                }
                var newFrame = new Frame(256);
                for (int j= 0;j < brightnessMap.Count(); j++)
                {
                    newFrame.BrightnessData[j] = brightnessMap[j];
                }
                Layer.Frames[i] = newFrame;

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

                    rectangleSet[index] = new Rectangle(x,y,rectWidth,rectHeight);
                    counter++;

                }
            }

            return rectangleSet;

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

            var layerJson = JsonConvert.SerializeObject(Layer, new JsonSerializerSettings()
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
