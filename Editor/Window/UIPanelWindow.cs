using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;

namespace HUI.Editor
{
    public class UIPanelWindow : EditorWindow
    {
        [MenuItem("GameObject/HUI/Root",priority=10)]
        static void CreateRoot()
        {
            string prefabPath = "UIRoot";
            var rootPrefab = Resources.Load<GameObject>(prefabPath);
            GameObject uiRoot = GameObject.Instantiate(rootPrefab);
            uiRoot.name = rootPrefab.name;

            EditorUtility.SetDirty(uiRoot);

            Selection.activeGameObject = uiRoot;

            Debug.Log($"Create UI Root：{uiRoot.name}");
        }

        [MenuItem("Tools/UI Panel &u")]
        public static void ShowWindow()
        {
            var window = GetWindow<UIPanelWindow>();
            window.titleContent = new GUIContent("UI Panel");
            window.minSize = new Vector2(400, 300);
        }

        private UISettings settings;

        private ToolbarToggle settingsTabButton;
        private ToolbarToggle generateTabButton;
        private ToolbarToggle statisticsTabButton;


        private UISettingsTab settingsTab;
        private UIGenerateTab generateTab;
        private UIStatisticsTab statisticsTab;


        public void CreateGUI()
        {
            settings = UISettings.Load();

            var root = rootVisualElement;
            root.style.paddingTop = 5;
            root.style.paddingBottom = 5;
            root.style.paddingLeft = 5;
            root.style.paddingRight = 5;

            if (settings == null)
            {
                var helpBox = new HelpBox("No UISettings found. Please create one in Resources folder.", HelpBoxMessageType.Error);
                root.Add(helpBox);
                return;
            }

            CreateTabToggles(root);
            CreateTabContents(root);

            var key = EditorPrefs.GetInt("UISettingsWindow_LastTab", 0);
            SwitchToTab(key);
        }

        private void CreateTabToggles(VisualElement root)
        {
            var tabTogglesContainer = new VisualElement();
            tabTogglesContainer.style.flexDirection = FlexDirection.Row;
            tabTogglesContainer.style.borderBottomWidth = 2;
            tabTogglesContainer.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            tabTogglesContainer.style.height = 30;

            root.Add(tabTogglesContainer);

            settingsTabButton = CreateToggle(tabTogglesContainer, "Settings", () => SwitchToTab(0));
            statisticsTabButton = CreateToggle(tabTogglesContainer, "Statistics", () => SwitchToTab(1));
            generateTabButton = CreateToggle(tabTogglesContainer, "Generate", () => SwitchToTab(2));
        }

        private ToolbarToggle CreateToggle(VisualElement contanier,string name,Action changed)
        {
            var toggle = new ToolbarToggle
            {
                text = name
            };
            toggle.style.flexGrow = 1;
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue) changed();
            });
            contanier.Add(toggle);

            return toggle;
        }

        private void CreateTabContents(VisualElement root)
        {
            var tabContainer = new VisualElement();
            tabContainer.style.height = Length.Percent(100);
            root.Add(tabContainer);

            settingsTab = new UISettingsTab();
            settingsTab.Init(settings);
            tabContainer.Add(settingsTab);

            generateTab = new UIGenerateTab();
            generateTab.Init(settings);
            tabContainer.Add(generateTab);

            statisticsTab = new UIStatisticsTab();
            statisticsTab.Init(settings);
            tabContainer.Add(statisticsTab);
        }

        private void SwitchToTab(int tabIndex)
        {
            EditorPrefs.SetInt("UISettingsWindow_LastTab", tabIndex);

            settingsTab.style.display = tabIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            statisticsTab.style.display = tabIndex == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            generateTab.style.display = tabIndex == 2 ? DisplayStyle.Flex : DisplayStyle.None;

            settingsTabButton.SetValueWithoutNotify(tabIndex == 0);
            statisticsTabButton.SetValueWithoutNotify(tabIndex == 1);
            generateTabButton.SetValueWithoutNotify(tabIndex == 2);

            if (tabIndex == 1)
            {
                statisticsTab.RefreshStats();
                statisticsTab.RefreshViews();
            }
        }
    }
}

