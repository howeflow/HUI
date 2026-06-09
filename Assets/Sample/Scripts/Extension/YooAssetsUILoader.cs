using System;
using System.Collections.Generic;
using HUI;
using UnityEngine;
using YooAsset;

namespace HUI.Extension
{
    public class YooAssetsUILoader : IUILoader
    {
        private ResourcePackage package;

        private Dictionary<string, AssetHandle> map;

        public YooAssetsUILoader(string packageName = "DefaultPackage")
        {
            map = new Dictionary<string, AssetHandle>();
            package = YooAssets.GetPackage(packageName);
        }
        public void Load(string key, UICallback<GameObject> onLoadComplete)
        {
            var handle = package.LoadAssetAsync<GameObject>(key);
            handle.Completed += (h) => {
                var asset = (GameObject)handle.AssetObject;

                if (asset != null)
                {
                    map[key] = h;
                }
                onLoadComplete?.Invoke(asset);
            };
        }

        public void Release(string key)
        {
            if (map.Remove(key, out var handle))
            {
                handle.Release();
                package.TryUnloadUnusedAsset(key);
            }
        }
    }

}
