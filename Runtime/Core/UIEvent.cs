namespace HUI
{
    public delegate void UICallback();
    public delegate void UICallback<T>(T arg);

    public class UIEvent
    {
        public UICallback<BaseUI> OnOpen;
        public UICallback<BaseUI> OnShow;
        public UICallback<BaseUI> OnShown;
        public UICallback<BaseUI> OnHide;
        public UICallback<BaseUI> OnHidden;
        public UICallback<BaseUI> OnClose;
        public UICallback<BaseUI> OnChanged;
        public void Notify(BaseUI ui) {
            var state = ui.State;
            switch (state) {
                case UIState.Open:
                    OnOpen?.Invoke(ui);
                    break;
                case UIState.Show:
                    OnShow?.Invoke(ui);
                    break;
                case UIState.Shown:
                    OnShown?.Invoke(ui);
                    break;
                case UIState.Hide:
                    OnHide?.Invoke(ui);
                    break;
                case UIState.Hidden:
                    OnHidden?.Invoke(ui);
                    break;
                case UIState.Close:
                    OnClose?.Invoke(ui);
                    break;
            }
            OnChanged?.Invoke(ui);
        }

        public void Clear() {
            OnOpen = null;
            OnShow = null;
            OnShown = null;
            OnHide = null;
            OnHidden = null;
            OnClose = null;
            OnChanged = null;
        }
    }
}