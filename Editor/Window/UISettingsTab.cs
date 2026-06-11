using HUI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace HUI.Editor
{
    public class UISettingsTab : VisualElement
    {

        private UISettings settings;
        private SerializedObject serializedObject;

        public void Init(UISettings settings)
        {
            this.settings = settings;
            serializedObject = new SerializedObject(settings);

            this.Add(CreateSettingsTab());
        }

        private VisualElement CreateSettingsTab()
        {
            var container = new VisualElement();
            container.style.flexGrow = 1;

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            container.Add(scrollView);

            var settingsInspector = new InspectorElement(serializedObject);
            scrollView.Add(settingsInspector);

            var buttonContainer = new VisualElement();
            buttonContainer.style.marginTop = 10;
            buttonContainer.style.flexDirection = FlexDirection.Row;
            scrollView.Add(buttonContainer);

            var resetButton = new Button(() => ResetSettings())
            {
                text = "Reset"
            };
            resetButton.style.height = 30;
            resetButton.style.flexGrow = 1;
            buttonContainer.Add(resetButton);

            return container;
        }

        private void ResetSettings()
        {
            if (settings == null) return;

            if (EditorUtility.DisplayDialog("Reset Settings",
                "Are you sure you want to reset all settings to default values? This cannot be undone.",
                "Yes", "No"))
            {
                settings.Reset();

                EditorUtility.SetDirty(settings);
                serializedObject.Update();
            }
        }
    }

}