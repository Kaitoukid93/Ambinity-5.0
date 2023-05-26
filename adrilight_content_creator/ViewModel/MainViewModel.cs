using adrilight;
using adrilight.Helpers;
using adrilight.Settings;
using adrilight.Spots;
using adrilight.View;
using adrilight.ViewModel;
using Castle.Core.Resource;
using HandyControl.Tools.Extension;
using Microsoft.Win32;
using Newtonsoft.Json;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml;
using static adrilight.ViewModel.MainViewViewModel;
using static System.Windows.Forms.AxHost;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;


namespace adrilight_content_creator.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        public MainViewModel(IList<ISelectableViewPart> selectableViewParts)
        {
            SetupCommand();
            if (selectableViewParts == null)
            {
                throw new ArgumentNullException(nameof(selectableViewParts));
            }
            SelectableViewParts = selectableViewParts.OrderBy(p => p.Order)
                .ToList();
            CanvasItems = new ObservableCollection<IDrawable>();
            CanvasSelectedItems = new ObservableCollection<IDrawable>();
            CanvasSelectedItems.CollectionChanged += SelectedItemsChanged;
            AvailableDeviceTypes = new ObservableCollection<SlaveDeviceTypeEnum>();
            foreach (SlaveDeviceTypeEnum type in Enum.GetValues(typeof(SlaveDeviceTypeEnum)))
            {
                AvailableDeviceTypes.Add(type);
            }
        }

        public ICommand OpenAddNewDrawableItemCommand { get; set; }
        public ICommand SaveDeviceDataCommand { get; set; }
        public ICommand ApplyDeviceActualDimensionCommand { get; set; }
        public ICommand AddImageToPIDCanvasCommand { get; set; }
        public ICommand AddItemsToPIDCanvasCommand { get; set; }
        public ICommand AddSpotLayoutCommand { get; set; }
        public ICommand AddSpotGeometryCommand { get; set; }
        public ICommand ImportSVGCommand { get; set; }
        public ICommand ClearPIDCanvasCommand { get; set; }
        public ICommand DeleteSelectedItemsCommand { get; set; }
        public ICommand CombineSelectedSpotsCommand { get; set; }
        public ICommand AddNewZoneCommand { get; set; }
        public ICommand SelectPIDCanvasItemCommand { get; set; }
        private bool _canSelectMultipleItems;
        public bool CanSelectMultipleItems
        {
            get { return _canSelectMultipleItems; }
            set
            {
                _canSelectMultipleItems = value;
            }
        }
        private void SelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                CanvasSelectedItem = CanvasSelectedItems.Count == 1 ? CanvasSelectedItems[0] : null;
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (CanvasSelectedItems.Count == 0 || CanvasSelectedItems.Count > 1)
                {
                    CanvasSelectedItem = null;
                }
            }

        }
        public void SetupCommand()
        {
            OpenAddNewDrawableItemCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                OpenAddNewItemWindow();

            }
          );
            AddSpotGeometryCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                AddSpotGeometry();

            }
              );
            ApplyDeviceActualDimensionCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                ApplyDeviceActualDimension();

            }
           );
            SaveDeviceDataCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                SaveDeviceData();

            }
           );
            ImportSVGCommand = new RelayCommand<string>((p) =>
                {
                    return true;
                }, (p) =>
                {

                    ImportSVG();

                }
              );

            SelectPIDCanvasItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (p is LEDSetup)
                {
                    var zone = p as LEDSetup;
                    foreach (var spot in zone.Spots)
                    {
                        spot.SetColor(0, 0, 255, true);
                    }
                }
            });
            AddImageToPIDCanvasCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                AddImage();

            }
           );
            ClearPIDCanvasCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                ClearPIDCanvas();

            }
          );
            AddSpotLayoutCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                AddSpotLayout();

            }
           );
            AddNewZoneCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AddNewZone();
            });
            CombineSelectedSpotsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CombineSelectedSpots();
            });
            DeleteSelectedItemsCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {

                DeleteSelectedItems(p);

            }
            );
            AddItemsToPIDCanvasCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                AddItemsToPIDCanvas();

            }
         );
        }
        private double _selectionRectangleStrokeThickness = 2.0;
        public double SelectionRectangleStrokeThickness
        {
            get { return _selectionRectangleStrokeThickness; }
            set
            {
                _selectionRectangleStrokeThickness = value;
                RaisePropertyChanged();
            }
        }
        private Thickness _canvasItemBorder = new Thickness(2.0);
        public Thickness CanvasItemBorder
        {
            get { return _canvasItemBorder; }
            set
            {
                _canvasItemBorder = value;
                RaisePropertyChanged();
            }
        }
        private double _canvasScale = 1.0;
        public double CanvasScale
        {
            get { return _canvasScale; }
            set
            {
                _canvasScale = value;
                CalculateItemsBorderThickness(value);
                RaisePropertyChanged();
            }
        }
        private void CalculateItemsBorderThickness(double scaleValue) // this keeps items border remain the same 2px anytime
        {
            SelectionRectangleStrokeThickness = 2 / scaleValue;
            CanvasItemBorder = new Thickness(SelectionRectangleStrokeThickness);
        }
        public IList<ISelectableViewPart> SelectableViewParts { get; }
        public ISelectableViewPart _selectedViewPart;
        public ISelectableViewPart SelectedViewPart
        {
            get => _selectedViewPart;
            set
            {
                Set(ref _selectedViewPart, value);

            }
        }

        #region Layout Creator Viewmodel region
        public DrawableShape SelectedShape { get; set; }
        public double NewItemWidth { get; set; }
        public double NewItemHeight { get; set; }
        public int ItemNumber { get; set; }
        private double _itemsScale;

        private ObservableCollection<DrawableShape> _availableShapeToAdd;
        public ObservableCollection<DrawableShape> AvailableShapeToAdd
        {
            get { return _availableShapeToAdd; }
            set
            {
                _availableShapeToAdd = value;
                RaisePropertyChanged();
            }
        }
        private adrilight_content_creator.View.NewItemParametersWindow NewItemParamsWindow { get; set; }
        public void UpdateZoneSize(bool withPoint, LEDSetup zone)
        {
            //get all child and set size
            var boundRct = GetDeviceRectBound(zone.Spots.ToList());
            zone.Width = boundRct.Width;
            zone.Height = boundRct.Height;
            if (withPoint)
            {
                zone.Left = boundRct.Left;
                zone.Top = boundRct.Top;
            }


        }
        private DrawableHelpers DrawableHlprs;
        private ControlModeHelpers CtrlHlprs;
        public Rect GetDeviceRectBound(List<IDrawable> spots)
        {


            if (DrawableHlprs == null)
                DrawableHlprs = new DrawableHelpers();
            var listDrawable = new List<IDrawable>();
            foreach (var spot in spots)
            {
                listDrawable.Add(spot as IDrawable);
            }

            return DrawableHlprs.GetBound(listDrawable);


        }
        public Rect GetDeviceRectBound(List<IDeviceSpot> spots)
        {


            if (DrawableHlprs == null)
                DrawableHlprs = new DrawableHelpers();
            var listDrawable = new List<IDrawable>();
            foreach (var spot in spots)
            {
                listDrawable.Add(spot as IDrawable);
            }

            return DrawableHlprs.GetBound(listDrawable);


        }
        private void CombineSelectedSpots()
        {

            var spotList = CanvasItems.Where(s => s is DeviceSpot && s.IsSelected).ToList();
            if (spotList.Count < 2)
                return;

            var bound = GetDeviceRectBound(spotList);
            var geometryList = new List<Geometry>();
            spotList.ForEach(p => geometryList.Add((p as DeviceSpot).Geometry));
            var stringValue = "";
            foreach (var geometry in geometryList)
            {
                var scaled = Geometry.Combine(
                    geometry,
                    geometry, GeometryCombineMode.Intersect,
                    new TransformGroup
                    {
                        Children = new TransformCollection
                        {
                            new TranslateTransform(bound.X * -1, bound.Y * -1),
                            new ScaleTransform(1.0 , 1.0 )
                        }
                    }
                );

                var scaledString = scaled.ToString(CultureInfo.InvariantCulture)
                    .Replace("F1", "")
                    .Replace(";", ",")
                    .Replace("L", " L")
                    .Replace("C", " C");

                stringValue = stringValue + " " + scaledString;
            }
            var shape = Geometry.Parse(stringValue.Trim());
            var lastSpotID = CanvasItems.Where(s => s is DeviceSpot).Count();
            var newItem = new DeviceSpot(
                      bound.Top,
                      bound.Left,
                      shape.Bounds.Width,
                      shape.Bounds.Height,
                      1,
                      1,
                      1,
                      1,
                      lastSpotID,
                      lastSpotID,
                      lastSpotID,
                      lastSpotID,
                      lastSpotID,
                      false,
                      shape);
            newItem.IsSelected = false;
            newItem.IsDeleteable = true;
            CanvasSelectedItems.Clear();
            CanvasItems.Add(newItem);
            spotList.ForEach(s => CanvasItems.Remove(s));
        }
        private void AddNewZone() // create new zone from selected spot fomr wellknown itemsource
        {

            var newZone = new LEDSetup();
            var spotList = CanvasItems.Where(s => s is DeviceSpot && s.IsSelected).ToList();
            if (spotList.Count < 1)
                return;
            foreach (var spot in spotList)
            {
                var clonedSpot = ObjectHelpers.Clone<DeviceSpot>(spot as DeviceSpot);
                newZone.Spots.Add(clonedSpot);

            }
            UpdateZoneSize(true, newZone);

            foreach (var spot in newZone.Spots)
            {

                (spot as DeviceSpot).Left -= newZone.Left;
                (spot as DeviceSpot).Top -= newZone.Top;
                (spot as DeviceSpot).IsSelected = false;

            }
            CanvasSelectedItems.Clear();
            CanvasItems.Add(newZone);
            spotList.ForEach(spot => CanvasItems.Remove(spot));

        }
        private void OpenAddNewItemWindow()
        {
            AvailableShapeToAdd = new ObservableCollection<DrawableShape>();
            var circleShape = new DrawableShape()
            {
                Geometry = Geometry.Parse("M100 50C100 77.6142 77.6142 100 50 100C22.3858 100 0 77.6142 0 50C0 22.3858 22.3858 0 50 0C77.6142 0 100 22.3858 100 50Z"),
                Name = "Round"
            };
            var squareShape = new DrawableShape()
            {
                Geometry = Geometry.Parse("M0 0H100V100H0V0Z"),
                Name = "Square"
            };
            AvailableShapeToAdd.Add(squareShape);
            AvailableShapeToAdd.Add(circleShape);
            NewItemParamsWindow = new adrilight_content_creator.View.NewItemParametersWindow();
            NewItemParamsWindow.Owner = System.Windows.Application.Current.MainWindow;
            NewItemParamsWindow.ShowDialog();

        }
        private double _deviceActualWidth;
        private double _deviceActualHeight;
        public double DeviceActualWidth
        {
            get { return _deviceActualWidth; }
            set
            {
                _deviceActualWidth = value;
                RaisePropertyChanged();
            }
        }
        public double DeviceActualHeight
        {
            get { return _deviceActualHeight; }
            set
            {
                _deviceActualHeight = value;
                RaisePropertyChanged();
            }
        }
        public string Name { get; set; }
        public string Description { get; set; } 
        public string Vendor { get; set; }
        private void SaveDeviceData()
        {
            if (DrawableHlprs == null)
                DrawableHlprs = new DrawableHelpers();
            if (CtrlHlprs == null)
                CtrlHlprs = new ControlModeHelpers();

            var usableZone = CanvasItems.Where(z => z is LEDSetup).ToList();
            if (usableZone.Count() == 0)
            {
                HandyControl.Controls.MessageBox.Show("Bạn phải thêm ít nhất 1 Zone", "Invalid LED number", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var rectBound = GetDeviceRectBound(CanvasItems.ToList());
            var image = CanvasItems.Where(i => i is ImageVisual).FirstOrDefault();
            System.Windows.Point scale = new System.Windows.Point(DeviceActualWidth / rectBound.Width, DeviceActualHeight / rectBound.Height);
            foreach (var zone in usableZone)
            {
                //reset origin for each zone
                var ledSetup = zone as LEDSetup;
                ledSetup.Left -= rectBound.Left;
                ledSetup.Top -= rectBound.Top;
                ledSetup.SetScale(scale.X, scale.Y, false);
                ledSetup.ZoneUID = Guid.NewGuid().ToString();
                //make this zone controlable
                CtrlHlprs.MakeZoneControlable(ledSetup);

            }
             Device = new ARGBLEDSlaveDevice();
            Device.ActualWidth = DeviceActualWidth;
            Device.ActualHeight = DeviceActualHeight;
            Device.Width = DeviceActualWidth;
            Device.Height = DeviceActualHeight;
            Device.Name = Name;
            Device.Description = Description;
            Device.Vendor = Vendor;
            Device.DeviceType = DeviceType;

            if (image != null)
            {
                image.Left -= rectBound.Left;
                image.Top -= rectBound.Top;
                image.SetScale(scale.X, scale.Y, false);
                Device.Image = image as ImageVisual;

            }
            usableZone.ForEach(zone => Device.ControlableZones.Add(zone as LEDSetup));
            CanvasSelectedItems.Clear();
            usableZone.ForEach(zone => CanvasItems.Remove(zone));
            CanvasItems.Remove(image);
            CanvasItems.Add(Device);
            CanSelectMultipleItems = false;
            ExportCurrentDeviceToFile();
        }
        private void ExportCurrentDeviceToFile()
        {
            Microsoft.Win32.SaveFileDialog Export = new Microsoft.Win32.SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;

            Export.Title = "Xuất dữ liệu";
            Export.FileName = "Layer";
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.DefaultExt = "json";
            Export.Filter = "All files (*.*)|*.*";
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;

            var layerJson = JsonConvert.SerializeObject(Device, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if (Export.ShowDialog() == true)
            {

                File.WriteAllText(Export.FileName, layerJson);

            }
        }

        private void ApplyDeviceActualDimension()
        {


            if (DrawableHlprs == null)
                DrawableHlprs = new DrawableHelpers();
            if (CtrlHlprs == null)
                CtrlHlprs = new ControlModeHelpers();
            var rectBound = GetDeviceRectBound(CanvasItems.ToList());
            System.Windows.Point scale = new System.Windows.Point(DeviceActualWidth / rectBound.Width, DeviceActualHeight / rectBound.Height);
            foreach (var item in CanvasItems)
            {
                item.Left -= rectBound.Left;
                item.Top -= rectBound.Top;
                item.SetScale(scale.X, scale.Y, false);
            }
        }
        public SlaveDeviceTypeEnum DeviceType { get; set; }
        public ObservableCollection<SlaveDeviceTypeEnum> AvailableDeviceTypes { get; set; }
        public ARGBLEDSlaveDevice Device { get; set; }
        private IDrawable _canvasSelectedItem;
        public IDrawable CanvasSelectedItem
        {
            get { return _canvasSelectedItem; }
            set { _canvasSelectedItem = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<IDrawable> _canvasSelectedItems;
        public ObservableCollection<IDrawable> CanvasSelectedItems
        {
            get { return _canvasSelectedItems; }
            set { _canvasSelectedItems = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<IDrawable> _canvasItems;
        public ObservableCollection<IDrawable> CanvasItems
        {
            get { return _canvasItems; }
            set { _canvasItems = value; RaisePropertyChanged(); }
        }
        private System.Windows.Point _mousePosition;
        public System.Windows.Point MousePosition
        {
            get { return _mousePosition; }
            set
            {
                _mousePosition = value; RaisePropertyChanged();
            }
        }
        private void AddImage()
        {
            OpenFileDialog addImage = new OpenFileDialog();
            addImage.Title = "Chọn Ảnh";
            addImage.CheckFileExists = true;
            addImage.CheckPathExists = true;
            addImage.DefaultExt = "Png";
            addImage.Filter = "Image files (*.png;*.jpeg;*jpg)|*.png;*.jpeg;*jpg|All files (*.*)|*.*";
            addImage.FilterIndex = 2;

            addImage.ShowDialog();

            if (!string.IsNullOrEmpty(addImage.FileName) && File.Exists(addImage.FileName))
            {
                Bitmap bitmap = (Bitmap)Bitmap.FromFile(addImage.FileName, true);
                var image = new ImageVisual
                {
                    Top = MousePosition.Y,
                    Left = MousePosition.X,
                    ImagePath = addImage.FileName,
                    Width = DeviceActualWidth,
                    Height = DeviceActualHeight,
                    IsResizeable = true,
                    IsDeleteable = true
                };
                CanvasItems.Insert(0, image);
            }
        }

        private void DeleteSelectedItems(ObservableCollection<IDrawable> itemSource)
        {
            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).ToArray();
            for (var i = 0; i < selectedItems.Count(); i++)
            {
                selectedItems[i].IsSelected = false;
                if (selectedItems[i].IsDeleteable)
                    itemSource.Remove(selectedItems[i]);
            }
        }
        private void AddItemsToPIDCanvas()
        {
            var lastSpotID = CanvasItems.Where(s => s is DeviceSpot).Count();
            var newItems = new ObservableCollection<IDrawable>();
            if (ItemNumber == 0)
            {
                ItemNumber = 1;
            }
            for (int i = 0; i < ItemNumber; i++)
            {
                var newItem = new DeviceSpot(
                    MousePosition.Y,
                    MousePosition.X,
                    NewItemWidth > 0 ? NewItemWidth : 20.0,
                    NewItemHeight > 0 ? NewItemHeight : 20.0,
                    1,
                    1,
                    1,
                    1,
                    lastSpotID + i,
                    lastSpotID + i,
                    lastSpotID + i,
                    lastSpotID + i,
                    lastSpotID + i,
                    false,
                    SelectedShape.Geometry);
                newItem.IsSelected = true;
                newItem.IsDeleteable = true;
                newItems.Add(newItem);

            }
            SpreadItemHorizontal(newItems, 0);
            foreach (var item in newItems)
            {
                CanvasItems.Add(item);
                //CurrentOutput.OutputLEDSetup.Spots.Add(item as DeviceSpot);
            }

        }
        private void AglignSelectedItemstoLeft(ObservableCollection<IDrawable> itemSource)
        {
            double minLeft = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).Min(x => x.Left);
            foreach (var item in itemSource.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.Left = minLeft;
            }
        }
        private void SpreadItemHorizontal(ObservableCollection<IDrawable> itemSource, int dirrection)
        {
            //get min X
            double spacing = 10.0;
            double minLeft = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).Min(x => x.Left);
            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).ToArray();
            switch (dirrection)
            {
                case 0:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            selectedItems[i].Left = minLeft;
                        else
                        {
                            var previousLeft = selectedItems[i - 1].Left;
                            selectedItems[i].Left = selectedItems[i - 1].Width + previousLeft + spacing;
                        }

                    }
                    AglignSelectedItemstoTop(itemSource);
                    break;
                case 1:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            selectedItems[i].Left = minLeft;
                        else
                        {
                            var previousLeft = selectedItems[i - 1].Left;
                            selectedItems[i].Left = previousLeft - (selectedItems[i].Width + spacing);
                        }

                    }
                    AglignSelectedItemstoTop(itemSource);
                    break;

            }
        }
        private void SpreadItemVertical(ObservableCollection<IDrawable> itemSource, int dirrection)
        {
            //get min Y
            double spacing = 10.0;
            double minTop = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).Min(x => x.Top);
            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).ToArray();
            switch (dirrection)
            {
                case 0:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            selectedItems[i].Top = minTop;
                        else
                        {
                            var previousTop = selectedItems[i - 1].Top;
                            selectedItems[i].Top = selectedItems[i - 1].Height + previousTop + spacing;
                        }

                    }
                    AglignSelectedItemstoLeft(itemSource);
                    break;
                case 1:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            selectedItems[i].Top = minTop;
                        else
                        {
                            var previousTop = selectedItems[i - 1].Top;
                            selectedItems[i].Top = previousTop - (selectedItems[i].Height + spacing);
                        }

                    }
                    AglignSelectedItemstoLeft(itemSource);
                    break;

            }

        }
        private void AglignSelectedItemstoTop(ObservableCollection<IDrawable> itemSource)
        {
            double minTop = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).Min(x => x.Top);
            foreach (var item in itemSource.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.Top = minTop;
            }
        }
        private void ClearPIDCanvas()
        {

            CanvasSelectedItems.Clear();
            CanvasItems.Clear();

        }
        private void LockSelectedItem(ObservableCollection<IDrawable> itemSource)
        {

            foreach (var item in itemSource.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.IsDraggable = false;
            }
        }
        private void UnlockSelectedItem(ObservableCollection<IDrawable> items)
        {

            foreach (var item in items.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.IsDraggable = true;
                item.IsSelectable = true;

            }
        }
        private void AddSpotLayout()
        {
            System.Windows.Forms.OpenFileDialog Import = new System.Windows.Forms.OpenFileDialog();
            Import.Title = "Chọn GeometryGroup files";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "Text files (*.txt)|*.TXT";
            Import.FilterIndex = 2;
            Import.Multiselect = false;

            Import.ShowDialog();

            var text = System.IO.File.ReadAllText(Import.FileName);
            try
            {
                StringReader sr = new StringReader(text);

                XmlReader reader = XmlReader.Create(sr);

                GeometryGroup gr = (GeometryGroup)XamlReader.Load(reader);


                var lastSpotID = CanvasItems.Where(s => s is DeviceSpot).Count();
                var newItems = new ObservableCollection<IDrawable>();
                for (int i = 0; i < gr.Children.Count; i++)
                {
                    var newItem = new DeviceSpot(
                        gr.Children[i].Bounds.Top,
                        gr.Children[i].Bounds.Left,
                        gr.Children[i].Bounds.Width,
                        gr.Children[i].Bounds.Height,
                        1,
                        1,
                        1,
                        1,
                        lastSpotID + i,
                        lastSpotID + i,
                        lastSpotID + i,
                        lastSpotID + i,
                        lastSpotID + i,
                        false,
                        gr.Children[i]);
                    newItem.IsSelected = true;
                    newItem.IsDeleteable = true;
                    newItems.Add(newItem);

                }
                foreach (var item in newItems)
                {
                    CanvasItems.Add(item);
                    //CurrentOutput.OutputLEDSetup.Spots.Add(item as DeviceSpot);
                }
            }
            catch (Exception ex)
            {

            }

        }
        public List<GeometryDrawing> GatherGeometry(DrawingGroup drawingGroup)
        {
            var result = new List<GeometryDrawing>();
            result.AddRange(drawingGroup.Children.Where(c => c is GeometryDrawing).Cast<GeometryDrawing>());
            foreach (var childGroup in drawingGroup.Children.Where(c => c is DrawingGroup).Cast<DrawingGroup>())
                result.AddRange(GatherGeometry(childGroup));

            return result;
        }
        private void ImportSVG()
        {
            System.Windows.Forms.OpenFileDialog Import = new System.Windows.Forms.OpenFileDialog();
            Import.Title = "Chọn GeometryGroup files";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "SVG files (*.svg)|*.SVG";
            Import.FilterIndex = 2;
            Import.Multiselect = false;

            Import.ShowDialog();
            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {
                var fileName = Import.FileName;
                var xamlFileName = fileName.Replace(".svg", ".xaml");
                var settings = new WpfDrawingSettings { IncludeRuntime = true, TextAsGeometry = true };
                var converter = new FileSvgConverter(settings);

                using var fileStream = File.OpenRead(fileName);
                converter.Convert(fileStream, xamlFileName);
                var xaml = File.ReadAllText(xamlFileName);
                File.Delete(xamlFileName);

                var parsed = (DrawingGroup)XamlReader.Parse(xaml, new ParserContext { BaseUri = new Uri(System.IO.Path.GetDirectoryName(fileName)) });
                var geometry = GatherGeometry(parsed);

                var group = new DrawingGroup { Children = new DrawingCollection(geometry) };
                // var stringValue = "";
                var newItems = new List<IDrawable>();
                foreach (var geometryDrawing in geometry)
                {
                    var scaled = Geometry.Combine(
                        geometryDrawing.Geometry,
                        geometryDrawing.Geometry, GeometryCombineMode.Intersect,
                        new TransformGroup
                        {
                            Children = new TransformCollection
                            {
                            new TranslateTransform(group.Bounds.X * -1, group.Bounds.Y * -1),
                            new ScaleTransform(1.0 , 1.0 )
                            }
                        }
                    );

                    var scaledString = scaled.ToString(CultureInfo.InvariantCulture)
                        .Replace("F1", "")
                        .Replace(";", ",")
                        .Replace("L", " L")
                        .Replace("C", " C");

                    //stringValue = stringValue + " " + scaledString;
                    var lastSpotID = CanvasItems.Where(s => s is DeviceSpot).Count();
                    var shape = Geometry.Parse(scaledString.Trim());
                    var newItem = new DeviceSpot(
                           shape.Bounds.Top,
                           shape.Bounds.Left,
                           shape.Bounds.Width,
                           shape.Bounds.Height,
                           1,
                           1,
                           1,
                           1,
                           lastSpotID,
                           lastSpotID,
                           lastSpotID,
                           lastSpotID,
                           lastSpotID,
                           false,
                           shape);
                    newItem.IsSelected = true;
                    newItem.IsDeleteable = true;
                    CanvasItems.Add(newItem);
                }
                //create geometry



            }
        }
        public string InputShapeData { get; set; }
        private void AddSpotGeometry()
        {
            System.Windows.Forms.OpenFileDialog Import = new System.Windows.Forms.OpenFileDialog();
            Import.Title = "Chọn GeometryGroup files";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "Text files (*.txt)|*.TXT";
            Import.FilterIndex = 2;
            Import.Multiselect = false;

            Import.ShowDialog();

            var text = System.IO.File.ReadAllText(Import.FileName);
            try
            {
                StringReader sr = new StringReader(text);

                XmlReader reader = XmlReader.Create(sr);

                Geometry geo = (Geometry)XamlReader.Load(reader);


                var lastSpotID = CanvasItems.Where(s => s is DeviceSpot).Count();
                var newItem = new DrawableShape()
                {
                    Geometry = geo,
                };
                AvailableShapeToAdd.Add(newItem);

            }
            catch (Exception ex)
            {

            }

        }
        #endregion
    }
}
