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
            var type = typeof(T);
            return LoadUI(type.Name, type) as T;
        }
        public static BaseUI LoadUI(string uiName, Type type)
        {
            return Manager.LoadUI(uiName, type);
        }

        public static T LoadUI<T,P>(P parameter) where T : BaseUI
        {
            var type = typeof(T);
            return LoadUI(type.Name, type, parameter) as T;
        }
        public static BaseUI LoadUI<P>(string uiName, Type type, P parameter)
        {
            return Manager.LoadUI(uiName, type, parameter);
        }

        public static T OpenUI<T>() where T : BaseUI
        {
            return OpenUI(typeof(T)) as T;
        }
        public static BaseUI OpenUI(Type type)
        {
            return OpenUI(type.Name, type);
        }
        public static BaseUI OpenUI(string uiName, Type type)
        {
            return Manager.OpenUI(uiName,type);
        }

        public static T OpenUI<T,P>(P parameter) where T : BaseUI<P>
        {
            return OpenUI(typeof(T),parameter) as T;
        }
        public static BaseUI OpenUI<P>(Type type, P parameter)
        {
            return OpenUI(type.Name, type,parameter);
        }
        public static BaseUI OpenUI<P>(string uiName, Type type,P parameter)
        {
            return Manager.OpenUI(uiName, type, parameter);
        }

        public static T OpenQueueUI<T>(int queueId = 0) where T : BaseUI
        {
            return OpenQueueUI(typeof(T), queueId) as T;
        }
        public static BaseUI OpenQueueUI(Type type, int queueId = 0)
        {
            return OpenQueueUI(type.Name, type, queueId);
        }
        public static BaseUI OpenQueueUI(string uiName, Type type, int queueId = 0)
        {
            return Manager.OpenQueueUI(uiName,type,queueId);
        }

        public static T OpenQueueUI<T, P>(P parameter, int queueId = 0) where T : BaseUI<P>
        {
            return OpenQueueUI(typeof(T), parameter, queueId) as T;
        }
        public static BaseUI OpenQueueUI<P>(Type type, P parameter, int queueId = 0)
        {
            return OpenQueueUI(type.Name, type, parameter, queueId);
        }
        public static BaseUI OpenQueueUI<P>(string uiName, Type type, P parameter, int queueId = 0)
        {
            return Manager.OpenQueueUI(uiName,type,parameter,queueId);
        }

        public static T InsertQueueUI<T>(int index, int queueId = 0) where T : BaseUI
        {
            return InsertQueueUI(index, typeof(T), queueId) as T;
        }
        public static BaseUI InsertQueueUI(int index, Type type, int queueId = 0)
        {
            return InsertQueueUI(index, type.Name, type, queueId);
        }
        public static BaseUI InsertQueueUI(int index, string uiName, Type type, int queueId = 0)
        {
            return Manager.InsertQueueUI(index, uiName, type, queueId);
        }

        public static T InsertQueueUI<T, P>(int index, P parameter, int queueId = 0) where T : BaseUI<P>
        {
            return InsertQueueUI(index, typeof(T), parameter, queueId) as T;
        }
        public static BaseUI InsertQueueUI<P>(int index, Type type, P parameter, int queueId = 0)
        {
            return InsertQueueUI(index, type.Name, type, parameter, queueId);
        }
        public static BaseUI InsertQueueUI<P>(int index, string uiName, Type type, P parameter, int queueId = 0)
        {
            return Manager.InsertQueueUI(index, uiName, type, parameter, queueId);
        }

        public static void CloseUI<T>(bool destroy = true) where T : BaseUI
        {
            CloseUI(typeof(T), destroy);
        }
        public static void CloseUI(Type type, bool destroy = true)
        {
            CloseUI(type.Name, destroy);
        }
        public static void CloseUI(string uiName, bool destroy = true)
        {
            Manager.CloseUI(uiName, destroy);
        }
    }
}