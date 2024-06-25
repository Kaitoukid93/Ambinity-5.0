﻿using adrilight.View;
using adrilight.View.Screens.Mainview.ControlView;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.DataSource;
using adrilight_shared.Models.ItemsCollection;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Services;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight.Views;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace adrilight.ViewModel.DeviceControl
{
    public class ListSelectionParameterViewModel : ControlParameterViewModelBase
    {
        public ListSelectionParameterViewModel()
        {
            _localFileHlprs = new LocalFileHelpers();
            CommandSetup();
            _progress = new Progress<int>(percent =>
            {
                LoadingPercent = percent;
            });

        }
        #region Events
        private void AvailableValues_ItemCheckStatusChanged(IGenericCollectionItem obj)
        {
            UpdateTools();
        }
        private void OnAddNewValue(IGenericCollectionItem item)
        {
            var vm = new AddNewDialogViewModel(adrilight_shared.Properties.Resources.AddNew, "New Item", "newfile");
            DialogService.ShowDialog<AddNewDialogViewModel>(result =>
            {
                if (result == "True")
                {
                    //tell automation manager to add new automation
                    item.Name = vm.Content;
                    InsertItem(item);
                }

            }, vm);


        }
        #endregion
        private IProgress<int> _progress;
        private bool _progressBarVisibility;
        public bool ProgressBarVisibility {
            get
            {
                return _progressBarVisibility;
            }
            set
            {
                _progressBarVisibility = value;
                if (!value)
                {
                    _progress.Report(0);
                }
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<CollectionItemTool> _availableTools;
        public ObservableCollection<CollectionItemTool> AvailableTools {
            get
            {
                return _availableTools;
            }
            set
            {
                _availableTools = value;
                RaisePropertyChanged();
            }
        }
        private int _loadingPercent;
        public int LoadingPercent {
            get { return _loadingPercent; }
            set
            {
                if (_loadingPercent != value) { _loadingPercent = value; RaisePropertyChanged(); }
            }
        }

        private LocalFileHelpers _localFileHlprs;
        public bool ShowToolBar => AvailableTools.Count > 0;
        private int _uniformGridRowNumber = 1;
        public int UniformGridRowNumber {
            get
            {
                return _uniformGridRowNumber;
            }
            set
            {
                _uniformGridRowNumber = value;
                RaisePropertyChanged();
            }
        }
        private ItemsCollection _availablevalues;
        public ItemsCollection AvailableValues {
            get
            {
                return _availablevalues;
            }
            set
            {
                _availablevalues = value;
                RaisePropertyChanged();
            }
        }
        private bool _showMore;
        public bool ShowMore {
            get
            {
                return _showMore;
            }
            set
            {
                _showMore = value;
                RaisePropertyChanged();
            }
        }
        private bool _isLoadingValue;
        private int _selectedDataSourceIndex;
        public int SelectedDataSourceIndex {
            get
            {
                return _selectedDataSourceIndex;
            }
            set
            {
                _selectedDataSourceIndex = value;
                RaisePropertyChanged();
                LoadAvailableValues();

            }
        }
        public List<string> DataSourceLocaFolderNames { get; set; }

        #region Methods
        public override void Init(IModeParameter param)
        {
            Parameter = param;
            TemplateSelector = param.Template;
            Type = param.ParamType;
            AvailableTools = new ObservableCollection<CollectionItemTool>();
        }
        public override void CommandSetup()
        {
            ParameterClickCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExecuteparameterClick(p);
            });
            CollectionItemToolCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                switch (p)
                {
                    case "delete":
                        var vm = new DeleteDialogViewModel(adrilight_shared.Properties.Resources.DeleteDialog_Name, adrilight_shared.Properties.Resources.DeleteDialog_Confirm_Header);
                        DialogService.ShowDialog<DeleteDialogViewModel>(result =>
                        {
                            if (result == "True")
                            {
                                //also remove database
                                RemoveSelectedItems();
                            }

                        }, vm);

                        break;
                }
                UpdateTools();
            });
            CollectionItemMouseLeaveCommand = new RelayCommand<Gif>((p) =>
            {
                return true;
            }, (p) =>
            {
                p.DisposeGif();
            });
            CollectionItemMouseEnterCommand = new RelayCommand<Gif>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await p.PlayGif(5);
            });
            ItemClickCommand = new RelayCommand<SelectionChangedEventArgs>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (p.AddedItems.Count <= 0)
                    return;
                (Parameter as ListSelectionParameter).SelectedValue = (IParameterValue)p.AddedItems[0];
            });
            RefreshButtonCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                RefreshCollection();
            });
            SubParameterButtonClickCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                switch (p)
                {
                    case "Add Color":
                        //OpenColorPickerWindow(2);
                        OpenColorEditorWindow();
                        break;

                    case "Add VID":
                        // IdEditMode = IDMode.VID;
                        //IsInIDEditStage = true;
                        break;

                    case "Add FID":
                        //IdEditMode = IDMode.FID;
                        //IsInIDEditStage = true;
                        break;
                    case "Add Palette":
                        OpenPaletteEditorWindow();
                        break;

                    case "Import Palette":
                        //ImportPaletteFromFile();
                        break;
                    case "Import Gif":
                        //ImportGifFromFile();
                        var gif = _localFileHlprs.OpenImportFileDialog("gif", "gif files (*.gif)|*.Gif|Image files (*.jpg)|*.Jpeg");
                        if (gif != null)
                        {
                            var newGif = new Gif() {
                                LocalPath = gif,
                                Name = Path.GetFileName(gif),
                                Owner = "User imported Gif"

                            };
                            InsertItem(newGif);
                        }
                        break;
                    case "Import Pattern":
                        var motion = _localFileHlprs.OpenImportFileDialog("AML", "AML files (*.AML)|*.Aml|Text files (*.txt)|*.Txt");

                        if (motion != null)
                        {
                            var newPattern = new ChasingPattern() {
                                LocalPath = motion,
                                Name = Path.GetFileName(motion),
                                Owner = "User imported Pattern"

                            };
                            InsertItem(newPattern);
                        }
                        break;
                    case "Export Palette":
                        //var _paletteControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Palette).FirstOrDefault();
                        //var palette = (_paletteControl as ListSelectionParameter).SelectedValue;
                        //ExportItemForOnlineStore(palette);
                        break;
                    case "Export Gif":
                        //var _gifControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Gifs).FirstOrDefault();
                        //var gif = (_gifControl as ListSelectionParameter).SelectedValue;
                        //ExportItemForOnlineStore(gif);
                        break;
                    case "Export Pattern":
                        //var _patternControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.ChasingPattern).FirstOrDefault();
                        //var pattern = (_patternControl as ListSelectionParameter).SelectedValue;
                        //ExportItemForOnlineStore(pattern);
                        break;
                }
            });
        }
        private void OpenPaletteEditorWindow()
        {
            var vm = new ColorPaletteCreatorViewModel();
            vm.AddNewPalette += OnAddNewValue;
            var window = new PaletteCreatorWindow();
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }
        private void OpenColorEditorWindow()
        {
            var vm = new ColorEditorViewModel();
            vm.AddNewColor += OnAddNewValue;
            var window = new ColorGradientCreatorWindow();
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }
        private void ExecuteparameterClick(string parameter)
        {
            if (ShowMore)
            {
                ShowMore = false;
                AvailableValues?.ResetSelectionStage();
                return;
            }
            else
            {
                LoadAvailableValues();
            }
        }
        private void AddItem(IGenericCollectionItem item)
        {
            AvailableValues.AddItem(item);
            var listparam = Parameter as ListSelectionParameter;
            var selectedSourceName = listparam.DataSourceLocaFolderNames[SelectedDataSourceIndex];
            var source = (DataSourceBase)DataSources.Where(s => s.Name == selectedSourceName).FirstOrDefault();
            if (source == null)
                return;
            source.AddItem(item);
        }
        private void InsertItem(IGenericCollectionItem item)
        {
            var listparam = Parameter as ListSelectionParameter;
            var selectedSourceName = listparam.DataSourceLocaFolderNames[SelectedDataSourceIndex];
            var source = (DataSourceBase)DataSources.Where(s => s.Name == selectedSourceName).FirstOrDefault();
            if (source == null)
                return;
            if (!source.InsertItem(item))
                return;
            AvailableValues.InsertItem(item);
        }
        private void RemoveSelectedItems()
        {
            AvailableValues.RemoveSelectedItems();
            var listparam = Parameter as ListSelectionParameter;
            var selectedSourceName = listparam.DataSourceLocaFolderNames[SelectedDataSourceIndex];
            var source = (DataSourceBase)DataSources.Where(s => s.Name == selectedSourceName).FirstOrDefault();
            if (source == null)
                return;
            source.RemoveSelectedItems();
        }
        private async Task LoadAvailableValues()
        {
            if (_isLoadingValue)
                return;
            ProgressBarVisibility = true;
            _isLoadingValue = true;
            AvailableValues = new ItemsCollection();
            await Task.Delay(100);
            ShowMore = true;
            AvailableValues.ItemCheckStatusChanged += AvailableValues_ItemCheckStatusChanged;
            //get current data source
            //load all values
            //create new items collection from that value


            var listparam = Parameter as ListSelectionParameter;
            var selectedSourceName = listparam.DataSourceLocaFolderNames[SelectedDataSourceIndex];
            var source = (DataSourceBase)DataSources.Where(s => s.Name == selectedSourceName).FirstOrDefault();
            if (source == null)
                return;
            await Task.Run(() => source.LoadData());
            int count = 0;
            UniformGridRowNumber = (int)Math.Ceiling(source.Items.Count / 4d);
            var step = 100/(source.Items.Count/4);
            var percent = step;
            foreach (var value in source.Items)
            {
                AvailableValues.AddItem(value);
                count++;
                
                if (count % 4 == 0)
                {
                    await Task.Delay(150);
                        if(LoadingPercent>100)
                        LoadingPercent = 100;
                    _progress.Report(percent += step);
                }
                    
            }



            UpdateTools();
            await Task.Delay(100);
            _isLoadingValue = false;
            ProgressBarVisibility = false;
        }


        private void UpdateTools()
        {
            //clear Tool
            AvailableTools?.Clear();
            var selectedItems = AvailableValues.Items.Where(d => d.IsChecked).ToList();
            if (selectedItems != null && selectedItems.Count > 0)
                AvailableTools.Add(DeleteTool());
            RaisePropertyChanged(nameof(ShowToolBar));

        }
        private void RefreshCollection()
        {
            LoadAvailableValues();
        }
        private CollectionItemTool DeleteTool()
        {
            return new CollectionItemTool() {
                Name = "Delete",
                ToolTip = "Delete Selected Items",
                Geometry = "remove",
                CommandParameter = "delete",
                Command = CollectionItemToolCommand

            };
        }
        public override void Dispose()
        {
            AvailableValues?.ResetSelectionStage();
            base.Dispose();
        }
        #endregion

        #region Icommand

        public ICommand SubParameterButtonClickCommand { get; set; }
        public ICommand GetMoreValueButtonClickCommand { get; set; }
        public ICommand ShowMoreButtonClickCommand { get; set; }
        public ICommand ItemClickCommand { get; set; }
        public ICommand CollectionItemToolCommand { get; set; }
        public ICommand CollectionItemMouseEnterCommand { get; set; }

        public ICommand CollectionItemMouseLeaveCommand { get; set; }
        public ICommand RefreshButtonCommand { get; set; }
        #endregion
    }

}