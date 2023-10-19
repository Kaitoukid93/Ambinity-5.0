using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Drawable;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight_shared.Models.Device.Group
{
    public class ControlZoneGroup : ViewModelBase
    {
        public ControlZoneGroup()
        {

        }
        public ControlZoneGroup(string name)
        {
            Name = name;
            ControlZones = new ObservableCollection<IControlZone>();
        }
        private Border _border;
        public string Name { get; set; }
        public Border Border { get => _border; set { Set(() => Border, ref _border, value); } }
        public ControllerTypeEnum Type { get; set; }
        public string GroupUID { get; set; }
        public IControlZone MaskedControlZone { get; set; }
        public ISlaveDevice MaskedSlaveDevice { get; set; }
        [JsonIgnore]
        public ObservableCollection<IControlZone> ControlZones { get; set; }
        public async Task AddZonesToGroup(List<IDrawable> items)
        {
            //more than 1 items get selected
            if (items == null || items.Count <= 0)
                return;
            GroupUID = Guid.NewGuid().ToString();

            if (items.Any(i => i is FanMotor))
            {
                items.ForEach(i => ControlZones.Add(i as FanMotor));
                Type = ControllerTypeEnum.PWMController;
                MaskedControlZone = ObjectHelpers.Clone<FanMotor>(items.First() as FanMotor);
                MaskedControlZone.Name = Name + " - " + "MultiFan";
                MaskedControlZone.Description = "Masked Control for multiple fans selected";
                MaskedSlaveDevice = new PWMMotorSlaveDevice()
                {
                    Name = "Union Device",
                    Description = "Thiết bị đại diện cho " + Name,
                    Owner = "System",
                    //Thumbnail = Path.Combine(ResourceFolderPath, "Group_thumb.png")
                };
            }
            else if (items.Any(i => i is LEDSetup))
            {
                items.ForEach(i => ControlZones.Add(i as LEDSetup));
                Type = ControllerTypeEnum.LightingController;
                MaskedControlZone = ObjectHelpers.Clone<LEDSetup>(items.First() as LEDSetup);
                MaskedControlZone.Name = Name + " - " + "MultiZone";
                MaskedControlZone.Description = "Masked Control for multizone selected";
                MaskedSlaveDevice = new ARGBLEDSlaveDevice()
                {
                    Name = "Union Device",
                    Description = "Thiết bị đại diện cho " + Name,
                    Owner = "System",
                };
            }

            GetGroupBorder();
            await RegisterGroupItem();
        }
        public async Task RegisterGroupItem()
        {
            //get items

            if (ControlZones.Count <= 0)
                return;
            foreach (var item in ControlZones)
            {
                if (item is LEDSetup)
                {
                    var ledZone = item as LEDSetup;
                    ledZone.IsSelected = false;
                    ledZone.IsSelectable = false;
                    ledZone.Group = this;
                }
                else if (item is FanMotor)
                {
                    var fanZone = item as FanMotor;
                    fanZone.IsSelected = false;
                    fanZone.IsSelectable = false;
                    fanZone.Group = this;
                }
                item.IsInControlGroup = true;
                item.GroupID = GroupUID;
                await Task.Run(() => { item.CurrentActiveControlMode = MaskedControlZone.CurrentActiveControlMode; });
            }
        }

        public void GetGroupBorder()
        {
            var drawableHelpers = new DrawableHelpers();

            if (ControlZones.Count == 0)
            {
                Border = null;
                return;
            }

            var left = drawableHelpers.GetRealBound(ControlZones.ToArray()).Left;
            var top = drawableHelpers.GetRealBound(ControlZones.ToArray()).Top;
            var width = drawableHelpers.GetRealBound(ControlZones.ToArray()).Width;
            var height = drawableHelpers.GetRealBound(ControlZones.ToArray()).Height;
            var border = new Border()
            {
                Name = Name,
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                IsSelectable = true,
            };
            if (Border != null)
            {
                Border.Name = border.Name;
                Border.Left = border.Left;
                Border.Top = border.Top;
                Border.Width = border.Width;
                Border.Height = border.Height;
            }
            else
            {
                Border = border;
            }
            //select current border
            Border.IsSelected = true;
            foreach (var zone in ControlZones)
            {
                if (zone is LEDSetup)
                {
                    var ledZone = zone as LEDSetup;
                    ledZone.GroupRect = new Rect(Border.Left, Border.Top, Border.Width, Border.Height);
                }
            }
          (MaskedControlZone as IDrawable).Left = Border.Left;
            (MaskedControlZone as IDrawable).Top = Border.Top;
            (MaskedControlZone as IDrawable).Width = Border.Width;
            (MaskedControlZone as IDrawable).Height = Border.Height;
        }
        //get group Items from device
        public void Init(IDeviceSettings device)
        {
            ControlZones = new ObservableCollection<IControlZone>();
            foreach (var zone in device.AvailableControlZones)
            {
                if (zone.GroupID == GroupUID)
                {
                    ControlZones.Add(zone);
                }
            }
        }
        public int GenerateVID(IParameterValue value, int intensity, IControlZone zone)
        {
            var vid = value as VIDDataModel;
            if (vid.ExecutionType == VIDType.PredefinedID)
                return 0;
            Rect vidSpace = new Rect();
            //get brush size
            //brush rect will move as the dirrection, so intersect size increase
            double vidSpaceWidth;
            double vidSpaceHeight;
            double zoneOffSetLeft;
            double zoneOffSetTop;
            int VIDCount = 0;
            var ledZone = zone as LEDSetup;
            ledZone.ResetVIDStage();
            switch (vid.Dirrection)
            {
                case VIDDirrection.left2right:
                    vidSpaceWidth = vidSpace.Width;
                    vidSpaceHeight = vidSpace.Height;
                    zoneOffSetLeft = ledZone.GetRect.Left - vidSpace.Left;
                    zoneOffSetTop = ledZone.GetRect.Top - vidSpace.Top;
                    VIDCount = (int)zoneOffSetLeft;
                    for (int x = 0; x < vidSpaceWidth; x += 5)
                    {
                        int settedVIDCount = 0;
                        var brush = new Rect(0 - zoneOffSetLeft, 0 - zoneOffSetTop, x, vidSpaceHeight);
                        foreach (var spot in ledZone.Spots)
                        {
                            if (spot.GetVIDIfNeeded(VIDCount, brush, 0))
                                settedVIDCount++;
                        }
                        if (settedVIDCount > 0)
                        {
                            VIDCount += intensity;
                        }
                        if (VIDCount > 1023)
                            VIDCount = 0;
                    }
                    break;
                case VIDDirrection.right2left:
                    vidSpaceWidth = (int)vidSpace.Width;
                    vidSpaceHeight = (int)vidSpace.Height;
                    zoneOffSetLeft = ledZone.GetRect.Left - vidSpace.Left;
                    zoneOffSetTop = ledZone.GetRect.Top - vidSpace.Top;
                    VIDCount = (int)(vidSpaceWidth - zoneOffSetLeft);
                    for (int x = (int)vidSpaceWidth; x > 0; x -= 5)
                    {
                        int settedVIDCount = 0;
                        var brush = new Rect(x - zoneOffSetLeft, 0 - zoneOffSetTop, vidSpaceWidth - x, vidSpaceHeight);
                        foreach (var spot in ledZone.Spots)
                        {
                            if (spot.GetVIDIfNeeded(VIDCount, brush, 0))
                                settedVIDCount++;
                        }
                        if (settedVIDCount > 0)
                        {
                            VIDCount += intensity;
                        }
                        if (VIDCount > 1023)
                            VIDCount = 0;
                    }
                    break;
                case VIDDirrection.bot2top:
                    vidSpaceWidth = vidSpace.Width;
                    vidSpaceHeight = vidSpace.Height;
                    zoneOffSetLeft = ledZone.GetRect.Left - vidSpace.Left;
                    zoneOffSetTop = ledZone.GetRect.Top - vidSpace.Top;
                    VIDCount = (int)(vidSpaceHeight - zoneOffSetTop);
                    for (int y = (int)vidSpaceHeight; y > 0; y -= 5)
                    {
                        int settedVIDCount = 0;
                        var brush = new Rect(0 - zoneOffSetLeft, y - zoneOffSetTop, vidSpaceWidth, vidSpaceHeight - y);
                        foreach (var spot in ledZone.Spots)
                        {
                            if (spot.GetVIDIfNeeded(VIDCount, brush, 0))
                                settedVIDCount++;
                        }
                        if (settedVIDCount > 0)
                        {
                            VIDCount += intensity;
                        }
                        if (VIDCount > 1023)
                            VIDCount = 0;
                    }
                    break;
                case VIDDirrection.top2bot:
                    vidSpaceWidth = vidSpace.Width;
                    vidSpaceHeight = vidSpace.Height;
                    zoneOffSetLeft = ledZone.GetRect.Left - vidSpace.Left;
                    zoneOffSetTop = ledZone.GetRect.Top - vidSpace.Top;
                    VIDCount = (int)zoneOffSetTop;
                    for (int y = 0; y < (int)vidSpaceHeight; y += 5)
                    {
                        int settedVIDCount = 0;
                        var brush = new Rect(0 - zoneOffSetLeft, 0 - zoneOffSetTop, vidSpaceWidth, y);
                        foreach (var spot in ledZone.Spots)
                        {
                            if (spot.GetVIDIfNeeded(VIDCount, brush, 0))
                                settedVIDCount++;
                        }
                        if (settedVIDCount > 0)
                        {
                            VIDCount += intensity;
                        }
                        if (VIDCount > 1023)
                            VIDCount = 0;
                    }
                    break;
                case VIDDirrection.linear:
                    foreach (var spot in ledZone.Spots)
                    {
                        spot.SetVID(spot.Index * intensity);
                        spot.HasVID = true;
                    }
                    break;


            }
            return VIDCount;
        }
    }
}
