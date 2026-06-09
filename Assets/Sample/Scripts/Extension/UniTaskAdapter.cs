using Cysharp.Threading.Tasks;
using HUI;
using UnityEngine;

namespace HUI.Extension
{
    public static class UniTaskAdapter
    {
        public static UniTask WaitOpenAsync(this BaseUI ui)
        {
            return WaitChangedToAsync(ui, UIState.Open);
        }
        public static UniTask WaitShowAsync(this BaseUI ui)
        {
            return WaitChangedToAsync(ui, UIState.Show);
        }
        public static UniTask WaitShownAsync(this BaseUI ui)
        {
            return WaitChangedToAsync(ui, UIState.Shown);
        }
        public static UniTask WaitHideAsync(this BaseUI ui)
        {
            return WaitChangedToAsync(ui, UIState.Hide);
        }
        public static UniTask WaitHiddenAsync(this BaseUI ui)
        {
            return WaitChangedToAsync(ui, UIState.Hidden);
        }
        public static UniTask WaitCloseAsync(this BaseUI ui)
        {
            return WaitChangedToAsync(ui, UIState.Close);
        }
        public static UniTask WaitChangedToAsync(this BaseUI ui, UIState state)
        {
            if (ui.State == state)
                return UniTask.CompletedTask;

            var tcs = new UniTaskCompletionSource();

            ui.Events.OnChanged += Trigger;

            void Trigger(BaseUI h)
            {
                if (h.State == state)
                {
                    h.Events.OnChanged -= Trigger;
                    tcs.TrySetResult();
                }
            }

            return tcs.Task;
        }
    }
}

