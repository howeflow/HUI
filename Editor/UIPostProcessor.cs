using UnityEditor;

namespace HUI.Editor
{
    public class UIPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromPath)
        {
            UpdatePath();
        }

        [InitializeOnLoadMethod]
        private static void SubscribeToReload()
        {
            UpdatePath();
        }

        public static void UpdatePath()
        {
            var setting = UISettings.Load();

            if (setting != null)
            {
                UIValidator.UpdateUIScriptPaths(setting.prefabPath, setting.scriptPath);
            }
        }
    }
}