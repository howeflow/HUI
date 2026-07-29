using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HUI
{
    public class UIManager
    {
        private IUILoader loader;
        private Dictionary<string, string> paths;
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

            paths = new Dictionary<string, string>();
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

        public BaseUI LoadUI(string uiName, Type type)
        {
            var ui = Generate(uiName, type);
            LoadView(ui);
            return ui;
        }
        public BaseUI LoadUI<P>(string uiName, Type type, P parameter)
        {
            var ui = Generate(uiName, type);
            SetParameter(ui, parameter);
            LoadView(ui);
            return ui;
        }

        public BaseUI OpenUI(string uiName, Type type)
        {
            var ui = LoadUI(uiName, type);
            scheduler.Request(ui, UIIntent.Show);
            return ui;
        }
        public BaseUI OpenUI<P>(string uiName, Type type, P parameter)
        {
            var ui = LoadUI(uiName, type, parameter);
            scheduler.Request(ui, UIIntent.Show);
            return ui;
        }
        public BaseUI OpenQueueUI(string uiName, Type type, int queueId = 0, bool first = false)
        {
            var command = new QueueCommand(uiName, type);
            var ui = Generate(uiName, type);
            Queue.Add(command, queueId, first);
            return ui;
        }
        public BaseUI OpenQueueUI<P>(string uiName, Type type, P parameter, int queueId = 0, bool first = false)
        {
            var command = new QueueCommand<P>(uiName, type, parameter);
            var ui = Generate(uiName, type);
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

        internal BaseUI OpenUIFromQueue(string uiName, Type type, long entryId)
        {
            var ui = Generate(uiName, type);
            scheduler.RequestQueueShow(ui, entryId);
            LoadView(ui);
            return ui;
        }

        internal BaseUI OpenUIFromQueue<P>(string uiName, Type type, P parameter, long entryId)
        {
            var ui = Generate(uiName, type);
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

        private string GetPath(string uiName, Type type)
        {
            var key = uiName + type.Name;

            if (paths.TryGetValue(key, out var path))
            {
                return path;
            }

            var attribute = type.GetCustomAttribute<UIPathAttribute>();
            var typeName = type.Name;

            path = uiName != typeName ? uiName : attribute?.Path ?? uiName;

            paths[key] = path;
            return path;
        }
        private BaseUI Generate(string uiName, Type type)
        {
            Debug.Assert(typeof(BaseUI).IsAssignableFrom(type), $"[UI] {type} must inherit from BaseUI.");

            if (uis.TryGetValue(uiName, out var ui))
                return ui;

            ui = Activator.CreateInstance(type) as BaseUI;
            ui.Name = uiName;
            ui.Path = GetPath(uiName, type);
            uis[uiName] = ui;
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
