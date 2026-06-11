using System.Collections.Generic;
using UnityEngine;

namespace HUI
{
    [CreateAssetMenu(fileName = "UISettings", menuName = "HUI/Create UISettings Assets")]
    public class UISettings : ScriptableObject
    {
        public const string PATH_SETTINGS = "UISettings";
        public const string ROOT_NAME = "UIRoot";


        public string prefabPath = "Assets";
        public string scriptPath = "Assets";
        
        public bool skipAnimation = false;
        public bool dontDestroyOnLoad = true;

        public List<UIGroupInfo> groups = new() {
            new UIGroupInfo() { name = "Default", depth = 0 }
        };

      
        public static UISettings Load() {
            var settings = Resources.Load<UISettings>(PATH_SETTINGS);
            return settings;
        }

        public void Reset()
        {
            dontDestroyOnLoad = true;
            prefabPath = "Assets";
            scriptPath = "Assets";
            skipAnimation = false;
            groups = new() { new UIGroupInfo() { name = "Default", depth = 0 } };
        }
    }
}