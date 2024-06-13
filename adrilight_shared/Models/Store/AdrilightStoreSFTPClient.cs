using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.User;
using adrilight_shared.Settings;
using FTPServer;
using GalaSoft.MvvmLight;
using HandyControl.Controls;
using Newtonsoft.Json;
using Renci.SshNet;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace adrilight_shared.Models.Store
{
    /// <summary>
    /// this contains properties and method to fetch data from adrilight server
    /// </summary>
    public class AdrilightStoreSFTPClient : ViewModelBase
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string CacheFolderPath => Path.Combine(JsonPath, "Cache");
        private string SupportedDeviceCollectionFolderPath => Path.Combine(JsonPath, "SupportedDevices");
        private string PalettesCollectionFolderPath => Path.Combine(JsonPath, "ColorPalettes");
        private string AnimationsCollectionFolderPath => Path.Combine(JsonPath, "Animations");
        private string ChasingPatternsCollectionFolderPath => Path.Combine(JsonPath, "ChasingPatterns");
        private string LightingProfilesCollectionFolderPath => Path.Combine(JsonPath, "LightingProfiles");
        private string GifsCollectionFolderPath => Path.Combine(JsonPath, "Gifs");
        #region Constant value
        private const string host = @"103.148.57.184";
        private const string public_User_DisplayName = "Public User";
        private const string public_User_LoginName = "adrilight_publicuser";
        private const string public_User_Name = "adrilight_enduser";
        private const string public_User_PassWord = "@drilightPublic";
        private Geometry public_User_Geometry = Geometry.Parse("M340.769 341.93C340.339 377.59 315.859 404.13 280.389 408.04C279.507 408.217 278.64 408.468 277.799 408.79H62.9991C58.3591 407.79 53.6491 407.12 49.0891 405.79C19.9991 397.63 1.54915 375.17 0.269149 344.59C-1.06085 312.99 2.40915 281.86 14.4091 252.27C22.1591 233.18 33.4092 216.65 52.2892 206.57C63.6947 200.41 76.51 197.332 89.4692 197.64C93.5892 197.73 97.9192 199.81 101.679 201.85C107.739 205.15 113.359 209.24 119.239 212.85C153.432 233.956 187.619 233.93 221.799 212.77C226.539 209.83 231.389 207 235.799 203.66C243.979 197.53 252.979 196.76 262.689 198.19C289.209 202.09 307.929 216.61 320.559 239.78C331.869 260.49 336.999 283 339.199 306.1C340.372 318.006 340.896 329.967 340.769 341.93ZM168.139 197C222.309 196.85 266.769 152.16 266.399 98.2998C266.069 43.4698 221.899 -0.400216 167.399 -0.000215969C141.305 0.126814 116.327 10.6011 97.9461 29.1239C79.5657 47.6468 69.2847 72.7051 69.3592 98.7998C69.3892 152.52 113.999 197.16 168.139 197Z");
        #region Store Folder Path
        private const string paletteFolderpath = "/home/adrilight_enduser/ftp/files/ColorPalettes";
        private const string chasingPatternsFolderPath = "/home/adrilight_enduser/ftp/files/ChasingPatterns";
        private const string gifxelationsFolderPath = "/home/adrilight_enduser/ftp/files/Gifxelations";
        private const string SupportedDevicesFolderPath = "/home/adrilight_enduser/ftp/files/SupportedDevices";
        private const string ProfilesFolderPath = "/home/adrilight_enduser/ftp/files/Profiles";
        private const string thumbResourceFolderPath = "/home/adrilight_enduser/ftp/files/Resources/Thumbs";
        private const string carouselFolderPath = "/home/adrilight_enduser/ftp/files/HomePage/Carousel";
        #endregion
        #endregion
        #region Construct
        public AdrilightStoreSFTPClient()
        {
            _ftpServer = new FTPServerHelpers();
            _appUser = new AppUser(public_User_DisplayName, public_User_LoginName, public_User_Name, public_User_PassWord, public_User_Geometry);
        }
        #endregion
        #region Properties
        private FTPServerHelpers _ftpServer;
        private AppUser _appUser;
        private int _currentDownloadProgress;
        public int CurrentDownloadProgress
        {
            get { return _currentDownloadProgress; }
            set
            {
                _currentDownloadProgress = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        #region Methods
        public void Init()
        {
            string userName = _appUser.LoginName;
            string password = _appUser.LoginPassword;
            _ftpServer.sFTP = new SftpClient(host, 1512, userName, password);
            _ftpServer.sFTP.Connect();
            IsInit = true;
        }
        public void Dispose()
        {
            _ftpServer.sFTP.Dispose();
            IsInit = false;
            GC.SuppressFinalize(this);
        }
        public bool IsInit { get; set; }
        public List<StoreCategory> GetStoreCategories()
        {
            var availableCatergories = new List<StoreCategory>();
            var home = new StoreCategory()
            {
                Name = "Home",
                OnlineFolderPath = "/home/adrilight_enduser/ftp/files/HomePage",
                LocalFolderPath = CacheFolderPath,
                DataType = typeof(ColorPalette),
                Description = "Home page",
                Geometry = "onlineStore"
            };
            var palettes = new StoreCategory()
            {
                Name = "Colors",
                OnlineFolderPath = "/home/adrilight_enduser/ftp/files/ColorPalettes",
                LocalFolderPath = PalettesCollectionFolderPath,
                DataType = typeof(ColorPalette),
                Description = "All Color Palette created by Ambino and Contributed by Ambino Community",
                Geometry = "palette"
            };
            var chasingPatterns = new StoreCategory()
            {
                Name = "Animations",
                OnlineFolderPath = "/home/adrilight_enduser/ftp/files/ChasingPatterns",
                LocalFolderPath = ChasingPatternsCollectionFolderPath,
                DataType = typeof(ChasingPattern),
                Description = "All  Animations created by Ambino and Contributed by Ambino Community",
                Geometry = "chasingPattern"
            };
            var gif = new StoreCategory()
            {
                Name = "Gifs",
                DataType = typeof(Gif),
                OnlineFolderPath = "/home/adrilight_enduser/ftp/files/Gifxelations",
                LocalFolderPath = GifsCollectionFolderPath,
                Description = "All Gifs created by Ambino and Contributed by Ambino Community",
                Geometry = "gifxelation"
            };
            var supportedDevices = new StoreCategory()
            {
                Name = "Devices",
                DataType = typeof(ARGBLEDSlaveDevice),
                OnlineFolderPath = "/home/adrilight_enduser/ftp/files/SupportedDevices",
                LocalFolderPath = SupportedDeviceCollectionFolderPath,
                Description = "All Color Palette created by Ambino and Contributed by Ambino Community",
                Geometry = "slaveDevice"
            };
            var lightingProfiles = new StoreCategory()
            {
                Name = "Lighting",
                DataType = typeof(LightingProfile),
                OnlineFolderPath = "/home/adrilight_enduser/ftp/files/LightingProfiles",
                LocalFolderPath = SupportedDeviceCollectionFolderPath,
                Description = "All Lighting Profiles created by Ambino and Contributed by Ambino Community",
                Geometry = "appProfile"
            };
            availableCatergories.Add(home);
            availableCatergories.Add(palettes);
            availableCatergories.Add(lightingProfiles);
            availableCatergories.Add(chasingPatterns);
            availableCatergories.Add(gif);
            availableCatergories.Add(supportedDevices);
            return availableCatergories;
        }
        public class StringIEqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }
        private string GetInfoPath(OnlineItemModel item)
        {
            string localPath = string.Empty;
            switch (item.Type)
            {
                case "ARGBLEDSlaveDevice":
                    localPath = SupportedDeviceCollectionFolderPath;
                    break;
                case "Gif":
                    localPath = GifsCollectionFolderPath;
                    break;
                case "ColorPalette":
                    localPath = PalettesCollectionFolderPath;
                    break;
                case "ChasingPattern":
                    localPath = ChasingPatternsCollectionFolderPath;
                    break;
                case "LightingProfile":
                    localPath = LightingProfilesCollectionFolderPath;
                    break;
            }
            var itemLocalInfo = Path.Combine(localPath, "info", item.Name + ".info");
            return itemLocalInfo;
        }

        /// <summary>
        /// Get Items by list address,page index should defines number of items in list
        /// </summary>
        /// <param name="listAddress"></param>
        /// <returns></returns>
        /// 

        private void CheckItemForExistance(OnlineItemModel item)
        {
            var itemLocalInfo = GetInfoPath(item);
            if (File.Exists(itemLocalInfo))
            {
                item.IsLocalExisted = true;
                var json = File.ReadAllText(itemLocalInfo);
                var localInfo = JsonConvert.DeserializeObject<OnlineItemModel>(json);
                var v1 = new Version(localInfo.Version);
                var v2 = new Version(item.Version);
                if (v2 > v1)
                    item.IsUpgradeAvailable = true;
            }
        }
        public async Task<List<OnlineItemModel>> GetStoreItems(List<string> listAddress)
        {

            //download items
            var items = new List<OnlineItemModel>();
            if (listAddress == null)
                return await Task.FromResult(items);

            foreach (var url in listAddress)
            {
                //get name
                var itemName = _ftpServer.GetFileOrFoldername(url).Name;
                //get description content
                var descriptionPath = url + "/description.md";
                var description = await _ftpServer.GetStringContent(descriptionPath);
                var itemList = new List<OnlineItemModel>();
                var infoPath = url + "/info.json";
                var info = _ftpServer.GetFiles<OnlineItemModel>(infoPath).Result;
                info.Path = url;
                CheckItemForExistance(info);
                items.Add(info);
            }
            return await Task.FromResult(items);
        }
        public async Task<List<StoreFilterModel>> GetCatergoryFilter(string path)
        {
            var listFilter = new List<StoreFilterModel>();
            if (path == null || path == string.Empty)
            {
                return await Task.FromResult(listFilter);
            }

            listFilter = _ftpServer.GetFiles<List<StoreFilterModel>>(path).Result;
            return await Task.FromResult(listFilter);
        }
        /// <summary>
        /// Get Items by filter
        /// filter contains page index
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// 
        private async Task<List<string>> FilterItemsByNameAndContent(List<string> items, string nameFilter)
        {
            var lowerFilter = string.Empty;
            //filter name
            if (nameFilter != null)
                lowerFilter = nameFilter.ToLower();
            //filter by supported device
            var filteredItemAddress = new List<string>();
            foreach (var address in items)
            {
                //get name
                var itemName = await Task.Run(() => _ftpServer.GetFileOrFoldername(address).Name);
                if (itemName.Contains("filters"))
                    continue;
                //get description content
                //var descriptionPath = address + "/description.md";
                // var description = await _ftpServer.GetStringContent(descriptionPath);
                if (itemName.ToLower().Contains(lowerFilter) || lowerFilter == string.Empty)
                {
                    filteredItemAddress.Add(address);
                }
            }
            return filteredItemAddress;
        }

        public async Task<(List<OnlineItemModel>, int)> GetStoreItems(StoreFilterModel filter, int offset, int numItem)
        {
            var currentPageListItemAddress = new List<string>();
            var currentDeviceTypeFilter = filter.DeviceTypeFilter;
            var currentnameFilter = filter.NameFilter;
            var currentCatergoryFilter = filter.CatergoryFilter;
            var listItemAddress = new List<string>();
            if (currentCatergoryFilter == null)
            {

                listItemAddress.AddRange(await _ftpServer.GetAllFilesAddressInFolder(paletteFolderpath));
                listItemAddress.AddRange(await _ftpServer.GetAllFilesAddressInFolder(chasingPatternsFolderPath));
                listItemAddress.AddRange(await _ftpServer.GetAllFilesAddressInFolder(gifxelationsFolderPath));
                listItemAddress.AddRange(await _ftpServer.GetAllFilesAddressInFolder(SupportedDevicesFolderPath));
                listItemAddress.AddRange(await _ftpServer.GetAllFilesAddressInFolder(ProfilesFolderPath));
            }
            else
            {
                var itemFolderPath = currentCatergoryFilter.OnlineFolderPath;
                listItemAddress = await _ftpServer.GetAllFilesAddressInFolder(itemFolderPath);
            }
            // get itemfolderPath and itemLocalFolderPath based on datatype if datatpye == all search all folder

            var pageIndex = filter.PageIndex;
            //filter item by name if exist
            if (listItemAddress != null && listItemAddress.Count > 0)
            {
                var filteredItemAddress = new List<string>();
                if (currentnameFilter != null)
                {
                    filteredItemAddress = await FilterItemsByNameAndContent(listItemAddress, currentnameFilter);
                }
                else
                {
                    filteredItemAddress = listItemAddress;
                }
                var finalItemList = new List<OnlineItemModel>();
                foreach (var address in filteredItemAddress.Skip(offset).Take(numItem).ToList())
                {
                    var infoPath = address + "/info.json";
                    var info = new OnlineItemModel();
                    try
                    {
                        info = _ftpServer.GetFiles<OnlineItemModel>(infoPath).Result;
                        if (info != null)
                        {
                            info.Path = address;
                            if (info.TargetDevices != null && currentDeviceTypeFilter != null && info.TargetDevices.Any(t => t.Type.ToString() == currentDeviceTypeFilter))
                            {
                                finalItemList.Add(info);
                            }
                            else if (currentDeviceTypeFilter == null)
                            {
                                finalItemList.Add(info);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }
                foreach (var item in finalItemList)
                {
                    CheckItemForExistance(item);
                }
                //foreach (var item in itemList)
                //{
                //    if (item.AvatarType == OnlineItemAvatarTypeEnum.Image)
                //    {
                //        var thumbPath = item.Path + "/thumb.png";
                //        item.Thumb = _ftpServer.GetThumb(thumbPath).Result;
                //    }
                //}
                return (finalItemList, filteredItemAddress.Count);
            }
            else
            {
                return (null, 0);
            }
        }
        public async Task<List<HomePageCarouselItem>> GetCarousel()
        {
            var listCarousels = new List<HomePageCarouselItem>();
            var listItemAddress = await _ftpServer.GetAllFilesAddressInFolder(carouselFolderPath);
            if (listItemAddress == null)
                return null;
            foreach (var address in listItemAddress)
            {
                try
                {
                    var infoPath = address + "/config.json";
                    var info = _ftpServer.GetFiles<HomePageCarouselItem>(infoPath).Result;
                    info.Path = address;
                    listCarousels.Add(info);
                }

                catch (Exception ex)
                {

                }
            }
            return listCarousels;
        }
        public async Task<BitmapImage> GetThumb(string thumbPath)
        {
            return await _ftpServer.GetThumb(thumbPath);
        }
        public async Task<List<BitmapImage>> GetItemScreenShots(OnlineItemModel item)
        {
            var screenShots = new List<BitmapImage>();
            var screenshotsPath = item.Path + "/screenshots";
            foreach (var file in _ftpServer.GetAllFilesAddressInFolder(screenshotsPath).Result)
            {
                var img = await _ftpServer.GetScreenShot(file);
                screenShots.Add(img);
            }
            return screenShots;
        }
        public async Task<string> GetItemDescription(OnlineItemModel item)
        {
            var descriptionPath = item.Path + "/description.md";
            var description = await _ftpServer.GetStringContent(descriptionPath);
            return description;
        }
        
        public async Task SaveItemToLocalCollection<T>(string collectionPath,
            DeserializeMethodEnum mode,
            OnlineItemModel item,
            string extension,
            IProgress<int> progress = null)
        {
            item.IsDownloading = true;
            Log.Information("Downloading Item" + " " + item.Name);
            if (!Directory.Exists(collectionPath))
                Directory.CreateDirectory(collectionPath);
            //get list of files
            var listofFiles = await _ftpServer.GetAllFilesInFolder(item.Path + "/content");
            if (listofFiles == null)
            {
                HandyControl.Controls.MessageBox.Show("Không tìm thấy file, vui lòng chọn nội dung khác!!", "File not found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            switch (mode)
            {
                case DeserializeMethodEnum.MultiJson:
                    //save to local folder , we only need json config file
                    var data = await _ftpServer.GetFiles<T>(item.Path + "/content" + "/" + "config.json");
                    WriteSimpleJson(data, Path.Combine(Path.Combine(collectionPath, "collection"), item.Name + extension));
                    break;

                case DeserializeMethodEnum.FolderStructure:
                    var localFolderPath = Path.Combine(collectionPath, item.Name);
                    Directory.CreateDirectory(localFolderPath);
                    foreach (var file in listofFiles)
                    {
                        //save to local folder
                        var remotePath = item.Path + "/content" + "/" + file.Name;
                        _ftpServer.DownloadFile(remotePath, localFolderPath + "/" + file.Name, progress);
                    }
                    break;
                case DeserializeMethodEnum.Files:
                    var localFilePath = Path.Combine(collectionPath, "collection");
                    var thumbFolder = Path.Combine(localFilePath, "thumb");
                    if (!Directory.Exists(localFilePath))
                    {
                        Directory.CreateDirectory(localFilePath);
                        
                    }

                    if (!Directory.Exists(thumbFolder))
                    {
                        Directory.CreateDirectory(thumbFolder);
                    }
                    foreach (var file in listofFiles.Where(f => f.Name != "thumbnail.png"))
                    {
                        //save to local folder
                        var remotePath = item.Path + "/content" + "/" + file.Name;
                        var thumbPath = item.Path + "/thumb.png";
                        _ftpServer.DownloadFile(remotePath, localFilePath + "/" + file.Name, progress);
                        _ftpServer.DownloadFile(thumbPath, thumbFolder + "/" + item.Name + ".png", progress);
                    }
                    break;
            }
            //download online details for upgrade purpose
            await Task.Delay(TimeSpan.FromSeconds(1));
            item.IsDownloading = false;
            Log.Information("Item Downloaded" + " " + item.Name);
            item.IsLocalExisted = true;
            Log.Information("Generating Online Infomation" + " " + item.Name);
            var itemInfo = new OnlineItemModel();
            itemInfo.Name = item.Name;
            itemInfo.Owner = item.Owner;
            itemInfo.Version = item.Version;
            itemInfo.Type = item.Type;
            var infoFolderPath = Path.Combine(collectionPath, "info");
            if (!Directory.Exists(infoFolderPath))
                Directory.CreateDirectory(infoFolderPath);
            JsonHelpers.WriteSimpleJson(itemInfo, Path.Combine(infoFolderPath, item.Name + ".info"));
            Log.Information("Online Infomation Created" + " " + item.Name);
            item.IsUpgradeAvailable = false;


        }

        public async Task DownloadCurrentOnlineItem(OnlineItemModel o, IProgress<int> progress = null)
        {
            //create needed info
            string localFolder = string.Empty; // the resource folder this item will be saved to
            switch (o.Type)
            {
                case "ColorPalette":
                    await SaveItemToLocalCollection<ColorPalette>(PalettesCollectionFolderPath, DeserializeMethodEnum.MultiJson, o, ".col", progress);
                    break;

                case "ARGBLEDSlaveDevice":
                    await SaveItemToLocalCollection<ARGBLEDSlaveDevice>(SupportedDeviceCollectionFolderPath, DeserializeMethodEnum.FolderStructure, o, string.Empty, progress);
                    //if (SlaveDeviceSelection != null)
                    //    RefreshLocalSlaveDeviceCollection();
                    break;
                case "Gif":
                    await SaveItemToLocalCollection<Gif>(GifsCollectionFolderPath, DeserializeMethodEnum.Files, o, "", progress);
                    break;
                case "ChasingPattern":
                    await SaveItemToLocalCollection<ChasingPattern>(ChasingPatternsCollectionFolderPath, DeserializeMethodEnum.Files, o, "AML", progress);
                    break;
                case "LightingProfile":
                    await SaveItemToLocalCollection<LightingProfile>(LightingProfilesCollectionFolderPath, DeserializeMethodEnum.MultiJson, o, ".ALP", progress);
                    break;
            }
        }
        private void WriteSimpleJson(object obj, string path)
        {
            try
            {
                var json = JsonConvert.SerializeObject(obj);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                //log
            }
        }
        #endregion



    }
}
