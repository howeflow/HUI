using System;
using System.Collections;
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
        private HashSet<BaseUI> pendingDestroys;
        private UIScheduler scheduler;

        public int Count => uis.Count;
        public IReadOnlyCollection<BaseUI> UIs => uis.Values;
        public UISettings Settings { get; private set; }
        public UIGroupCollection Groups { get; private set; }
        public UIQueueManager Queue { get; private set; }
        public Camera Camera { get; private set; }
        public UIEvent Events { get; private set; }

        public UIManager(GameObject root, IUILoader loader, UISettings settings)
        {
            this.loader = loader;
            this.Settings = settings;

            paths = new Dictionary<string, string>();
            uis = new Dictionary<string, BaseUI>();
            pendingDestroys = new HashSet<BaseUI>();


            scheduler = root.AddComponent<UIScheduler>();
            scheduler.Init(settings);

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
            if (!uis.TryGetValue(uiName, out var ui))
            {
                ui = LoadUI(uiName, type);
            }
            InternalShowUI(ui);
            return ui;
        }
        public BaseUI OpenUI<P>(string uiName, Type type, P parameter)
        {
            if (!uis.TryGetValue(uiName, out var ui))
            {
                ui = LoadUI(uiName, type, parameter);
            }
            else
            {
                SetParameter(ui, parameter);
            }
            InternalShowUI(ui);
            return ui;
        }
        public BaseUI OpenQueueUI(string uiName, Type type, int queueId = 0)
        {
            var command = new QueueCommand(uiName, type);
            var ui = LoadUI(uiName, type);
            Queue.Add(command, queueId);
            return ui;
        }
        public BaseUI OpenQueueUI<P>(string uiName, Type type, P parameter, int queueId = 0)
        {
            var command = new QueueCommand<P>(uiName, type, parameter);
            var ui = LoadUI(uiName, type, parameter);
            Queue.Add(command, queueId);
            return ui;
        }
        public BaseUI InsertQueueUI(int index, string uiName, Type type, int queueId = 0)
        {
            var command = new QueueCommand(uiName, type);
            var ui = LoadUI(uiName, type);
            Queue.Insert(command, index, queueId);
            return ui;
        }
        public BaseUI InsertQueueUI<P>(int index, string uiName, Type type, P parameter, int queueId = 0)
        {
            var command = new QueueCommand<P>(uiName, type, parameter);
            var ui = LoadUI(uiName, type, parameter);
            Queue.Insert(command, index, queueId);
            return ui;
        }

        public void CloseUI(string uiName, bool destroy = true)
        {
            if (!uis.TryGetValue(uiName, out var ui))
            {
                Debug.LogWarning($"[UI] {uiName} is not load.");
                return;
            }

            if (destroy)
            {
                pendingDestroys.Add(ui);
            }
            else
            {
                pendingDestroys.Remove(ui);
            }
            InternalHideUI(ui);
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

        private void InternalShowUI(BaseUI ui)
        {
            scheduler.Schedule(() => ShowUI(ui));
        }
        private void InternalHideUI(BaseUI ui)
        {
            scheduler.Schedule(() => HideUI(ui));
        }

        internal void ShowUI(BaseUI ui)
        {
            if (ui.State <= UIState.Load)
            {
                ui.pending = ShowUI;
                return;
            }
            if (ui.State == UIState.Show || ui.State == UIState.Shown || ui.State == UIState.Close)
            {
                Debug.LogWarning($"[UI] {ui.Name} cannot show, current state is {ui.State}.");
                return;
            }

            pendingDestroys.Remove(ui);
            Groups.AddToGroup(ui);
            SetState(ui, UIState.Show);
            scheduler.Show(ui.View, () => SetState(ui, UIState.Shown));
        }
        internal void HideUI(BaseUI ui)
        {
            if (ui.State <= UIState.Load)
            {
                ui.pending = HideUI;
                return;
            }

            if (ui.State == UIState.Hide)
            {
                Debug.LogWarning($"[UI] {ui.Name} is inactive.");
                return;
            }

            if (ui.State == UIState.Hidden || ui.State == UIState.Open)
            {
                if (!IsPendingDestroy(ui))
                {
                    Debug.LogWarning($"[UI] {ui.Name} is inactive.");
                    return;
                }

                TryDestroyUI(ui);
                return;
            }

            SetState(ui, UIState.Hide);
            scheduler.Hide(ui.View, () => {
                SetState(ui, UIState.Hidden);
                Groups.RemoveFromGroup(ui);
                Queue.NotifyHidden(ui);

                TryDestroyUI(ui);
            });
        }

        private bool IsPendingDestroy(BaseUI ui)
        {
            return pendingDestroys.Contains(ui);
        }
        private void TryDestroyUI(BaseUI ui)
        {
            if (!IsPendingDestroy(ui))
                return;

            pendingDestroys.Remove(ui);
            uis.Remove(ui.Name);

            SetState(ui, UIState.Close);

            loader.Release(ui.Path);
            GameObject.Destroy(ui.View.gameObject);
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
            Debug.Assert(prefab != null, $"[UI] Prefab load fail. {ui.Path}");

            var hasView = prefab.TryGetComponent<BaseView>(out var view);
            Debug.Assert(hasView, $"[UI] BaseView is not found. {ui.Path}");

            ui.View = GameObject.Instantiate(view, Groups.Template.transform, false);
            ui.View.name = ui.Name;

            SetState(ui, UIState.Open);

            ui.pending?.Invoke(ui);
            ui.pending = null;
        }
    }
}