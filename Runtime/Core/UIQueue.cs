using System;
using System.Collections;
using System.Collections.Generic;

namespace HUI
{
    public interface IQueueCommand
    {
        string Name { get; }
        void Execute(UIManager manager);
    }
    public struct QueueCommand : IQueueCommand
    {
        public string name;
        public Type type;

        public string Name => name;

        public QueueCommand(string name, Type type)
        {
            this.name = name;
            this.type = type;
        }

        public void Execute(UIManager manager)
        {
            var ui = manager.GetUI(name);
            if (ui == null || ui.State < UIState.Open)
            {
                manager.OpenUI(name, type);
            }
            else
            {
                manager.ShowUI(ui);
            }
        }
    }
    public struct QueueCommand<T> : IQueueCommand
    {
        public string name;
        public Type type;
        public T parameter;

        public string Name => name;

        public QueueCommand(string name, Type type, T parameter)
        {
            this.name = name;
            this.type = type;
            this.parameter = parameter;
        }

        public void Execute(UIManager manager)
        {
            var ui = manager.GetUI(name);
            if (ui == null || ui.State < UIState.Open)
            {
                manager.OpenUI(name, type, parameter);
            }
            else
            {
                manager.SetParameter(ui, parameter);
                manager.ShowUI(ui);
            }
        }
    }

    public class UIQueue
    {
        public int Id { get; internal set; }
        public bool IsPaused { get; internal set; }
        public int Count => List.Count;
        public LinkedList<IQueueCommand> List { get; internal set; }
        public LinkedListNode<IQueueCommand> Current { get; internal set; }
        public UIQueue(int id)
        {
            this.Id = id;
            List = new LinkedList<IQueueCommand>();
        }
    }

    public class UIQueueManager
    {
        private UIManager manager;
        private Dictionary<int, UIQueue> queues;
        public IReadOnlyCollection<UIQueue> Queues => queues.Values;

        public UIQueueManager(UIManager manager)
        {
            this.manager = manager;
            queues = new Dictionary<int, UIQueue>();
        }

        internal void NotifyHidden(BaseUI ui)
        {
            foreach (var queue in queues.Values)
            {
                if (queue.Current == null)
                    continue;

                var command = queue.Current.Value;
                if (command.Name != ui.Name)
                    continue;

                queue.List.Remove(queue.Current);
                queue.Current = null;

                Execute(queue);
            }
        }

        private void Execute(UIQueue queue)
        {
            if (queue.IsPaused)
                return;

            if (queue.Current != null)
                return;

            if (queue.List.First == null)
                return;

            queue.Current = queue.List.First;
            var command = queue.Current.Value;

            command.Execute(manager);
        }

        private UIQueue GetOrCreate(int queueId)
        {
            if (!queues.TryGetValue(queueId, out var queue))
            {
                queue = new UIQueue(queueId);
                queues[queueId] = queue;
            }
            return queue;
        }
        public void Add(IQueueCommand command, int queueId = 0)
        {
            var queue = GetOrCreate(queueId);

            queue.List.AddLast(command);

            Execute(queue);
        }
        public void Insert(IQueueCommand command, int index, int queueId = 0)
        {
            var queue = GetOrCreate(queueId);

            if (index < 0 || index > queue.List.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var node = queue.List.First;
            for (int i = 0; i < index && node != null; i++)
                node = node.Next;

            if (node == null)
                queue.List.AddLast(command);
            else
                queue.List.AddBefore(node, command);

            Execute(queue);
        }

        public void Pause(int queueId = 0)
        {
            if (queues.TryGetValue(queueId, out var queue))
            {
                queue.IsPaused = true;
            }
        }
        public void Resume(int queueId = 0)
        {
            if (queues.TryGetValue(queueId, out var queue))
            {
                queue.IsPaused = false;
                Execute(queue);
            }
        }

        public void Clear(int queueId = 0)
        {
            if (queues.TryGetValue(queueId, out var queue))
            {
                queue.List.Clear();
                queue.Current = null;
            }
            queues.Remove(queueId);
        }

        public void ClearAll()
        {
            foreach (var queue in queues.Values)
            {
                queue.List.Clear();
                queue.Current = null;
            }
            queues.Clear();
        }
    }
}