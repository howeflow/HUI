using System;
using System.Collections.Generic;

namespace HUI
{
    internal interface IQueueCommand
    {
        string Name { get; }
        BaseUI Execute(UIManager manager, long entryId);
    }

    internal struct QueueCommand : IQueueCommand
    {
        private readonly string name;
        private readonly Type type;

        public string Name => name;

        internal QueueCommand(string name, Type type)
        {
            this.name = name;
            this.type = type;
        }

        public BaseUI Execute(UIManager manager, long entryId)
        {
            return manager.OpenUIFromQueue(name, type, entryId);
        }
    }

    internal struct QueueCommand<T> : IQueueCommand
    {
        private readonly string name;
        private readonly Type type;
        private readonly T parameter;

        public string Name => name;

        internal QueueCommand(string name, Type type, T parameter)
        {
            this.name = name;
            this.type = type;
            this.parameter = parameter;
        }

        public BaseUI Execute(UIManager manager, long entryId)
        {
            return manager.OpenUIFromQueue(name, type, parameter, entryId);
        }
    }

    public class UIQueueEntry
    {
        internal IQueueCommand Command { get; }
        internal long Id { get; }
        public BaseUI UI { get; internal set; }
        public string Name => Command.Name;

        internal UIQueueEntry(long id, IQueueCommand command)
        {
            Id = id;
            Command = command;
        }
    }

    public class UIQueue
    {
        public int Id { get; internal set; }
        public bool IsPaused { get; internal set; }
        public int Count => pending.Count + (Current == null ? 0 : 1);
        public IReadOnlyCollection<UIQueueEntry> Pending => pending;
        public UIQueueEntry Current { get; internal set; }

        private LinkedList<UIQueueEntry> pending;
        internal LinkedList<UIQueueEntry> PendingList => pending;

        internal UIQueue(int id)
        {
            this.Id = id;
            pending = new LinkedList<UIQueueEntry>();
        }
    }

    public class UIQueueManager
    {
        private UIManager manager;
        private Dictionary<int, UIQueue> queues;
        private Dictionary<long, UIQueue> entryQueues;
        private Dictionary<string, UIQueueEntry> activeNames;
        private long entryIdCounter;

        public IReadOnlyCollection<UIQueue> Queues => queues.Values;

        internal UIQueueManager(UIManager manager)
        {
            this.manager = manager;
            queues = new Dictionary<int, UIQueue>();
            entryQueues = new Dictionary<long, UIQueue>();
            activeNames = new Dictionary<string, UIQueueEntry>();
        }

        internal void NotifyCompleted(BaseUI ui, long entryId)
        {
            if (!entryQueues.TryGetValue(entryId, out var queue))
            {
                return;
            }

            var entry = queue.Current;
            if (entry.UI == null)
            {
                entry.UI = ui;
            }

            entryQueues.Remove(entryId);
            activeNames.Remove(entry.Name);

            queue.Current = null;
            Execute(queue);
            ExecuteWaitingQueues();
        }

        private void Execute(UIQueue queue)
        {
            if (queue.IsPaused)
                return;

            if (queue.Current != null)
                return;

            if (queue.PendingList.First == null)
                return;

            var entry = queue.PendingList.First.Value;
            if (activeNames.ContainsKey(entry.Name))
                return;

            queue.PendingList.RemoveFirst();
            queue.Current = entry;
            activeNames[entry.Name] = entry;
            entryQueues[entry.Id] = queue;

            entry.UI = entry.Command.Execute(manager, entry.Id);
        }

        private void ExecuteWaitingQueues()
        {
            foreach (var queue in queues.Values)
            {
                Execute(queue);
            }
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

        private UIQueueEntry CreateEntry(IQueueCommand command)
        {
            return new UIQueueEntry(++entryIdCounter, command);
        }

        internal void Add(IQueueCommand command, int queueId = 0, bool first = false)
        {
            var queue = GetOrCreate(queueId);
            var entry = CreateEntry(command);

            if (first)
                queue.PendingList.AddFirst(entry);
            else
                queue.PendingList.AddLast(entry);

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
                queue.PendingList.Clear();
            }
        }

        public void ClearAll()
        {
            foreach (var queue in queues.Values)
            {
                queue.PendingList.Clear();
            }
        }
    }
}
