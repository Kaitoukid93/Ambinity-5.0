using adrilight_shared.Models.Device;
using adrilight_shared.Models.Drawable;
using adrilight_shared.Models.Stores;
using adrilight_shared.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight_content_creator.ViewModel
{
    public class OutputMappingViewModel
    {
        public OutputMappingViewModel(DeviceCanvas deviceCanvas)
        {
            Canvas = deviceCanvas;
            CommandSetup();
        }
        public DeviceCanvas Canvas { get; set; }
        public void Init(IDeviceSettings device)
        {
            Canvas.Items.Clear();
            foreach (var output in device.AvailableLightingOutputs)
            {
                (output as IDrawable).IsSelectable = true;
                (output as IDrawable).IsDraggable = true;
                Canvas.Items.Add(output as IDrawable);
            }
            var image = new ImageVisual();
            image.ImagePath = device.DeviceThumbnail;
            image.Left = 0;
            image.Top = 0;
            image.Width = 500;
            image.Height = 500;
            image.IsSelectable = false;
            Canvas.Items.Insert(0,image);
        }
        private void CommandSetup()
        {
            RichCanvasMouseButtonDownCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftShift)) // user is draging or holding ctrl
                {
                    Canvas.UnselectAllCanvasItem();
                }

            });
            SelectItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                Canvas.ChangeSelectedItem(p);
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    p.IsSelected = false;
                }

            });
        }
        public ICommand RichCanvasMouseButtonDownCommand { get; set; }
        public ICommand SelectItemCommand { get; set; }
        public ICommand ImportOutputMappingImageCommand { get; set; }
    }
}
