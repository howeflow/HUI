using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HUI
{
    public enum ComponentState
    {
        None,
        Added,
        Show,
        Hide,
        Removed,
    }

    public abstract class BaseComponent
    {
        public string Name { get; internal set; }
        public BaseUI Parent { get; internal set; }
        public BaseView View { get; internal set; }
        public ComponentState State { get; private set; }

        public void UpdateState(ComponentState state)
        {
            State = state;
            switch (state)
            {
                case ComponentState.Added:
                    OnAdded();
                    break;
                case ComponentState.Removed:
                    OnRemoved();
                    break;
                case ComponentState.Show:
                    View.SetVisible(true);
                    OnShow();
                    break;
                case ComponentState.Hide:
                    View.SetVisible(false);
                    OnHide();
                    break;
            }
        }

        public void Refresh()
        {
            OnRefresh();
        }
        protected virtual void OnRefresh() { }
        protected virtual void OnAdded() { }
        protected virtual void OnRemoved() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
    public abstract class BaseComponent<P> : BaseComponent
    {
        public P Parameter { get; set; }
    }
    public abstract class BaseViewComponent<V> : BaseComponent where V : BaseView
    {
        public new V View => (V)base.View;
    }
    public abstract class BaseViewComponent<V, P> : BaseComponent<P> where V : BaseView
    {
        public new V View => (V)base.View;
    }
}