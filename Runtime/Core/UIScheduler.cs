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

    internal enum UIIntent
    {
        Show,
        Hide,
        Close,
    }

    internal sealed class TransitionOperation
    {
        internal BaseUI UI;
        internal UIIntent Target;
        internal int TransitionVersion;
        internal bool TransitionScheduled;
        internal bool IsTransitioning;
        internal bool AnimationCompleted;
        internal long? QueueEntryId;
    }

    public class UIScheduler : MonoBehaviour
    {
        private UIManager manager;
        private UISettings settings;

        private Queue<TransitionOperation> scheduledOperations;
        private Dictionary<BaseUI, TransitionOperation> operations;
        private bool running = false;


        internal void Init(UIManager manager, UISettings settings)
        {
            this.manager = manager;
            this.settings = settings;

            scheduledOperations = new Queue<TransitionOperation>();
            operations = new Dictionary<BaseUI, TransitionOperation>();
        }

        private IEnumerator Run()
        {
            running = true;
            yield return new WaitForEndOfFrame();

            while (scheduledOperations.Count > 0)
            {
                var operation = scheduledOperations.Dequeue();
                operation.TransitionScheduled = false;
                if (!operations.ContainsKey(operation.UI))
                {
                    continue;
                }

                try
                {
                    ProcessTransition(operation);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            running = false;
        }

        internal void Request(BaseUI ui, UIIntent target)
        {
            var operation = GetOrCreate(ui);
            operation.Target = target;
            ScheduleTransition(operation);
        }

        internal void RequestQueueShow(BaseUI ui, long entryId)
        {
            var operation = GetOrCreate(ui);
            operation.Target = UIIntent.Show;
            operation.QueueEntryId = entryId;
            ScheduleTransition(operation);
        }

        internal void NotifyReady(BaseUI ui)
        {
            if (operations.TryGetValue(ui, out var operation))
            {
                ScheduleTransition(operation);
            }
        }

        internal void NotifyLoadFailed(BaseUI ui)
        {
            if (!operations.TryGetValue(ui, out var operation))
            {
                return;
            }

            NotifyPresentationEnded(operation);
            operations.Remove(ui);
        }

        private TransitionOperation GetOrCreate(BaseUI ui)
        {
            if (!operations.TryGetValue(ui, out var operation))
            {
                operation = new TransitionOperation() { UI = ui };
                operations[ui] = operation;
            }

            return operation;
        }

        private void ScheduleTransition(TransitionOperation operation)
        {
            if (operation.TransitionScheduled)
            {
                return;
            }

            operation.TransitionScheduled = true;
            scheduledOperations.Enqueue(operation);
            if (!running)
            {
                StartCoroutine(Run());
            }
        }

        private void ProcessTransition(TransitionOperation operation)
        {
            var ui = operation.UI;
            if (operation.AnimationCompleted)
            {
                operation.AnimationCompleted = false;
                if (ui.State == UIState.Show)
                {
                    CompleteShow(operation);
                }
                else
                {
                    CompleteHide(operation);
                }
                return;
            }

            if (ui.State <= UIState.Load)
            {
                return;
            }

            if (operation.Target == UIIntent.Show)
            {
                ProcessShow(operation);
            }
            else
            {
                ProcessHide(operation);
            }
        }

        private void ProcessShow(TransitionOperation operation)
        {
            var ui = operation.UI;
            if (ui.State == UIState.Shown)
            {
                return;
            }
            if (ui.State == UIState.Show && operation.IsTransitioning)
            {
                return;
            }

            BeginShow(operation);
        }

        private void ProcessHide(TransitionOperation operation)
        {
            var ui = operation.UI;
            if (ui.State == UIState.Open || ui.State == UIState.Hidden)
            {
                NotifyPresentationEnded(operation);

                if (operation.Target == UIIntent.Close)
                {
                    TryDestroy(operation);
                }
                else if (operation.Target == UIIntent.Show)
                {
                    ScheduleTransition(operation);
                }
                return;
            }
            if (ui.State == UIState.Hide && operation.IsTransitioning)
            {
                return;
            }

            BeginHide(operation);
        }

        private void BeginShow(TransitionOperation operation)
        {
            var ui = operation.UI;
            var transitionVersion = ++operation.TransitionVersion;
            operation.IsTransitioning = true;

            manager.Groups.AddToGroup(ui);
            manager.SetState(ui, UIState.Show);

            if (operation.Target != UIIntent.Show)
            {
                operation.IsTransitioning = false;
                ScheduleTransition(operation);
                return;
            }

            Show(ui.View, operation, transitionVersion);
        }

        private void CompleteShow(TransitionOperation operation)
        {
            operation.IsTransitioning = false;
            if (operation.Target != UIIntent.Show)
            {
                ScheduleTransition(operation);
                return;
            }

            manager.SetState(operation.UI, UIState.Shown);
            if (operation.Target != UIIntent.Show)
            {
                ScheduleTransition(operation);
            }
        }

        private void BeginHide(TransitionOperation operation)
        {
            var ui = operation.UI;
            var transitionVersion = ++operation.TransitionVersion;
            operation.IsTransitioning = true;

            manager.SetState(ui, UIState.Hide);
            if (operation.Target == UIIntent.Show)
            {
                operation.IsTransitioning = false;
                ScheduleTransition(operation);
                return;
            }

            Hide(ui.View, operation, transitionVersion);
        }

        private void CompleteHide(TransitionOperation operation)
        {
            operation.IsTransitioning = false;
            if (operation.Target == UIIntent.Show)
            {
                ScheduleTransition(operation);
                return;
            }

            var ui = operation.UI;
            manager.SetState(ui, UIState.Hidden);
            manager.Groups.RemoveFromGroup(ui);
            NotifyPresentationEnded(operation);

            if (operation.Target == UIIntent.Close)
            {
                TryDestroy(operation);
            }
            else if (operation.Target == UIIntent.Show)
            {
                ScheduleTransition(operation);
            }
        }

        private void TryDestroy(TransitionOperation operation)
        {
            NotifyPresentationEnded(operation);
            operations.Remove(operation.UI);
            manager.DestroyUI(operation.UI);
        }

        private void NotifyPresentationEnded(TransitionOperation operation)
        {
            if (!operation.QueueEntryId.HasValue)
            {
                return;
            }

            var entryId = operation.QueueEntryId.Value;
            operation.QueueEntryId = null;
            manager.Queue.NotifyCompleted(operation.UI, entryId);
        }

        private bool IsCurrentTransition(TransitionOperation operation, int transitionVersion)
        {
            return operations.ContainsKey(operation.UI) && operation.IsTransitioning && operation.TransitionVersion == transitionVersion;
        }

        private void NotifyAnimationCompleted(TransitionOperation operation, int transitionVersion)
        {
            if (!IsCurrentTransition(operation, transitionVersion))
            {
                return;
            }

            operation.AnimationCompleted = true;
            ScheduleTransition(operation);
        }

        private void Show(BaseView view, TransitionOperation operation, int transitionVersion)
        {
            if (settings.skipAnimation)
            {
                NotifyAnimationCompleted(operation, transitionVersion);
                return;
            }

            view.ShowAnimation(() => NotifyAnimationCompleted(operation, transitionVersion));
        }

        private void Hide(BaseView view, TransitionOperation operation, int transitionVersion)
        {
            if (settings.skipAnimation)
            {
                NotifyAnimationCompleted(operation, transitionVersion);
                return;
            }

            view.HideAnimation(() => NotifyAnimationCompleted(operation, transitionVersion));
        }
    }
}
