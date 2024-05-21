using adrilight.View;
using adrilight_shared.Models.AppUser;
using adrilight_shared.Settings;
using FTPServer;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace adrilight.Services.AdrilightStoreService
{
    public class AdrilightSFTPClient
    {
        #region Construct
        public AdrilightSFTPClient(IGeneralSettings generalSettings)
        {
            _ftpServer = new FTPServerHelpers();
            _generalSettings = generalSettings;
            _appUser = new AppUser(public_User_DisplayName, public_User_LoginName, public_User_Name, public_User_PassWord, public_User_Geometry);
        }
        #endregion

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
        private const string openRGBDevicesFolderPath = "/home/adrilight_enduser/ftp/files/OpenRGBDevices";
        private const string ambinoDevicesFolderPath = "/home/adrilight_enduser/ftp/files/AmbinoDevices";
        #endregion
        #endregion
        #region Properties
        private IGeneralSettings _generalSettings;
        private FTPServerHelpers _ftpServer;
        private AppUser _appUser;
        #endregion
        #region Methods
        public void Init()
        {

            if (_ftpServer != null && _ftpServer.sFTP.IsConnected)
            {
                _ftpServer.sFTP.Disconnect();
            }
            string userName = _appUser.LoginName;
            string password = _appUser.LoginPassword;
            _ftpServer.sFTP = new SftpClient(host, 1512, userName, password);
        }
        public bool Connect()
        {
            try
            {
                _ftpServer.sFTP.Connect();
                return true;
            }
            catch (Exception ex)
            {
                //show dialog
                return false;
            }

        }
        #endregion

        ///legacy data
        //string public_User_DisplayName = "Public User";
        //string public_User_LoginName = "adrilight_publicuser";
        //string public_User_Name = "adrilight_enduser";
        //string public_User_PassWord = "@drilightPublic";
        //var public_User_Geometry = Geometry.Parse("M340.769 341.93C340.339 377.59 315.859 404.13 280.389 408.04C279.507 408.217 278.64 408.468 277.799 408.79H62.9991C58.3591 407.79 53.6491 407.12 49.0891 405.79C19.9991 397.63 1.54915 375.17 0.269149 344.59C-1.06085 312.99 2.40915 281.86 14.4091 252.27C22.1591 233.18 33.4092 216.65 52.2892 206.57C63.6947 200.41 76.51 197.332 89.4692 197.64C93.5892 197.73 97.9192 199.81 101.679 201.85C107.739 205.15 113.359 209.24 119.239 212.85C153.432 233.956 187.619 233.93 221.799 212.77C226.539 209.83 231.389 207 235.799 203.66C243.979 197.53 252.979 196.76 262.689 198.19C289.209 202.09 307.929 216.61 320.559 239.78C331.869 260.49 336.999 283 339.199 306.1C340.372 318.006 340.896 329.967 340.769 341.93ZM168.139 197C222.309 196.85 266.769 152.16 266.399 98.2998C266.069 43.4698 221.899 -0.400216 167.399 -0.000215969C141.305 0.126814 116.327 10.6011 97.9461 29.1239C79.5657 47.6468 69.2847 72.7051 69.3592 98.7998C69.3892 152.52 113.999 197.16 168.139 197Z");
        //var public_User = new AppUser(public_User_DisplayName, public_User_LoginName, public_User_Name, public_User_PassWord, public_User_Geometry);
        //string developer_User_DisplayName = "Developer User";
        //string developer_User_LoginName = "adrilight_developeruser";
        //string developer_User_UserName = "adrilight_developeruser";
        //string developer_User_PassWord = "@12345";
        //var developer_User_Geometry = Geometry.Parse("M124.8 0C100.115 7.92343e-08 75.9836 7.32023 55.4585 21.035C34.9334 34.7497 18.9362 54.243 9.48993 77.0496C0.0436497 99.8562 -2.42742 124.952 2.38921 149.163C7.20584 173.374 19.0938 195.613 36.5498 213.068C54.0058 230.522 76.2458 242.408 100.457 247.223C124.669 252.038 149.764 249.565 172.57 240.117C195.376 230.668 214.868 214.67 228.581 194.143C242.294 173.617 249.612 149.486 249.61 124.8C249.608 91.7001 236.457 59.9567 213.051 36.5525C189.645 13.1482 157.9 -1.06243e-07 124.8 0ZM82.5003 162C82.283 163.745 81.5704 165.391 80.4468 166.743C79.3232 168.095 77.8357 169.097 76.1603 169.63C74.5014 170.308 72.679 170.48 70.9226 170.124C69.1663 169.768 67.5545 168.9 66.2903 167.63C54.197 155.63 42.167 143.58 30.2003 131.48C28.9703 130.24 28.3903 128.36 27.5103 126.78V122.98C28.3703 120.1 30.3203 117.98 32.4103 115.98C43.5903 104.813 54.7403 93.6433 65.8603 82.47C67.0717 81.1537 68.6333 80.21 70.3619 79.7495C72.0905 79.289 73.9146 79.3308 75.6203 79.87C77.3558 80.3104 78.9249 81.2482 80.1347 82.5682C81.3445 83.8881 82.1424 85.5328 82.4303 87.3C82.7777 88.9512 82.6832 90.6649 82.1565 92.2679C81.6297 93.8709 80.6894 95.3066 79.4303 96.43C70.477 105.357 61.517 114.307 52.5503 123.28C52.0703 123.76 51.6103 124.28 51.0703 124.84C51.6003 125.41 52.0703 125.91 52.5503 126.4C61.4503 135.307 70.3603 144.21 79.2803 153.11C81.7903 155.59 83.0903 158.5 82.5003 162ZM162.69 64.34C158.33 74.1 153.91 83.83 149.53 93.57C134.684 126.563 119.85 159.563 105.03 192.57C103.14 196.77 100.23 199.3 96.5903 199.4C88.3903 199.4 83.9003 192.17 86.8803 185.4C90.9803 176.15 95.1703 166.95 99.3303 157.72C114.404 124.22 129.48 90.7167 144.56 57.21C146.98 51.84 151.22 49.36 156.09 50.46C162.26 51.88 165.37 58.34 162.69 64.35V64.34ZM183.54 170.62C182.644 171.582 181.562 172.352 180.36 172.883C179.158 173.414 177.86 173.696 176.546 173.71C175.231 173.725 173.927 173.473 172.713 172.969C171.499 172.465 170.4 171.72 169.483 170.779C168.565 169.838 167.848 168.72 167.376 167.493C166.903 166.267 166.684 164.957 166.732 163.644C166.781 162.33 167.095 161.04 167.657 159.852C168.219 158.663 169.016 157.601 170 156.73C179.007 147.65 188.054 138.603 197.14 129.59C197.58 129.14 198.14 128.79 198.81 128.22C192.88 122.31 187.23 116.68 181.59 111.04C177.65 107.1 173.67 103.2 169.78 99.2C168.375 97.8044 167.44 96.0063 167.103 94.0549C166.766 92.1036 167.045 90.0958 167.9 88.31C168.707 86.59 169.997 85.1423 171.613 84.1442C173.229 83.146 175.101 82.6407 177 82.69C179.435 82.7522 181.75 83.7559 183.46 85.49C195.39 97.39 207.33 109.26 219.18 121.24C220.121 122.128 220.869 123.2 221.377 124.389C221.886 125.578 222.145 126.859 222.137 128.152C222.13 129.446 221.856 130.724 221.334 131.907C220.811 133.09 220.051 134.153 219.1 135.03C207.307 146.937 195.454 158.803 183.54 170.63V170.62Z");
        //var developer_User = new AppUser(developer_User_DisplayName, developer_User_LoginName, developer_User_UserName, developer_User_PassWord, developer_User_Geometry);
        ///

    }
}
