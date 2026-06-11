using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HUI.Editor
{
    public class UIGenerateTab : VisualElement
    {
        private string scriptPath = "Assets/Scripts";
        private string namespaceName = "";
        private string className = "";
        private bool generateWithParameter;
        private bool generateWithView;
        private bool generateUIComponent;
        private TextField previewTextField;


        private string prefabPath = "Assets/Prefabs";
        private string prefabName = "";
        private bool createMask = false;
        private bool createRoot = false;
        private int animationTypeIndex = 0;
        private Type[] animationTypes;
        private string[] animationTypeNames;
        private int prefabGroupIndex = 0;
        private Priority prefabPriority = Priority.Normal;

        private int mode;
        private VisualElement prefabSection;
        private VisualElement codeSection;

        private UISettings settings;


        #region UI Helper Methods

        private VisualElement CreateSection(VisualElement parent)
        {
            var section = new VisualElement();

            section.style.marginTop = 5;
            section.style.paddingTop = 10;
            section.style.paddingLeft = 10;
            section.style.paddingRight = 10;
            section.style.paddingBottom = 10;
            section.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.2f);
            section.style.borderTopLeftRadius = 5;
            section.style.borderTopRightRadius = 5;
            
            parent.Add(section);
            return section;
        }

        private VisualElement CreateFieldContainer(VisualElement parent, string labelText, int labelWidth = 100)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.marginBottom = 8;
            parent.Add(container);

            var label = new Label(labelText);
            label.style.minWidth = labelWidth;
            container.Add(label);

            return container;
        }

        private TextField CreateTextField(VisualElement parent,string label, string initialValue, Action<string> onValueChanged)
        {
            var container = CreateFieldContainer(parent, label);

            var textField = new TextField();
            textField.value = initialValue;
            textField.style.flexGrow = 1;
            textField.style.flexShrink = 1;
            textField.style.minWidth = 0;
            textField.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(evt.newValue));

            container.Add(textField);
            parent.Add(container);

            return textField;
        }

        private void CreatePathFieldWithBrowse(VisualElement parent, string labelText, string initialPath, Action<string> onPathChanged)
        {
            var container = CreateTextField(parent, labelText, initialPath, onPathChanged);

            var localPathField = container;
            var browseButton = new Button(() =>
            {
                var selectedPath = EditorUtility.OpenFolderPanel($"Select {labelText}", localPathField.value, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        var newPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                        localPathField.value = newPath;
                        onPathChanged?.Invoke(newPath);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Path", "Please select a path inside the Assets folder.", "OK");
                    }
                }
            })
            {
                text = "..."
            };
            browseButton.style.width = 30;
            browseButton.style.height = 20;
            browseButton.style.marginLeft = 5;
            container.Add(browseButton);
        }

        private Toggle CreateToggle(VisualElement parent, string text, bool initialValue, Action<bool> onValueChanged)
        {
            var toggle = new Toggle();
            toggle.style.flexDirection = FlexDirection.Row;
            toggle.value = initialValue;
            toggle.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(evt.newValue));

            var label = new Label(text);
            label.style.marginLeft = 5;
            label.style.marginRight = 20;
            toggle.Add(label);

            parent.Add(toggle);

            return toggle;
        }

        private DropdownField CreateDropdownField(VisualElement parent, List<string> choices, int initialIndex, Action<int> onValueChanged)
        {
            var dropdown = new DropdownField(choices, initialIndex);
            dropdown.style.flexGrow = 1;
            dropdown.style.flexShrink = 1;
            dropdown.style.marginLeft = 0;
            dropdown.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(dropdown.index));
            parent.Add(dropdown);
            return dropdown;
        }

        #endregion

        public void Init(UISettings settnigs)
        {
            this.settings = settnigs;

            LoadAnimationTypes();

            var tab = CreateCodeGenerateTab();
            Add(tab);
        }

        private void LoadAnimationTypes()
        {
            var types = UIGenerator.GetInstanceTypes<IUIAnimation>().Where(p=> typeof(MonoBehaviour).IsAssignableFrom(p)).ToList();
            var names = types.Select(t => t.Name).ToList();

            types.Insert(0, null);
            animationTypes = types.ToArray();

            names.Insert(0, "None");
            animationTypeNames = names.ToArray();
        }


        private VisualElement CreateCodeGenerateTab()
        {
            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.flexDirection = FlexDirection.Column;

            var combinedSection = CreateOptionsSection();
            container.Add(combinedSection);

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            container.Add(scrollView);

            var content = CreateContent();
            scrollView.Add(content);

            return container;
        }



        private VisualElement CreateOptionsSection()
        {
            var container = new VisualElement();
            container.style.marginTop = 10;
            container.style.marginLeft = 10;
            container.style.marginRight = 10;
            container.style.paddingTop = 10;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            container.style.paddingBottom = 10;
            container.style.borderTopLeftRadius = 5;
            container.style.borderTopRightRadius = 5;
            container.style.borderBottomLeftRadius = 5;
            container.style.borderBottomRightRadius = 5;
            container.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.2f);

            // Header Section (Fixed, not scrollable)
            var title = new Label("Generate");
            title.style.fontSize = 14;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 10;
            container.Add(title);

            var optionsContainer = new VisualElement();
            optionsContainer.style.flexDirection = FlexDirection.Row;
            container.Add(optionsContainer);


            CreateDropdownField(optionsContainer, new List<string>() { "Prefab Generator", "Code Generator", "All Generator" }, mode, index =>
            {
                mode = index;
                prefabSection.style.display = mode == 0 || mode == 2 ? DisplayStyle.Flex : DisplayStyle.None;
                codeSection.style.display = mode == 1 || mode == 2 ? DisplayStyle.Flex : DisplayStyle.None;
            });


           var generateBothButton = new Button(() =>
            {
                if (mode == 0)
                {
                    GeneratePrefab();
                }
                if (mode == 1)
                {
                    GeneratedCode();
                }
                if (mode == 2)
                {
                    GenerateBoth();
                }
            })
            {
                text = "Generate"
            };
            generateBothButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            generateBothButton.style.height = 25;
            generateBothButton.style.width = 120;
            generateBothButton.style.marginLeft = 10;
            optionsContainer.Add(generateBothButton);

            return container;
        }

        private VisualElement CreateContent()
        {
            var container = new VisualElement();
            container.style.marginLeft = 10;
            container.style.marginRight = 10;

            // Settings Section
            prefabSection = CreateSection(container);

            // Prefab Name Field
            var prefabNameField = CreateTextField(prefabSection, "Prefab Name", prefabName, value =>
            {
                prefabName = value;
            });

            // Group Dropdown
            var groupContainer = CreateFieldContainer(prefabSection, "Group");
            var groupChoices = new List<string>();
            if (settings.groups != null && settings.groups.Count > 0)
            {
                for (int i = 0; i < settings.groups.Count; i++)
                {
                    groupChoices.Add(settings.groups[i].name);
                }
            }
            else
            {
                groupChoices.Add("No Groups Available");
            }
            CreateDropdownField(groupContainer, groupChoices, prefabGroupIndex, index => prefabGroupIndex = index);

            // Priority Dropdown
            var priorityContainer = CreateFieldContainer(prefabSection, "Priority");
            var priorityValues = System.Enum.GetValues(typeof(Priority)).Cast<Priority>().OrderBy(p => (int)p).ToList();
            var priorityChoices = priorityValues.Select(p => p.ToString()).ToList();
            var currentPriorityIndex = priorityValues.IndexOf(prefabPriority);
            var priorityDropdown = CreateDropdownField(priorityContainer, priorityChoices, currentPriorityIndex, index =>
            {
                if (System.Enum.TryParse<Priority>(priorityChoices[index], out var newPriority))
                {
                    prefabPriority = newPriority;
                }
            });

            // Animation Type Dropdown
            var animationContainer = CreateFieldContainer(prefabSection, "Animation");
            CreateDropdownField(animationContainer, animationTypeNames.ToList(), animationTypeIndex,
                index => animationTypeIndex = index);

            // Options Row
            var prefabPptionsRow = CreateFieldContainer(prefabSection, "Options");
            CreateToggle(prefabPptionsRow, "Mask", createMask, value => createMask = value);
            CreateToggle(prefabPptionsRow, "Root", createRoot, value => createRoot = value);

            // Path Field
            prefabPath = settings.prefabPath;
            CreatePathFieldWithBrowse(prefabSection, "Output Path", prefabPath, value =>
            {
                prefabPath = value;
            });


            // Code Section
            codeSection = CreateSection(container);
            
            // Class Name Field
            CreateTextField(codeSection, "Class Name", className, value =>
            {
                className = value;
                UpdatePreview();
            });

            // Namespace Field
            CreateTextField(codeSection, "Namespace", namespaceName, value =>
            {
                namespaceName = value;
                UpdatePreview();
            });

            // Options Row
            var optionsRow = CreateFieldContainer(codeSection, "Options");

            CreateToggle(optionsRow, "UIComponent", generateUIComponent, value =>
            {
                generateUIComponent = value;
                UpdatePreview();
            });


            CreateToggle(optionsRow, "Parameter", generateWithParameter, value =>
            {
                generateWithParameter = value;
                UpdatePreview();
            });

            CreateToggle(optionsRow, "View", generateWithView, value =>
            {
                generateWithView = value;
                UpdatePreview();
            });


            // Path Field
            scriptPath = settings.scriptPath;
            CreatePathFieldWithBrowse(codeSection, "Output Path", scriptPath, value =>
            {
                scriptPath = value;
            });

            // Preview Section
            var previewSection = new VisualElement();
            previewSection.style.flexGrow = 1;
            previewSection.style.paddingTop = 10;
            previewSection.style.paddingLeft = 10;
            previewSection.style.paddingRight = 10;
            previewSection.style.paddingBottom = 10;
            previewSection.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
            previewSection.style.borderBottomLeftRadius = 5;
            previewSection.style.borderBottomRightRadius = 5;
            previewSection.style.flexDirection = FlexDirection.Column;
            codeSection.Add(previewSection);

            var previewLabel = new Label("Code Preview");
            previewLabel.style.fontSize = 11;
            previewLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            previewLabel.style.marginBottom = 8;
            previewLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            previewSection.Add(previewLabel);

            previewTextField = new TextField();
            previewTextField.multiline = true;
            previewTextField.style.flexGrow = 1;
            previewTextField.style.minHeight = 50;
            previewTextField.style.unityFontStyleAndWeight = FontStyle.Normal;
            previewTextField.SetEnabled(false);
            previewSection.Add(previewTextField);

            // Initial preview
            UpdatePreview();

            prefabSection.style.display = mode == 0 || mode == 2 ? DisplayStyle.Flex : DisplayStyle.None;
            codeSection.style.display = mode == 1 || mode == 2 ? DisplayStyle.Flex : DisplayStyle.None;

            return container;
        }


        private void UpdatePreview()
        {
            if (previewTextField == null) return;

            try
            {
                string preview = "";
                if (generateUIComponent)
                {
                    preview = UIGenerator.GenerateUIComponentContent(className, namespaceName, generateWithParameter, generateWithView);
                }
                else
                {
                    preview = UIGenerator.GenerateUIContent(className, namespaceName, generateWithParameter, generateWithView);
                }
                previewTextField.value = preview;
            }
            catch (System.Exception ex)
            {
                previewTextField.value = $"// {ex.Message}";
            }

        }


        private void GeneratedCode()
        {
            try
            {
                if (string.IsNullOrEmpty(className))
                {
                    throw new Exception("className can not be empty.");
                }

                var filePath = $"{scriptPath}/{(generateWithView ? $"{className}View" : $"{className}")}.cs";

                if (generateUIComponent)
                {
                    UIGenerator.GenerateUIComponentCode(filePath, className, namespaceName, generateWithParameter, generateWithView);
                }
                else
                {
                    UIGenerator.GenerateUICode(filePath, className, namespaceName, generateWithParameter, generateWithView);
                }


                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to generate code:{ex.Message}");
            }
        }

        private void GeneratePrefab()
        {
            try
            {
                if (string.IsNullOrEmpty(prefabName))
                {
                    throw new Exception("prefabName can not be empty.");
                }

                var option = new UIGenerator.CreatePrefabOptions()
                {
                    prefabName = prefabName + (generateWithView ? "View" : ""),
                    outputPath = prefabPath,
                    settings = new () { group = prefabGroupIndex, priority = prefabPriority },
                    animType = animationTypes[animationTypeIndex],
                    createMask = createMask,
                    createRoot = createRoot
                };

                var prefab = UIGenerator.GeneratePrefab(option);

                if (prefab != null)
                {
                    EditorGUIUtility.PingObject(prefab);
                    Selection.activeObject = prefab;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to generate prefab: {ex.Message}");
            }
        }

        private void GenerateBoth()
        {
            try
            {
                GeneratedCode();

                GeneratePrefab();


                if (generateWithView)
                {
                    CompilationPipeline.compilationFinished += OnCompilationFinished;

                    void OnCompilationFinished(object obj)
                    {
                        CompilationPipeline.compilationFinished -= OnCompilationFinished;

                        SessionState.SetBool(PendingData_Has_Key, true);

                        var scriptPath = $"{this.scriptPath}/{className}View.cs";
                        SessionState.SetString(PendingData_ScriptPath_Key, scriptPath);

                        var prefabPath = $"{this.prefabPath}/{prefabName}View.prefab";
                        SessionState.SetString(PendingData_PrefabPath_Key, prefabPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to generate: {ex.Message}");
            }
        }




        // SessionState key for persisting generation data
        private const string PendingData_Has_Key = "UISettingsWindow_PendingData_Has";
        private const string PendingData_ScriptPath_Key = "UISettingsWindow_PendingData_ScriptPath";
        private const string PendingData_PrefabPath_Key = "UISettingsWindow_PendingData_PrefabPath";



        [InitializeOnLoadMethod]
        private static void OnCompilationFinished()
        {
            EditorApplication.delayCall += OnReloadComplete;
        }

        private static void OnReloadComplete()
        {
            if (!SessionState.GetBool(PendingData_Has_Key, false))
            {
                return;
            }
            SessionState.EraseBool(PendingData_Has_Key);

            var scriptPath = SessionState.GetString(PendingData_ScriptPath_Key, "");
            var prefabPath = SessionState.GetString(PendingData_PrefabPath_Key, "");

            try
            {
                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
                var type = monoScript.GetClass();
                if (type == null)
                {
                    Debug.LogError("GetClass still null after Domain Reload.");
                    return;
                }

                var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var view = go.GetComponent<BaseView>();

                var setting = view.Setting;

                GameObject.DestroyImmediate(view, true);

                view = go.AddComponent(type) as BaseView;
                view.Setting = setting;

                Debug.Log($"Successfully replaced view component with generated script:{type.FullName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to repalce view type:{ex.Message}");
            }
        }
    }

}