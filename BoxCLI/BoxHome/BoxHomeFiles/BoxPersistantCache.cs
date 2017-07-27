using System;
using System.IO;
using BoxCLI.BoxPlatform.Cache;
using BoxCLI.CommandUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BoxCLI.BoxHome.BoxHomeFiles
{
    public class BoxPersistantCache
    {
        private readonly IBoxHome _boxHome;
        private readonly string _boxHomeCacheFileName;
        public BoxPersistantCache(string fileName, IBoxHome home)
        {
            _boxHome = home;
            _boxHomeCacheFileName = fileName;
        }

        public BoxCachedToken RetrieveTokenFromCache()
        {
            var path = GetBoxCacheFilePath();
            if (new FileInfo(path).Length == 0)
            {
                return new BoxCachedToken();
            }
            else
            {
                using (var fs = File.OpenText(path))
                {
                    var serializer = new JsonSerializer();
                    return (BoxCachedToken)serializer.Deserialize(fs, typeof(BoxCachedToken));
                }
            }
        }

        public void SetTokenInCache(BoxCachedToken token)
        {
            var path = GetBoxCacheFilePath();
            var serializer = new JsonSerializer();
            using (StreamWriter file = File.CreateText(path))
            {
                serializer.Serialize(file, token);
            }
        }

        public BoxCachedToken BustCache()
        {
            var token = this._boxHome.GetBoxCache().RetrieveTokenFromCache();
            RemoveBoxCacheFile();
            CreateBoxCacheFile();
            return token;
        }

        public void RemoveBoxCacheFile()
        {
            var path = GetBoxCacheFilePath();
            File.Delete(path);
        }

        private string GetBoxCacheFilePath()
        {
            return CreateBoxCacheFile();
        }

        private string CreateBoxCacheFile()
        {
            var boxHome = _boxHome.GetBoxHomeDirectoryPath();
            var path = Path.Combine(boxHome, _boxHomeCacheFileName);
            if (!CheckIfBoxCacheFileExists())
            {
                using (var fs = File.Create(path))
                {
                    return path;
                }
            }
            else
            {
                return path;
            }
        }

        private bool CheckIfBoxCacheFileExists()
        {
            var boxHome = _boxHome.GetBoxHomeDirectoryPath();
            var path = Path.Combine(boxHome, _boxHomeCacheFileName);
            try
            {
                return File.Exists(path);
            }
            catch (Exception e)
            {
                Reporter.WriteError(e.Message);
                return false;
            }
        }
    }
}