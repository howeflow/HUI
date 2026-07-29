using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HUI
{
    public static class UIKit
    {
        public static event UICallback Initialized;
        public static bool IsInitialized { get; private set; }

        private static UIManager manager;
        public static UIManager Manager
        {
            get
            {
                if (!IsInitialized) 
                    throw new InvalidOperationException("[UI] UIKit is not initialized. Please call UIKit.Initialize() first.");
                return manager;
            }
        }

        public static void Initialize(IUILoader loader, GameObject root = null)
        {
            if (IsInitialized) 
                return;

            var settings = UISettings.Load();

            Debug.Assert(loader != null, "[UI] ui loader is null.");
            Debug.Assert(settings != null, "[UI] ui settings is null.");


            if (root != null)
            {
                Initialize(loader, settings, root);
                return;
            }

            var rootName = UISettings.ROOT_NAME;
            var async = Resources.LoadAsync<GameObject>(rootName);
            async.completed += a => {
                var prefab = async.asset as GameObject;
                var root = Object.Instantiate(prefab);
                root.name = rootName;
                Initialize(loader, settings, root);
            };
        }

        private static void Initialize(IUILoader loader,UISettings settings, GameObject root)
        {
            if (settings.dontDestroyOnLoad)
            {
                Object.DontDestroyOnLoad(root);
            }

            manager = new UIManager(root, loader, settings);

            IsInitialized = true;

            Initialized?.Invoke();
            Initialized = null;
        }


        public static T GetUI<T>() where T : BaseUI
        {
            var ui = GetUI(typeof(T));
            return ui as T;
        }
        public static BaseUI GetUI(Type type)
        {
            var name = type.Name;
            return GetUI(name);
        }
        public static BaseUI GetUI(string uiName)
        {
            return Manager.GetUI(uiName);
        }

        public static T LoadUI<T>() where T : BaseUI
        {
            return LoadUI(UIKey.Create<T>()) as T;
        }
        public static BaseUI LoadUI(UIKey key)
        {
            return Manager.LoadUI(key);
        }

        public static T LoadUI<T,P>(P parameter) where T : BaseUI<P>
        {
            return LoadUI(UIKey.Create<T>(), parameter) as T;
        }
        public static BaseUI LoadUI<P>(UIKey key, P parameter)
        {
            return Manager.LoadUI(key, parameter);
        }

        public static T OpenUI<T>() where T : BaseUI
        {
            return OpenUI(UIKey.Create<T>()) as T;
        }
        public static BaseUI OpenUI(UIKey key)
        {
            return Manager.OpenUI(key);
        }

        public static T OpenUI<T,P>(P parameter) where T : BaseUI<P>
        {
            return OpenUI(UIKey.Create<T>(), parameter) as T;
        }
        public static BaseUI OpenUI<P>(UIKey key, P parameter)
        {
            return Manager.OpenUI(key, parameter);
        }

        public static T OpenQueueUI<T>(int queueId = 0, bool first = false) where T : BaseUI
        {
            return OpenQueueUI(UIKey.Create<T>(), queueId, first) as T;
        }
        public static BaseUI OpenQueueUI(UIKey key, int queueId = 0, bool first = false)
        {
            return Manager.OpenQueueUI(key, queueId, first);
        }

        public static T OpenQueueUI<T, P>(P parameter, int queueId = 0, bool first = false) where T : BaseUI<P>
        {
            return OpenQueueUI(UIKey.Create<T>(), parameter, queueId, first) as T;
        }
        public static BaseUI OpenQueueUI<P>(UIKey key, P parameter, int queueId = 0, bool first = false)
        {
            return Manager.OpenQueueUI(key, parameter, queueId, first);
        }

        public static void CloseUI<T>(bool destroy = true) where T : BaseUI
        {
            CloseUI(UIKey.Create<T>(), destroy);
        }
        public static void CloseUI(UIKey key, bool destroy = true)
        {
            CloseUI(key.Name, destroy);
        }
        public static void CloseUI(string uiName, bool destroy = true)
        {
            Manager.CloseUI(uiName, destroy);
        }
    }
}
