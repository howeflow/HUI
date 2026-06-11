using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HUI
{
    public class UIContainer
    {
        public BaseUI Parent { get; private set; }
        public Dictionary<string, BaseComponent> Childs { get; }
        public event UICallback<BaseComponent> OnChanged;

        public UIContainer(BaseUI parent)
        {
            this.Parent = parent;
            Childs = new Dictionary<string, BaseComponent>();
        }

        private void ChangeState(BaseComponent ui,ComponentState state)
        {
            if (ui.State == state)
            {
                return;
            }

            ui.UpdateState(state);
            OnChanged?.Invoke(ui);
        }
        private void SetParameter<T>(BaseComponent ui,T value)
        {
            if(ui is BaseComponent<T> target)
            {
                target.Parameter = value;
            }
        }


        public bool Contains(string name)
        {
            return Childs.ContainsKey(name);
        }
        public bool Contains<T>() where T : BaseComponent
        {
            return Contains(typeof(T).Name);
        }

        public T Get<T>() where T : BaseComponent
        {
            var name = typeof(T).Name;
            return Childs.TryGetValue(name, out var ui) ? ui as T : null;
        }
        public BaseComponent Get(string name)
        {
            return Childs.TryGetValue(name, out var ui) ? ui : null;
        }

        public BaseComponent Add<T>(BaseView view) where T : BaseComponent
        {
            return Add(typeof(T).Name, typeof(T),view);
        }
        public BaseComponent Add(string name, Type type, BaseView view)
        {
            if(Childs.TryGetValue(name,out var ui))
            {
                return ui;
            }

            ui = Activator.CreateInstance(type) as BaseComponent;
            ui.Parent = Parent;
            ui.View = view;
            ui.Name = name;

            Childs[name] = ui;

            ChangeState(ui, ComponentState.Added);

            return ui;
        }
        public BaseComponent Add<T, P>(BaseView view, P parameter) where T : BaseComponent<P>
        {
            var ui = Add<T>(view);
            SetParameter(ui, parameter);
            return ui;
        }
        public BaseComponent Add<P>(string name,Type type, BaseView view, P parameter)
        {
            var ui = Add(name,type, view);
            SetParameter(ui, parameter);
            return ui;
        }
 
        public bool Remove(string name)
        {
            if (Childs.TryGetValue(name, out var ui))
            {
                ChangeState(ui, ComponentState.Removed);
                Childs.Remove(name);
                return true;
            }
            return false;
        }
        public bool Remove<T>() where T : BaseComponent
        {
            return Remove(typeof(T).Name);
        }

        public void Show(string name)
        {
            if (Childs.TryGetValue(name, out var ui))
            {
                ChangeState(ui, ComponentState.Show);
            }
        }
        public void Show<T>() where T : BaseComponent
        {
            Show(typeof(T).Name);
        }

        public void Hide(string name)
        {
            if (Childs.TryGetValue(name, out var ui))
            {
                ChangeState(ui, ComponentState.Hide);
            }
        }
        public void Hide<T>() where T : BaseComponent
        {
            Hide(typeof(T).Name);
        }

        public void Refresh(string name)
        {
            if (Childs.TryGetValue(name, out var ui))
            {
                ui.Refresh();
            }
        }
        public void Refresh<T>() where T : BaseComponent
        {
            Refresh(typeof(T).Name);
        }

        public void RemoveAll()
        {
            foreach (var ui in Childs.Values)
            {
                ChangeState(ui, ComponentState.Removed);
            }
            Childs.Clear();
        }
        public void ShowAll()
        {
            foreach (var ui in Childs.Values)
            {
                ChangeState(ui, ComponentState.Show);
            }
        }
        public void HideAll()
        {
            foreach (var ui in Childs.Values)
            {
                ChangeState(ui, ComponentState.Hide);
            }
        }
        public void RefreshAll()
        {
            foreach (var ui in Childs.Values)
            {
                ui.Refresh();
            }
        }
    }
}