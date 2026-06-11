using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HUI.Editor
{
    public class UIGenerator
    {
        public static string GenerateUIComponentContent(string className, string namespaceName = null, bool parameter = false, bool view = false)
        {
            ValidateClassName(className);
            ValidateType(className, namespaceName);

            bool hasNamespace = !string.IsNullOrEmpty(namespaceName);

            string parameterClassName = $"{className}Parameter";
            string viewClassName = $"{className}View";


            var builder = new StringBuilder();


            // Add usings
            builder.AppendLine($"using UnityEngine;");
            builder.AppendLine($"using HUI;");
            builder.AppendLine();

            // Add namespace if specified
            var indent = "";
            if (hasNamespace)
            {
                builder.AppendLine($"namespace {namespaceName}");
                builder.AppendLine($"{{");
                indent = "    ";
            }

            if (view)
            {
                // Generate View class (inherits from BaseView)
                builder.AppendLine($"{indent}public class {viewClassName} : BaseView");
                builder.AppendLine($"{indent}{{");
                builder.AppendLine($"{indent}");
                builder.AppendLine($"{indent}}}");
                builder.AppendLine();
            }

            if (parameter)
            {
                builder.AppendLine($"{indent}public class {parameterClassName}");
                builder.AppendLine($"{indent}{{");
                builder.AppendLine($"{indent}{indent}");
                builder.AppendLine($"{indent}}}");
                builder.AppendLine();
            }

            var baseClass = view ? (parameter ? $"BaseViewComponent<{viewClassName}, {parameterClassName}>" : $"BaseViewComponent<{viewClassName}>")
                : (parameter ? $"BaseComponent<{parameterClassName}>" : "BaseComponent");

            builder.AppendLine($"{indent}public class {className} : {baseClass}");
            builder.AppendLine($"{indent}{{");
            builder.AppendLine($"{indent}");
            builder.AppendLine($"{indent}}}");


            // Close namespace if specified
            if (hasNamespace)
            {
                builder.AppendLine($"}}");
            }

            return builder.ToString();
        }
        public static void GenerateUIComponentCode(string filePath, string className, string namespaceName = null, bool parameter = false, bool view = false)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception($"Output path cannot be empty.");
            }

            if (File.Exists(filePath))
            {
                throw new Exception($"File already exists at path: {filePath}");
            }

            string scriptContent = GenerateUIComponentContent(className, namespaceName, parameter, view);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, scriptContent);
            AssetDatabase.Refresh();

            Debug.Log($"Script generated at path: {filePath}");
        }

        public static string GenerateUIContent(string className, string namespaceName = null, bool parameter = false, bool view = false)
        {

            ValidateClassName(className);
            ValidateType(className, namespaceName);

            bool hasNamespace = !string.IsNullOrEmpty(namespaceName);

            string parameterClassName = $"{className}Parameter";
            string viewClassName = $"{className}View";


            var builder = new StringBuilder();


            // Add usings
            builder.AppendLine($"using UnityEngine;");
            builder.AppendLine($"using HUI;");
            builder.AppendLine();

            // Add namespace if specified
            var indent = "";
            if (hasNamespace)
            {
                builder.AppendLine($"namespace {namespaceName}");
                builder.AppendLine($"{{");
                indent = "    ";
            }

            if (view)
            {
                // Generate View class (inherits from BaseView)
                builder.AppendLine($"{indent}public class {viewClassName} : BaseView");
                builder.AppendLine($"{indent}{{");
                builder.AppendLine($"{indent}");
                builder.AppendLine($"{indent}}}");
                builder.AppendLine();
            }

            if (parameter)
            {
                builder.AppendLine($"{indent}public class {parameterClassName}");
                builder.AppendLine($"{indent}{{");
                builder.AppendLine($"{indent}{indent}");
                builder.AppendLine($"{indent}}}");
                builder.AppendLine();
            }

            // Add UIPath attribute and class
            var pathName = view ? viewClassName : className;
            builder.AppendLine($"{indent}[UIPath(nameof({pathName}))]");

            
            var baseClass = view ? (parameter ? $"BaseViewUI<{viewClassName}, {parameterClassName}>" : $"BaseViewUI<{viewClassName}>") 
                : (parameter ? $"BaseUI<{parameterClassName}>" : "BaseUI");

            builder.AppendLine($"{indent}public class {className} : {baseClass}");
            builder.AppendLine($"{indent}{{");
            builder.AppendLine($"{indent}");
            builder.AppendLine($"{indent}}}");


            // Close namespace if specified
            if (hasNamespace)
            {
                builder.AppendLine($"}}");
            }

            return builder.ToString();
        }
        public static void GenerateUICode(string filePath, string className, string namespaceName = null, bool parameter = false, bool view = false)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception($"Output path cannot be empty.");
            }

            if (File.Exists(filePath))
            {
                throw new Exception($"File already exists at path: {filePath}");
            }

            string scriptContent = GenerateUIContent(className, namespaceName, parameter, view);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, scriptContent);
            AssetDatabase.Refresh();

            Debug.Log($"Script generated at path: {filePath}");
        }


        private static bool IsValidCSharpIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (!char.IsLetter(name[0]) && name[0] != '_')
                return false;

            for (int i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                    return false;
            }

            string[] csharpKeywords = new string[]
            {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this",
            "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
            "using", "virtual", "void", "volatile", "while"
            };

            if (csharpKeywords.Contains(name.ToLower()))
                return false;

            return true;
        }
        private static void ValidateClassName(string scriptName) {
            if (string.IsNullOrWhiteSpace(scriptName)) {
                throw new Exception("Script name cannot be empty.");
            }

            if (!char.IsLetter(scriptName[0]) && scriptName[0] != '_') {
                throw new Exception("Script name must start with a letter or underscore.");
            }

            foreach (char c in scriptName) {
                if (!char.IsLetterOrDigit(c) && c != '_') {
                    throw new Exception("Script name can only contain letters, digits, or underscores.");
                }
            }

            if (!IsValidCSharpIdentifier(scriptName))
            {
                throw new InvalidOperationException($"Invalid names (must be valid C# identifiers): {scriptName}");
            }
        }
        private static void ValidateType(string className, string namespaceName)
        {
            var fullTypeName = string.IsNullOrWhiteSpace(namespaceName)
                ? className
                : $"{namespaceName}.{className}";

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType(fullTypeName) != null)
                    throw new Exception($"Type '{fullTypeName}' already exists in assembly '{assembly.FullName}'.");


                if (string.IsNullOrWhiteSpace(namespaceName) && assembly.GetType(className) != null)
                    throw new Exception($"Type '{className}' already exists in assembly '{assembly.FullName}'.");

            }
        }



        public static List<Type> GetInstanceTypes<T>()
        {
            var targetType = typeof(T);

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch
                    {
                        return Enumerable.Empty<Type>();
                    }
                })
                .Where(type =>
                    targetType.IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsInterface &&
                    type != targetType)
                .Where(p => p != null)
                .ToList();
            return types;
        }


        public class CreatePrefabOptions
        {
            public string prefabName;
            public string outputPath;
            public ViewSettings settings;
            public Type animType;
            public bool createMask;
            public bool createRoot;
        }

        public static GameObject GeneratePrefab(CreatePrefabOptions option)
        {
            var prefabName = option.prefabName;
            var outputPath = option.outputPath;
            var settings = option.settings;
            var animationType = option.animType;
            var createMask = option.createMask;
            var createRoot = option.createRoot;

            ValidatePrefabParameters(prefabName, outputPath);

            // Create directory if it doesn't exist
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // Create GameObject with RectTransform
            var go = new GameObject(prefabName);
            var goRectTransform = go.AddComponent<RectTransform>();
            goRectTransform.anchorMin = Vector2.zero;
            goRectTransform.anchorMax = Vector2.one;
            goRectTransform.sizeDelta = Vector2.zero;
            goRectTransform.anchoredPosition = Vector2.zero;

            // Add ViewType
            var view = go.AddComponent(typeof(BaseView)) as BaseView;
            view.Setting = settings;

            // Add animation component if specified
            if (animationType != null)
            {
                ValidateAnimationType(animationType);
                go.AddComponent(animationType);
            }

            // Create mask child if option is enabled
            if (createMask)
            {
                CreateMaskChild(go.transform);
            }

            if (createRoot)
            {
                CreateRootChild(go.transform);
            }

            // Save as prefab
            var fullPath = $"{outputPath}/{prefabName}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, fullPath);
            
            // Clean up the GameObject in the scene
            UnityEngine.Object.DestroyImmediate(go);

            // Refresh AssetDatabase
            AssetDatabase.Refresh();

            Debug.Log($"Prefab generated at path: {fullPath}");

            return prefab;
        }

        private static void ValidatePrefabParameters(string prefabName, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                throw new Exception("Prefab name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new Exception("Output path cannot be empty.");
            }

            // Validate prefab name doesn't contain invalid characters
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (prefabName.IndexOfAny(invalidChars) >= 0)
            {
                throw new Exception("Prefab name contains invalid characters.");
            }
            var prefabFullPath = $"{outputPath}/{prefabName}.prefab";

            if (File.Exists(prefabFullPath))
            {
                throw new Exception($"Prefab already exists at path: {prefabFullPath}");
            }
        }

        private static void ValidateAnimationType(Type animationType)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(animationType))
            {
                throw new Exception($"Animation type '{animationType.Name}' must inherit from MonoBehaviour.");
            }

            if (!typeof(IUIAnimation).IsAssignableFrom(animationType))
            {
                throw new Exception($"Animation type '{animationType.Name}' must implement IUIAnimation interface.");
            }

            if (animationType.IsAbstract || animationType.IsInterface)
            {
                throw new Exception($"Animation type '{animationType.Name}' cannot be abstract or interface.");
            }
        }

        private static void CreateMaskChild(Transform parent)
        {
            var child = new GameObject("Mask");
            child.transform.SetParent(parent);
            
            // Set RectTransform to stretch
            var rectTransform = child.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add Image component
            var image = child.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0.75f);
        }

        private static void CreateRootChild(Transform parent)
        {
            var child = new GameObject("Root");
            child.transform.SetParent(parent);

            // Set RectTransform to stretch
            var rectTransform = child.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }

    }

}