using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.Stores;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_shared.Models.Lighting
{
    public class ProfilesManager
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string LightingProfilesCollectionFolderPath => Path.Combine(JsonPath, "LightingProfiles");
        private string LightingProfilePlaylistsCollectionFolderPath => Path.Combine(JsonPath, "Playlists");
        public ProfilesManager() {
            CreateLightingProfilePlaylistsCollection();
            CreateLightingProfilesCollection();
        }
        private void CreateLightingProfilesCollection()
        {
            if (!Directory.Exists(LightingProfilesCollectionFolderPath))
            {
                Directory.CreateDirectory(LightingProfilesCollectionFolderPath);
                var collectionFolder = Path.Combine(LightingProfilesCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                //var allResourceNames = "adrilight.Resources.Colors.ChasingPatterns.json";
                var _resourceHelpers = new ResourceHelpers();
                var allResourceNames = _resourceHelpers.GetResourceFileNames();
                foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".ALP")))
                {
                    var name = _resourceHelpers.GetResourceFileName(resourceName);
                    _resourceHelpers.CopyResource(resourceName, Path.Combine(collectionFolder, name));
                }
                var config = new ResourceLoaderConfig(nameof(LightingProfile), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(LightingProfilesCollectionFolderPath, "config.json"), configJson);
                //copy default chasing pattern from resource
            }
        }
        private void CreateLightingProfilePlaylistsCollection()
        {
            if (!Directory.Exists(LightingProfilePlaylistsCollectionFolderPath))
            {
                Directory.CreateDirectory(LightingProfilePlaylistsCollectionFolderPath);
                var collectionFolder = Path.Combine(LightingProfilePlaylistsCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                var demoPlaylist = new LightingProfilePlaylist("All profiles");
                foreach (var profile in LoadLightingProfileIfExist())
                {
                    demoPlaylist.LightingProfiles.Add(profile);
                    demoPlaylist.LightingProfilesUID.Add((profile as LightingProfile).ProfileUID);
                    demoPlaylist.IsDeleteable = false;
                }
                JsonHelpers.WriteSimpleJson(demoPlaylist, Path.Combine(collectionFolder, demoPlaylist.Name + ".LPP"));
                ///
                var config = new ResourceLoaderConfig(nameof(LightingProfilePlaylist), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(LightingProfilePlaylistsCollectionFolderPath, "config.json"), configJson);
                //copy default chasing pattern from resource
            }
        }

        public List<LightingProfile> LoadLightingProfileIfExist()
        {
            var existedProfile = new List<LightingProfile>();
            string[] files = Directory.GetFiles(Path.Combine(LightingProfilesCollectionFolderPath, "collection"));
            foreach (var file in files)
            {
                var jsonData = File.ReadAllText(file);
                var profile = JsonConvert.DeserializeObject<LightingProfile>(jsonData);
                if (profile == null)
                    continue;
                profile.LocalPath = file;
                existedProfile.Add(profile);
            }
            return existedProfile;
        }
        public List<LightingProfilePlaylist> LoadLightingProfilePlaylistIfExist()
        {
            var existedPlaylist = new List<LightingProfilePlaylist>();
            string[] files = Directory.GetFiles(Path.Combine(LightingProfilePlaylistsCollectionFolderPath, "collection"));
            foreach (var file in files)
            {
                var jsonData = File.ReadAllText(file);
                var playlist = JsonConvert.DeserializeObject<LightingProfilePlaylist>(jsonData);
                if (playlist == null)
                    continue;
                playlist.LocalPath = file;
                if (playlist != null)
                {
                    playlist.LoadLightingProfiles(LoadLightingProfileIfExist());
                    existedPlaylist.Add(playlist);
                }

            }
            return existedPlaylist;
        }
        public void SaveData(List<LightingProfile> availableLightingProfiles, List<LightingProfilePlaylist> availablePlaylists)
        {
            lock (availableLightingProfiles)
            {
                foreach (var item in availableLightingProfiles)
                {
                    var profile = item as LightingProfile;
                    var localPath = Path.Combine(LightingProfilesCollectionFolderPath, "collection", item.Name + ".ALP");
                    JsonHelpers.WriteSimpleJson(profile, localPath);
                }
            }
            lock (availablePlaylists)
            {
                foreach (var item in availablePlaylists)
                {
                    var playlist = item as LightingProfilePlaylist;
                    var localPath = Path.Combine(LightingProfilePlaylistsCollectionFolderPath, "collection", item.Name + ".LPP");
                    JsonHelpers.WriteSimpleJson(playlist, localPath);
                }
            }

        }
    }
}
