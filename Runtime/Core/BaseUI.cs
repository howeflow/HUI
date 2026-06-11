using System;

namespace HUI
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UIPathAttribute : Attribute
    {
        public string Path { get; }
        public UIPathAttribute(string path) => Path = path;
    }

    public enum UIState
    {
        None,
        Load,
        Open,
        Show,
        Shown,
        Hide,
        Hidden,
        Close
    }

    public abstract class BaseUI
    {
        internal UICallback<BaseUI> pending;
        public string Name { get; internal set; }
        public string Path { get; internal set; }
        public UIState State { get; private set; }
        public BaseView View { get; internal set; }
        public UIGroup Group { get; internal set; }


        private UIEvent events;
        public UIEvent Events
        {
            get
            {
                events ??= new UIEvent();
                return events;
            }
        }


        private UIContainer container;
        public UIContainer Container
        {
            get
            {
                container ??= new UIContainer(this);
                return container;
            }
        }

        internal void UpdateState(UIState state) {
            State = state;
            switch (state) {
                case UIState.Open:
                    OnOpen();
                    break;
                case UIState.Show:
                    View.SetVisible(true);
                    OnShow();
                    break;
                case UIState.Shown:
                    OnShown();
                    break;
                case UIState.Hide:
                    OnHide();
                    break;
                case UIState.Hidden:
                    View.SetVisible(false);
                    OnHidden();
                    break;
                case UIState.Close:
                    OnClose();
                    break;
            }
            events?.Notify(this);
        }

        public void Refresh() {
            OnRefresh();
        }

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnShow() { }
        protected virtual void OnShown() { }
        protected virtual void OnHide() { }
        protected virtual void OnHidden() { }
        protected virtual void OnRefresh() { }
    }

    public abstract class BaseUI<P> : BaseUI
    {
        public P Parameter { get; set; }
    }

    public abstract class BaseViewUI<V> : BaseUI where V : BaseView
    {
        public new V View => (V)base.View;
    }

    public abstract class BaseViewUI<V, P> : BaseUI<P> where V : BaseView
    {
        public new V View => (V)base.View;
    }

}