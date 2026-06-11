using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HUI.Editor
{
    public class UIStatisticsTab : VisualElement
    {

        private string searchText = "";
        private int sortMode = 0;


        private UISettings settings;

        public void Init(UISettings settings)
        {
            this.settings = settings;

            CreateElement();
            RefreshStats();
        }

        private void CreateElement()
        {
            var container = this;
            container.style.flexGrow = 1;
            container.style.marginTop = 10;

            // Statistics Section
            var headerContainer = new VisualElement();
            headerContainer.style.flexShrink = 0;
            headerContainer.style.flexGrow = 0;
            headerContainer.style.paddingLeft = 10;
            headerContainer.style.paddingRight = 10;
            headerContainer.style.paddingTop = 10;
            headerContainer.style.paddingBottom = 5;
            headerContainer.style.marginBottom = 5;
            headerContainer.style.marginLeft = 10;
            headerContainer.style.marginRight = 10;
            headerContainer.style.borderTopLeftRadius = 5;
            headerContainer.style.borderTopRightRadius = 5;
            headerContainer.style.borderBottomLeftRadius = 5;
            headerContainer.style.borderBottomRightRadius = 5;
            headerContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.2f);
            container.Add(headerContainer);

            var title = new Label("Statistics");
            title.style.fontSize = 14;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 10;
            headerContainer.Add(title);

            var statsContainer = new VisualElement();
            statsContainer.name = "stats-container";
            statsContainer.style.flexDirection = FlexDirection.Row;
            statsContainer.style.flexWrap = Wrap.Wrap;
            headerContainer.Add(statsContainer);


            // Views Section
            var viewsContainer = new VisualElement();
            viewsContainer.style.paddingLeft = 10;
            viewsContainer.style.paddingRight = 10;
            viewsContainer.style.paddingTop = 10;
            viewsContainer.style.paddingBottom = 5;
            viewsContainer.style.marginBottom = 10;
            viewsContainer.style.marginLeft = 10;
            viewsContainer.style.marginRight = 10;
            viewsContainer.style.borderTopLeftRadius = 5;
            viewsContainer.style.borderTopRightRadius = 5;
            viewsContainer.style.borderBottomLeftRadius = 5;
            viewsContainer.style.borderBottomRightRadius = 5;
            viewsContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.2f);
            container.Add(viewsContainer);



            // Search Field
            var searchContainer = new VisualElement();
            searchContainer.style.flexDirection = FlexDirection.Row;
            searchContainer.style.marginBottom = 10;
            viewsContainer.Add(searchContainer);

            var searchLabel = new Label("Filter:");
            searchLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            searchLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            searchLabel.style.minWidth = 40;
            searchLabel.style.marginRight = 5;
            searchContainer.Add(searchLabel);

            var searchField = new TextField();
            searchField.value = searchText;
            searchField.style.flexGrow = 1;
            searchField.style.marginRight = 10;
            searchField.RegisterValueChangedCallback(evt =>
            {
                searchText = evt.newValue;
                RefreshViews();
            });
            searchContainer.Add(searchField);

            var sortLabel = new Label("Sort:");
            sortLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            sortLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            sortLabel.style.minWidth = 35;
            sortLabel.style.marginRight = 5;
            searchContainer.Add(sortLabel);

            var sortDropdown = new DropdownField(new List<string> { "By Group", "By Name" }, sortMode);
            sortDropdown.style.width = 100;
            sortDropdown.RegisterValueChangedCallback(evt =>
            {
                sortMode = sortDropdown.index;
                RefreshViews();
            });
            searchContainer.Add(sortDropdown);

            // Scrollable Content
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            viewsContainer.Add(scrollView);

            // Views Content
            var viewsContent = new VisualElement();
            viewsContent.name = "views-content";
            viewsContent.style.paddingBottom = 10;
            scrollView.Add(viewsContent);
        }

        public void RefreshStats()
        {
            var statsContainer = this.Q<VisualElement>("stats-container");
            statsContainer.Clear();

            var views = UIValidator.GetViewPaths(settings.prefabPath).Values.ToList();
            var groupCount = settings.groups?.Count ?? 0;
            var viewCount = views.Count;
            var missingGroupViews = views.Where(v => v.Setting.group < 0 || v.Setting.group >= groupCount).Count();

            AddStatItem(statsContainer, "Total Groups", groupCount.ToString(), new Color(0.3f, 0.6f, 0.9f));
            AddStatItem(statsContainer, "Total Views", viewCount.ToString(), new Color(0.5f, 0.7f, 0.4f));
            AddStatItem(statsContainer, "Missing Used", missingGroupViews.ToString(), new Color(0.9f, 0.6f, 0.3f));

            var result = UIValidator.ValidateUIPath(settings.prefabPath);
            var item = AddStatItem(statsContainer, "Missing Paths", result.MissingPrefabUIPaths.Count.ToString(), new Color(0.9f, 0.6f, 0.3f));
            item.tooltip = string.Join("\n", result.MissingPrefabUIPaths);

            item = AddStatItem(statsContainer, "Unmarked Prefabs", result.UnmarkedPrefabs.Count.ToString(), new Color(0.9f, 0.6f, 0.3f));
            item.tooltip = string.Join("\n", result.UnmarkedPrefabs);

            item = AddStatItem(statsContainer, "Multiple Mapping", result.MultipleMapping.SelectMany(p => p.Value).Count().ToString(), new Color(0.9f, 0.6f, 0.3f));
            item.tooltip = string.Join("\n", result.MultipleMapping.SelectMany(kv => kv.Value.Select(t => $"[UIPath(\"{kv.Key}\")] -> {t.Name}")));
        }
        private VisualElement AddStatItem(VisualElement parent, string label, string value, Color color)
        {
            var statItem = new VisualElement();
            statItem.style.marginRight = 15;
            statItem.style.marginBottom = 5;
            parent.Add(statItem);

            var statLabel = new Label(label);
            statLabel.style.fontSize = 10;
            statLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            statItem.Add(statLabel);

            var statValue = new Label(value);
            statValue.style.fontSize = 16;
            statValue.style.unityFontStyleAndWeight = FontStyle.Bold;
            statValue.style.color = color;
            statItem.Add(statValue);

            return statItem;
        }

        public void RefreshViews()
        {
            var views = UIValidator.GetViewPaths(settings.prefabPath).Values.ToList();

            var viewsContainer = this.Q<VisualElement>("views-content");
            viewsContainer.Clear();

            if (views.Count == 0)
            {
                var noViewLabel = new Label("No views found in prefab path");
                noViewLabel.style.marginTop = 10;
                noViewLabel.style.fontSize = 12;
                noViewLabel.style.color = Color.grey;
                viewsContainer.Add(noViewLabel);
                return;
            }

            var groups = settings.groups;
            var hasSearchFilter = !string.IsNullOrWhiteSpace(searchText);

            // Apply search filter
            var filteredViews = views;
            if (hasSearchFilter)
            {
                filteredViews = views.Where(v => v.name.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            if (sortMode == 0) // By Group
            {
                var viewGroups = filteredViews.GroupBy(p => p.Setting.group).OrderBy(p =>
                {
                    if (p.Key >= 0 && p.Key < groups.Count)
                        return groups[p.Key].depth;
                    return 10000;
                });

                foreach (var list in viewGroups)
                {
                    var query = list.OrderBy(p => p.Setting.priority);

                    int i = 0;
                    foreach (var view in query)
                    {
                        AddUIItem(viewsContainer, view, i++);
                    }
                }
            }
            else // By Name
            {
                var sortedViews = filteredViews.OrderBy(v => v.name);

                int i = 0;
                foreach (var view in sortedViews)
                {
                    AddUIItem(viewsContainer, view, i++);
                }
            }
        }
        private VisualElement AddUIItem(VisualElement container, BaseView view,int i)
        {
            var path = AssetDatabase.GetAssetPath(view);

            var viewRow = new VisualElement();
            viewRow.name = view.name;
            viewRow.style.flexDirection = FlexDirection.Row;
            viewRow.style.paddingTop = 2;
            viewRow.style.paddingBottom = 2;

            container.Add(viewRow);

            var viewButton = new Button(() => Selection.activeObject = view)
            {
                text = view.name,
                tooltip = path,
            };
            viewButton.style.flexGrow = 1;
            viewButton.style.height = 22;
            viewButton.style.unityTextAlign = TextAnchor.MiddleLeft;
            viewButton.style.marginLeft = 0;
            viewButton.style.marginRight = 0;
            viewButton.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

            viewRow.Add(viewButton);

            var so = new SerializedObject(view);
            var settingProperty = so.FindProperty("setting");

            //Group Dropdown
            var groupProperty = settingProperty.FindPropertyRelative("group");
            var groups = settings.groups.Select(g => g.name).ToList();
            var groupField = new DropdownField(groups, groupProperty.intValue);
            groupField.style.width = 120;
            groupField.style.marginLeft = 0;
            groupField.style.marginRight = 0;
            groupField[0].style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            groupField.RegisterValueChangedCallback(evt => {
                groupProperty.intValue = groups.IndexOf(evt.newValue);
                groupProperty.serializedObject.ApplyModifiedProperties();
                RefreshViews();
            });
            groupField.TrackPropertyValue(groupProperty, evt =>
            {
                RefreshViews();
            });
            viewRow.Add(groupField);

            // Priority Dropdown
            var priorityProperty = settingProperty.FindPropertyRelative("priority");
            var priorityField = new EnumField(view.Setting.priority);
            priorityField.style.width = 100;
            priorityField.style.marginLeft = 0;
            priorityField[0].style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            priorityField.RegisterValueChangedCallback(evt => {
                priorityProperty.intValue = Convert.ToInt32(evt.newValue);
                priorityProperty.serializedObject.ApplyModifiedProperties();
                RefreshViews();
            });
            priorityField.TrackPropertyValue(priorityProperty, evt =>
            {
                RefreshViews();
            });
            viewRow.Add(priorityField);

            return viewRow;
        }
    }
}