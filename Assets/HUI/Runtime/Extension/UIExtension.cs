namespace HUI
{
    public static class UIExtension
    {
        public static void Close(this BaseUI ui)
        {
            UIKit.CloseUI(ui.Name, true);
        }
        public static void Close(this BaseUI ui, bool destroy)
        {
            UIKit.CloseUI(ui.Name, destroy);
        }


		public static BaseUI OnOpen(this BaseUI ui, UICallback<BaseUI> callback)
        {
            ui.Events.OnOpen = callback;
            return ui;
        }
        public static BaseUI OnShow(this BaseUI ui, UICallback<BaseUI> callback)
        {
            ui.Events.OnShow = callback;
            return ui;
        }
        public static BaseUI OnShown(this BaseUI ui, UICallback<BaseUI> callback)
        {
            ui.Events.OnShown = callback;
            return ui;
        }
        public static BaseUI OnHide(this BaseUI ui, UICallback<BaseUI> callback)
        {
            ui.Events.OnHide = callback;
            return ui;
        }
        public static BaseUI OnHidden(this BaseUI ui, UICallback<BaseUI> callback)
        {
            ui.Events.OnHidden = callback;
            return ui;
        }
        public static BaseUI OnClose(this BaseUI ui, UICallback<BaseUI> callback)
        {
            ui.Events.OnClose = callback;
            return ui;
        }
        public static BaseUI OnChanged(this BaseUI ui, UICallback<BaseUI> callback)
        {
            ui.Events.OnChanged = callback;
            return ui;
        }

		public static T OnOpen<T>(this T ui, UICallback<T> callback) where T : BaseUI
		{
			ui.Events.OnOpen = u => callback((T)u);
			return ui;
		}
		public static T OnShow<T>(this T ui, UICallback<T> callback) where T : BaseUI
		{
			ui.Events.OnShow = u => callback((T)u);
			return ui;
		}
		public static T OnShown<T>(this T ui, UICallback<T> callback) where T : BaseUI
		{
			ui.Events.OnShown = u => callback((T)u);
			return ui;
		}
		public static T OnHide<T>(this T ui, UICallback<T> callback) where T : BaseUI
		{
			ui.Events.OnHide = u => callback((T)u);
			return ui;
		}
		public static T OnHidden<T>(this T ui, UICallback<T> callback) where T : BaseUI
		{
			ui.Events.OnHidden = u => callback((T)u);
			return ui;
		}
		public static T OnClose<T>(this T ui, UICallback<T> callback) where T : BaseUI
		{
			ui.Events.OnClose = u => callback((T)u);
			return ui;
		}
		public static T OnChanged<T>(this T ui, UICallback<T> callback) where T : BaseUI
		{
			ui.Events.OnChanged = u => callback((T)u);
			return ui;
		}


		public static BaseUI OnOpen(this BaseUI ui, UICallback callback)
		{
			ui.Events.OnOpen = u => callback();
			return ui;
		}
		public static BaseUI OnShow(this BaseUI ui, UICallback callback)
		{
			ui.Events.OnShow = u => callback();
			return ui;
		}
		public static BaseUI OnShown(this BaseUI ui, UICallback callback)
		{
			ui.Events.OnShown = u => callback();
			return ui;
		}
		public static BaseUI OnHide(this BaseUI ui, UICallback callback)
		{
			ui.Events.OnHide = u => callback();
			return ui;
		}
		public static BaseUI OnHidden(this BaseUI ui, UICallback callback)
		{
			ui.Events.OnHidden = u => callback();
			return ui;
		}
		public static BaseUI OnClose(this BaseUI ui, UICallback callback)
		{
			ui.Events.OnClose = u => callback();
			return ui;
		}
		public static BaseUI OnChanged(this BaseUI ui, UICallback callback)
		{
			ui.Events.OnChanged = u => callback();
			return ui;
		}

	}
}