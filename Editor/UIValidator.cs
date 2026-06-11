using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
using HUI;

namespace HUI.Editor
{


    
    public static class UIValidator
    {
        private static double lastScan;
        private static readonly Dictionary<string, string> cache = new();

        private static readonly Dictionary<string, string> scriptPaths = new();

        private static List<(string Path, Type Type)> cachedTypes;

        public static List<(string Path, Type Type)> GetUIPathTypes() {
            var now = EditorApplication.timeSinceStartup;
            if (cachedTypes != null && now - lastScan < 5) return cachedTypes;

            lastScan = now;
            cachedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => SafeGetTypes(a))
                .Where(t => t.GetCustomAttribute<UIPathAttribute>() != null)
                .Select(t => (t.GetCustomAttribute<UIPathAttribute>().Path, t))
                .ToList();
            return cachedTypes;
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly a) {
            try {
                return a.GetTypes();
            }
            catch {
                return Enumerable.Empty<Type>();
            }
        }

        public static void UpdateUIScriptPaths(string prefabFolder, string scriptFolder) {
            var hash = scriptFolder + Directory.GetLastWriteTime(scriptFolder).Ticks;

            var now = EditorApplication.timeSinceStartup;

            if (cache.TryGetValue(hash, out var cacheStr) && now - lastScan < 5) {
                foreach (var kv in cacheStr.Split('|')) {
                    var parts = kv.Split(';');
                    if (parts.Length == 2) scriptPaths[parts[0]] = parts[1];
                }

                return;
            }

            lastScan = now;

            scriptPaths.Clear();
            var set = GetViewPaths(prefabFolder).Values.Select(p => p.name).ToHashSet();
            var guids = AssetDatabase.FindAssets("t:script", new[] { scriptFolder });

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                var fileName = Path.GetFileNameWithoutExtension(path);
                if (set.Contains(fileName))
                {
                    scriptPaths[fileName] = path;
                }
                else {
                    var text = File.ReadAllText(path);
                    foreach (var name in set) {
                        if (scriptPaths.ContainsKey(name)) 
                            continue;

                        var pattern = $@"\bclass\s+{Regex.Escape(name)}\b";
                        if (Regex.IsMatch(text, pattern))
                        {
                            scriptPaths[name] = path;
                        }
                    }
                }
            }

            cache[hash] = string.Join("|", scriptPaths.Select(p => $"{p.Key};{p.Value}"));
        }

        public static string GetScriptPath(string viewName) {
            scriptPaths.TryGetValue(viewName, out var path);
            return path;
        }

        public static Dictionary<string, BaseView> GetViewPaths(string prefabFolder) {
            var views = new Dictionary<string, BaseView>();

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolder });
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var view = prefab.GetComponent<BaseView>();
                if (view != null) views[path] = view;
            }

            return views;
        }

        public class UIValidationResult
        {
            public List<string> MissingPrefabUIPaths = new List<string>();
            public List<string> UnmarkedPrefabs = new List<string>();
            public Dictionary<string, List<Type>> MultipleMapping = new Dictionary<string, List<Type>>();
        }

        public static UIValidationResult ValidateUIPath(string prefabPath) {
            if (string.IsNullOrEmpty(prefabPath))
                return new UIValidationResult();

            var result = new UIValidationResult();

            var uiPathTypes = GetUIPathTypes();
            var viewPrefabs = GetViewPaths(prefabPath);

            var typesLookup = new Dictionary<string, List<Type>>();
            var prefabLookup = viewPrefabs.ToDictionary(v => v.Key, v => v.Value.name);

            foreach (var (path, type) in uiPathTypes) {
                if (!typesLookup.TryGetValue(path, out var types)) {
                    types = new List<Type>();
                    typesLookup[path] = types;
                }

                types.Add(type);
            }

            foreach (var item in typesLookup) {
                var prefabName = item.Key;
                if (!prefabLookup.ContainsValue(prefabName)) {

                    foreach (var kv in item.Value)
                    {
                        var content = $"[UIPath(\"{prefabName}\")] -> {kv.Name}";
                        result.MissingPrefabUIPaths.Add(content);
                    }
                }
            }

            foreach (var item in prefabLookup) {
                var prefabName = item.Value;
                if (!typesLookup.ContainsKey(prefabName)) {
                    result.UnmarkedPrefabs.Add(prefabName);
                }
            }

            foreach (var kvp in typesLookup) {
                if (kvp.Value.Count > 1) {
                    var scriptPaths = kvp.Value.Select(t => GetScriptPath(t.Name)).ToArray();
                    for (int i = 0; i < kvp.Value.Count; i++) {
                        var script = scriptPaths[i] != null ? AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPaths[i]) : null;

                        if (!result.MultipleMapping.ContainsKey(kvp.Key)) {
                            result.MultipleMapping[kvp.Key] = new List<Type>();
                        }
                        result.MultipleMapping[kvp.Key].Add(kvp.Value[i]);
                    }
                }
            }

            return result;
        }
    }
}
