using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HUI
{
    public interface IUIAnimation
    {
        public static readonly IUIAnimation Default = new DefaultUIAnimation();
        IEnumerator Show();
        IEnumerator Hide();

        class DefaultUIAnimation : IUIAnimation
        {
            public IEnumerator Show() { yield break; }
            public IEnumerator Hide() { yield break; }
        }
    }


    public class UIScheduler : MonoBehaviour
    {
        private UISettings settings;

        private Queue<UICallback> commands;
        private bool running = false;


        internal void Init(UISettings settings)
        {
            this.settings = settings;

            commands = new Queue<UICallback>();
        }


        public void Schedule(UICallback command)
        {
            commands.Enqueue(command);
            if (!running)
            {
                StartCoroutine(Run());
            }
        }

        private IEnumerator Run()
        {
            running = true;
            yield return new WaitForEndOfFrame();

            while (commands.Count > 0)
            {
                var command = commands.Dequeue();
                command?.Invoke();
            }

            running = false;
        }

        public void Show(BaseView view, UICallback callback = null)
        {
            if (settings.skipAnimation)
            {
                callback?.Invoke();
                return;
            }

            view.ShowAnimation(callback);
        }

        public void Hide(BaseView baseView, UICallback callback = null)
        {
            if (settings.skipAnimation)
            {
                callback?.Invoke();
                return;
            }

            baseView.HideAnimation(callback);
        }
    }
}
