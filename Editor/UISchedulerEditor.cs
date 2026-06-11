using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HUI.Editor
{
    [CustomEditor(typeof(UIScheduler))]
    public class UISchedulerEditor : UnityEditor.Editor
    {
        private VisualElement rootElement;
        private Foldout groupFoldout;
        private Foldout queueFoldout;

        private void OnEnable()
        {
            if (!UIKit.IsInitialized)
                return;

            UIKit.Manager.Events.OnChanged += ReBuild;
        }
        private void OnDisable()
        {
            if (!UIKit.IsInitialized)
                return;

            UIKit.Manager.Events.OnChanged -= ReBuild;
        }
        private void ReBuild(BaseUI ui)
        {
            ReBuild();
        }


        public override VisualElement CreateInspectorGUI()
        {
            rootElement = new VisualElement();
            BuildUI(rootElement);
            return rootElement;
        }

        private void BuildUI(VisualElement root)
        {
            if (!UIKit.IsInitialized)
            {
                return;
            }

            root.Add(CreateSpacer(5));

            // Groups Section
            groupFoldout = new Foldout { text = $"UI Groups"};
            root.Add(groupFoldout);
            root.Add(CreateSpacer(10));

            // Queues Section
            queueFoldout = new Foldout { text = "UI Queues"};
            root.Add(queueFoldout);
            root.Add(CreateSpacer(10));

            ReBuild();
        }

        private void ReBuild()
        {
            groupFoldout.Clear();
            var groupBox = CreateBox();
            BuildGroups(groupBox);
            groupFoldout.Add(groupBox);

            queueFoldout.Clear();
            var queueBox = CreateBox();
            BuildQueues(queueBox);
            queueFoldout.Add(queueBox);
        }


        private void BuildGroups(VisualElement container)
        {
            var mgr = UIKit.Manager;
            if (mgr.Groups.All(p => p.Count == 0))
            {
                var emptyLabel = new Label("No uis available");
                emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                emptyLabel.style.color = Color.gray;
                emptyLabel.style.marginTop = 5;
                emptyLabel.style.marginBottom = 5;
                container.Add(emptyLabel);
                return;
            }

            foreach (var group in mgr.Groups.Where(p => p.Count > 0))
            {
                var groupLabel = new Label($"{group.Info.name}: {group.Count}");
                groupLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                container.Add(groupLabel);

                var itemBox = CreateBox();

                foreach (var ui in group)
                {
                    var row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.justifyContent = Justify.SpaceBetween;
                    row.style.marginBottom = 1;

                    var capturedUI = ui;
                    var nameBtn = new Button(() => EditorGUIUtility.PingObject(capturedUI.View))
                    {
                        text = capturedUI.Name
                    };
                    nameBtn.style.flexGrow = 1;
                    nameBtn.style.flexShrink = 1;
                    nameBtn.style.unityTextAlign = TextAnchor.MiddleLeft;
                    nameBtn.style.backgroundColor = Color.clear;
                    nameBtn.style.borderTopWidth = nameBtn.style.borderBottomWidth =
                    nameBtn.style.borderLeftWidth = nameBtn.style.borderRightWidth = 0;
                    row.Add(nameBtn);

                    var stateLabel = new Label($"[{capturedUI.State}]");
                    stateLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    stateLabel.style.color = Color.gray;
                    stateLabel.style.width = 80;
                    row.Add(stateLabel);

                    var closeBtn = new Button(() => capturedUI.Close()) { text = "Close" };
                    closeBtn.style.width = 60;
                    row.Add(closeBtn);

                    itemBox.Add(row);
                }

                container.Add(itemBox);
                container.Add(CreateSpacer(2));
            }
        }

        private void BuildQueues(VisualElement container)
        {
            var queues = UIKit.Manager.Queue;

            if (queues.Queues.All(p => p.Count == 0))
            {
                var emptyLabel = new Label("No queues available");
                emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                emptyLabel.style.color = Color.gray;
                emptyLabel.style.marginTop = 5;
                emptyLabel.style.marginBottom = 5;
                container.Add(emptyLabel);
                return;
            }

            foreach (var queue in queues.Queues.Where(p => p.Count > 0))
            {
                var queueLabel = new Label($"ID: {queue.Id}");
                queueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                container.Add(queueLabel);

                container.Add(CreateSpacer(4));

                var box = CreateBox();

                //  Commands
                if (queue.Count > 0)
                {
                    box.Add(CreateSpacer(2));

                    var pendingLabel = new Label($"Commands: {queue.Count}");
                    pendingLabel.style.fontSize = 10;
                    box.Add(pendingLabel);

                    box.Add(CreateSpacer(2));

                    foreach (var command in queue.List)
                    {
                        var ui = UIKit.GetUI(command.Name);

                        var row = new VisualElement();
                        row.style.justifyContent= Justify.SpaceBetween;
                        row.style.flexDirection = FlexDirection.Row;
                        box.Add(row);

                        var nameBtn = new Button(() => EditorGUIUtility.PingObject(ui.View))
                        {
                            text = command.Name
                        };
                        nameBtn.style.flexGrow = 1;
                        nameBtn.style.flexShrink = 1;
                        nameBtn.style.unityTextAlign = TextAnchor.MiddleLeft;
                        nameBtn.style.backgroundColor = Color.clear;
                        nameBtn.style.borderTopWidth = nameBtn.style.borderBottomWidth =
                        nameBtn.style.borderLeftWidth = nameBtn.style.borderRightWidth = 0;
                        row.Add(nameBtn);

                        if (queue.Current != null && queue.Current.Value == command)
                        {
                            var currentLabel = new Label("Current");
                            currentLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                            currentLabel.style.color = Color.gray;
                            currentLabel.style.width = 80;

                            var closeBtn = new Button(() => ui.Close()) { text = "Close" };
                            closeBtn.style.width = 60;

                            row.Add(currentLabel);
                            row.Add(closeBtn);
                        }
                    }
                }

                container.Add(box);
                container.Add(CreateSpacer(2));
            }
        }

        private static VisualElement CreateSpacer(float height)
        {
            return new VisualElement { style = { height = height } };
        }

        private static VisualElement CreateBox()
        {
            var box = new VisualElement();
            box.style.borderTopWidth = box.style.borderBottomWidth =
                box.style.borderLeftWidth = box.style.borderRightWidth = 1;
            box.style.borderTopColor = box.style.borderBottomColor =
                box.style.borderLeftColor = box.style.borderRightColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            box.style.borderTopLeftRadius = box.style.borderTopRightRadius =
                box.style.borderBottomLeftRadius = box.style.borderBottomRightRadius = 3;
            box.style.paddingTop = box.style.paddingBottom =
                box.style.paddingLeft = box.style.paddingRight = 4;
            box.style.marginTop = 2;
            box.style.marginBottom = 2;
            box.style.marginLeft = 0;
            box.style.backgroundColor = new Color(0, 0, 0, 0.1f);
            return box;
        }
    }
}
