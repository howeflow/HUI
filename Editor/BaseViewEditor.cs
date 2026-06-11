using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HUI.Editor
{
    [CustomPropertyDrawer(typeof(ViewSettings))]
    public class ViewSettingsDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var root = new VisualElement();

            var settings = UISettings.Load();
            if (settings == null) {
                var helpBox = new HelpBox("No UISettings found. Please create one in Resources folder.", HelpBoxMessageType.Error);
                root.Add(helpBox);
                return root;
            }

            var targetView = property.serializedObject.targetObject as BaseView;

            if (targetView != null)
            {
                var parent = targetView.transform.parent;
                if (parent != null)
                {
                    var parentView = parent.GetComponentInParent<BaseView>();
                    if (parentView != null)
                    {
                        var helpBox = new HelpBox("Setting driven by parent view.", HelpBoxMessageType.None);
                        helpBox.style.height = 20;
                        root.Add(helpBox);
                        return root;
                    }
                }
            }


            var groups = settings.groups.Select(g => g.name).ToList();
            var groupProp = property.FindPropertyRelative("group");

            var popup = new PopupField<string>("Group", groups, groupProp.intValue);
            popup.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            popup.RegisterValueChangedCallback(evt => {
                groupProp.intValue = groups.IndexOf(evt.newValue);
                groupProp.serializedObject.ApplyModifiedProperties();
            });
            root.Add(popup);

            var priorityProp = property.FindPropertyRelative("priority");
            root.Add(new PropertyField(priorityProp, "Priority"));

            return root;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(BaseView))]
    public class BaseViewEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();

            var pathName = target.name;

            if (PrefabUtility.IsPartOfPrefabInstance(target))
            {
                var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(target);
                if (prefabAsset != null)
                {
                    pathName = prefabAsset.name;
                }
            }

            var scriptPath = UIValidator.GetScriptPath(pathName);
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);

            var scriptField = new ObjectField("Script") {
                objectType = typeof(MonoScript),
                value = script
            };
            scriptField.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            scriptField.SetEnabled(false);
            root.Add(scriptField);

            var settingsProp = serializedObject.FindProperty("setting");
            var field = new PropertyField(settingsProp);
            root.Add(field);


            return root;
        }
    }
}