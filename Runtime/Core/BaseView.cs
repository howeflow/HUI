using System;
using System.Collections;
using UnityEngine;

namespace HUI
{
    public enum Priority
    {
        Lowest = -2000,
        Low = -1000,
        Normal = 0,
        Middle = 1000,
        High = 2000,
        Highest = 3000
    }
    
    [Serializable]
    public struct ViewSettings
    {
        public int group;
        public Priority priority;
    }

    [DisallowMultipleComponent]
    public class BaseView : MonoBehaviour
    {
        [SerializeField]private ViewSettings setting;
        private Coroutine coroutine;
        public IUIAnimation Anim { get; set; }
        public ViewSettings Setting { get => setting; set => setting = value; }

        public virtual void SetVisible(bool show) {
            gameObject.SetActive(show);
        }

        public void ShowAnimation(UICallback callback = null) {
            ValidateAnimation();
            StopCurrentAnimation();
            SetVisible(true);
            coroutine = StartCoroutine(ExecuteShowAnimation(callback));
        }

        public void HideAnimation(UICallback callback = null) {
            ValidateAnimation();
            StopCurrentAnimation();
            coroutine = StartCoroutine(ExecuteHideAnimation(callback));
        }
        private IEnumerator ExecuteShowAnimation(UICallback callback) {
            yield return StartCoroutine(Anim.Show());
            StopCurrentAnimation();
            callback?.Invoke();
        }
        private IEnumerator ExecuteHideAnimation(UICallback callback) {
            yield return StartCoroutine(Anim.Hide());
            StopCurrentAnimation();
            SetVisible(false);
            callback?.Invoke();
        }


        private void StopCurrentAnimation() {
            if (coroutine == null)
                return;
            StopCoroutine(coroutine);
            coroutine = null;
        }
        private void ValidateAnimation() {
            if (Anim != null)
                return;

            Anim ??= GetComponent<IUIAnimation>() ?? IUIAnimation.Default;
        }
    }
}

