using UnityEngine;
using System;
using System.Collections.Generic;

namespace HUI
{
    public interface IUILoader
    {
        void Load(string key, UICallback<GameObject> onLoadComplete);
        void Release(string key);
    }

    public class DefaultUILoader : IUILoader
    {
        private Dictionary<string, GameObject> map;

        private string path;

        private string GetKey(string key)
        {
            return path + key;
        }

        public DefaultUILoader() {
            map = new Dictionary<string, GameObject>();
            var prefabPath = UISettings.Load().prefabPath;

            var prePath = "Resources/";

            var startIndex= prefabPath.IndexOf(prePath);

            if (startIndex >= 0)
            {
                path = prefabPath.Substring(startIndex + prePath.Length);
            }
            if (!string.IsNullOrEmpty(path) && !path.EndsWith("/"))
            {
                path += "/";
            }
        }
        public void Load(string key, UICallback<GameObject> onLoadComplete) {
            key = GetKey(key);
            var async = Resources.LoadAsync<GameObject>(key);
            async.completed += (s) => {
                var asset = (GameObject)async.asset;

                if (asset != null) {
                    map[key] = asset;
                }

                onLoadComplete?.Invoke(asset);
            };
        }

        public void Release(string key) {
            key = GetKey(key);
            map.Remove(key, out var prefab);
        }
    }
}


