using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HUI
{
    public class UIManager
    {
        private IUILoader loader;
        private Dictionary<UIKey, string> paths;
        private Dictionary<string, BaseUI> uis;
        private UIScheduler scheduler;

        public int Count => uis.Count;
        public IReadOnlyCollection<BaseUI> UIs => uis.Values;
        public UISettings Settings { get; private set; }
        public UIGroupCollection Groups { get; private set; }
        public UIQueueManager Queue { get; private set; }
        public Camera Camera { get; private set; }
        public UIEvent Events { get; private set; }

        internal UIManager(GameObject root, IUILoader loader, UISettings settings)
        {
            this.loader = loader;
            this.Settings = settings;

            paths = new Dictionary<UIKey, string>();
            uis = new Dictionary<string, BaseUI>();


            scheduler = root.AddComponent<UIScheduler>();
            scheduler.Init(this, settings);

            Groups = new UIGroupCollection(settings, root);
            Queue = new UIQueueManager(this);

            Camera = root.GetComponentInChildren<Camera>();

            Events = new UIEvent();
        }

        public BaseUI GetUI(string uiName)
        {
            return uis.GetValueOrDefault(uiName);
        }
        public BaseUI GetUI(UIKey key)
        {
            return GetUI(key.Name);
        }

        public BaseUI LoadUI(UIKey key)
        {
            var ui = Generate(key);
            LoadView(ui);
            return ui;
        }
        public BaseUI LoadUI<P>(UIKey key, P parameter)
        {
            var ui = Generate(key);
            SetParameter(ui, parameter);
            LoadView(ui);
            return ui;
        }

        public BaseUI OpenUI(UIKey key)
        {
            var ui = LoadUI(key);
            scheduler.Request(ui, UIIntent.Show);
            return ui;
        }
        public BaseUI OpenUI<P>(UIKey key, P parameter)
        {
            var ui = LoadUI(key, parameter);
            scheduler.Request(ui, UIIntent.Show);
            return ui;
        }
        public BaseUI OpenQueueUI(UIKey key, int queueId = 0, bool first = false)
        {
            var command = new QueueCommand(key);
            var ui = Generate(key);
            Queue.Add(command, queueId, first);
            return ui;
        }
        public BaseUI OpenQueueUI<P>(UIKey key, P parameter, int queueId = 0, bool first = false)
        {
            var command = new QueueCommand<P>(key, parameter);
            var ui = Generate(key);
            Queue.Add(command, queueId, first);
            return ui;
        }

        public void CloseUI(string uiName, bool destroy = true)
        {
            if (!uis.TryGetValue(uiName, out var ui))
            {
                Debug.LogWarning($"[UI] {uiName} is not load.");
                return;
            }

            scheduler.Request(ui, destroy ? UIIntent.Close : UIIntent.Hide);
        }
        public void CloseAllUI(Predicate<BaseUI> condition, bool destroy = true)
        {
            if (uis.Count <= 0) return;
            var keys = new List<string>(uis.Keys);
            foreach (var item in keys)
            {
                if (condition == null || condition(uis[item]))
                {
                    CloseUI(item, destroy);
                }
            }
        }

        internal BaseUI OpenUIFromQueue(UIKey key, long entryId)
        {
            var ui = Generate(key);
            scheduler.RequestQueueShow(ui, entryId);
            LoadView(ui);
            return ui;
        }

        internal BaseUI OpenUIFromQueue<P>(UIKey key, P parameter, long entryId)
        {
            var ui = Generate(key);
            SetParameter(ui, parameter);
            scheduler.RequestQueueShow(ui, entryId);
            LoadView(ui);
            return ui;
        }

        internal void DestroyUI(BaseUI ui)
        {
            uis.Remove(ui.Name);

            var view = ui.View;
            if (ui.Group != null)
            {
                Groups.RemoveFromGroup(ui);
            }

            SetState(ui, UIState.Close);
            ui.View = null;
            loader.Release(ui.Path);
            GameObject.Destroy(view.gameObject);
        }

        internal void SetState(BaseUI ui, UIState state)
        {
            ui.UpdateState(state);
            Events.Notify(ui);
        }
        internal void SetParameter<P>(BaseUI ui, P value)
        {
            if(ui is BaseUI<P> target)
            {
                target.Parameter = value;
            }
        }

        private string GetPath(UIKey key)
        {
            if (paths.TryGetValue(key, out var path))
            {
                return path;
            }

            var attribute = key.Type.GetCustomAttribute<UIPathAttribute>();
            var typeName = key.Type.Name;

            path = key.Name != typeName ? key.Name : attribute?.Path ?? key.Name;

            paths[key] = path;
            return path;
        }
        private BaseUI Generate(UIKey key)
        {
            Debug.Assert(typeof(BaseUI).IsAssignableFrom(key.Type), $"[UI] {key.Type} must inherit from BaseUI.");

            if (uis.TryGetValue(key.Name, out var ui))
                return ui;

            ui = Activator.CreateInstance(key.Type) as BaseUI;
            ui.Name = key.Name;
            ui.Path = GetPath(key);
            uis[key.Name] = ui;
            return ui;
        }

        private void LoadView(BaseUI ui)
        {
            if (ui.State == UIState.None)
            {
                SetState(ui, UIState.Load);
                var address = ui.Path;
                loader.Load(address, prefab => OnLoadComplete(prefab, ui));
            }
        }
        private void OnLoadComplete(GameObject prefab, BaseUI ui)
        {
            if (prefab == null)
            {
                HandleLoadFailure(ui, new InvalidOperationException($"[UI] Prefab load fail. {ui.Path}"));
                return;
            }
            if (!prefab.TryGetComponent<BaseView>(out var view))
            {
                HandleLoadFailure(ui, new InvalidOperationException($"[UI] BaseView is not found. {ui.Path}"));
                return;
            }

            ui.View = GameObject.Instantiate(view, Groups.Template.transform, false);
            ui.View.name = ui.Name;
            SetState(ui, UIState.Open);
            scheduler.NotifyReady(ui);
        }

        private void HandleLoadFailure(BaseUI ui, Exception exception)
        {
            Debug.LogException(exception);
            uis.Remove(ui.Name);
            loader.Release(ui.Path);
            scheduler.NotifyLoadFailed(ui);
        }
    }
}
