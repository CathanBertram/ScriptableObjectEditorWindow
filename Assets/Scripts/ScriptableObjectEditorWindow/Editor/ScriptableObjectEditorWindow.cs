

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.Rendering;
using static Codice.CM.Common.CmCallContext;
using Unity.VisualScripting;

public abstract class ScriptableObjectEditorWindow<T> : EditorWindow where T : ScriptableObject
{
    VisualElement rightPane;
    protected T templateData;
    protected List<T> dataList;
    protected List<T> displayList;
    protected ListView assetList;
    protected SerializedProperty filteredProperty;
    private bool filterAssetName;
    public void CreateGUI()
    {
        var type = typeof(T);
        var GUIDS = AssetDatabase.FindAssets($"t:{type}");
        dataList = new List<T>();

        foreach (var guid in GUIDS)
        {
            dataList.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
        }
        displayList = dataList;

        // TOP BAR
        var topBar = new TwoPaneSplitView(0, 20, TwoPaneSplitViewOrientation.Vertical);
        var current = rootVisualElement;
        current.Add(topBar);
        current = topBar;
        // ADD SEARCH TO TOP BAR
        var searchField = new ToolbarSearchField();
        searchField.RegisterValueChangedCallback(x => OnSearch(x.newValue));
        current = AddAsSplitView(current, searchField, 0, 250, TwoPaneSplitViewOrientation.Horizontal);
        //current.Add(new VisualElement());
        // ADD FILTERS
        AddFilters(current);

        // END TOP BAR

        // MAIN CONTENT SPLIT VIEW
        var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        topBar.Add(splitView);
        current = splitView;

        // ADD CREATE NEW BUTTON TO LEFT PANE
        var button = new Button(() => { DisplayNewTCreation(); }) { text = $"New {type}" };
        current = AddAsSplitView(current, button);

        // ADD LIST VIEW TO LEFT PANE
        assetList = new ListView();
        assetList.onSelectionChange += OnTSelectionChange;
        assetList.makeItem = () => new Label();
        assetList.bindItem = (item, index) => { (item as Label).text = displayList[index].name; };
        assetList.itemsSource = displayList;
        current.Add(assetList);

        // ADD RIGHT PANE
        rightPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
        splitView.Add(rightPane);

        FilterAssetName();
    }
    
    private void DisplayNewTCreation()
    {
        rightPane.Clear();

        // Initialize Scriptable Object Instance
        templateData = ScriptableObject.CreateInstance<T>();
        // Get As Serialized Object
        var serializedObject = new SerializedObject(templateData);

        // Create Inspector Element For SerializedObject
        var inspectorElement = new InspectorElement();
        inspectorElement.Bind(serializedObject);

        // Add Inspector Element as Element 0 of Splitview Using Inspector Element Height
        var curr = AddAsSplitView(rightPane, inspectorElement, 0, inspectorElement.contentRect.height);

        // Add Button To Create New Scriptable Object
        var button = new Button(() => { CreateT(); }) { text = "Create Scriptable Object" };

        // Add Button as Element 0  of Splitview
        curr = AddAsSplitView(curr, button);
        curr.Add(new VisualElement());
    }

    private void CreateT()
    {
        var path = GetSavePath();
        ValidateFolders(path);

        AssetDatabase.CreateAsset(templateData, $"{GetSavePath()}/{GetFileName()}.asset");
        AssetDatabase.SaveAssets();

        dataList.Add(AssetDatabase.LoadAssetAtPath<T>($"{GetSavePath()}/{GetFileName()}.asset"));
        assetList.Rebuild();

        DisplayNewTCreation();
    }
    private void OnTSelectionChange(IEnumerable<object> selectedItems)
    {
        rightPane.Clear();
        var enumerator = selectedItems.GetEnumerator();
        if (enumerator.MoveNext())
        {
            var selectedT = enumerator.Current as T;
            if (selectedT != null)
            {
                var serializedT = new SerializedObject(selectedT);

                var inspector = new InspectorElement();
                inspector.Bind(serializedT);
                //rightPane.Add(inspector);
                var currRight = AddAsSplitView(rightPane, inspector, 0, inspector.contentRect.height);
                var button = new Button(() => { DeleteCurrentAsset(selectedT); }) { text = "Delete Asset" };
                currRight = AddAsSplitView(currRight, button);
                currRight.Add(new VisualElement());            
                
            }
        }
    }
    private void DeleteCurrentAsset(T assetToDelete)
    {
        dataList.Remove(assetToDelete);
        displayList.Remove(assetToDelete);

        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(assetToDelete));

        assetList.Rebuild();

        rightPane.Clear();
    }
    private void ValidateFolders(string path)
    {
        var pathParts = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries);
        string tempPath = "";
        tempPath += $"{pathParts[0]}";
        for (int i = 1; i < pathParts.Length; i++)
        {
            if (!AssetDatabase.IsValidFolder($"{tempPath}/{pathParts[i]}"))
                AssetDatabase.CreateFolder(tempPath, pathParts[i]);
            tempPath += $"/{pathParts[i]}";
        }
    }

    private TwoPaneSplitView AddAsSplitView(VisualElement parent, VisualElement initialElement, int fixedPaneIndex = 0, float fixedPaneStartDimensions = 25f, TwoPaneSplitViewOrientation orientation = TwoPaneSplitViewOrientation.Vertical)
    {
        var splitView = GetAsSplitView(initialElement, fixedPaneIndex, fixedPaneStartDimensions, orientation);
        parent.Add(splitView);

        return splitView;
    }
    private TwoPaneSplitView GetAsSplitView(VisualElement initialElement, int fixedPaneIndex = 0, float fixedPaneStartDimensions = 25f, TwoPaneSplitViewOrientation orientation = TwoPaneSplitViewOrientation.Vertical)
    {
        var tempSplitView = new TwoPaneSplitView(fixedPaneIndex, fixedPaneStartDimensions, orientation);
        tempSplitView.Add(initialElement);
        return tempSplitView;
    }
    private void AddFilters(VisualElement currentElement)
    {
        Label filterLabel = new Label();
        filterLabel.text = "Filters:";
        currentElement = AddAsSplitView(currentElement, filterLabel, 0, filterLabel.contentRect.width, TwoPaneSplitViewOrientation.Horizontal);

        var assetNameFilter = new Button(() => { FilterAssetName(); }) { text = $"Asset Name" };
        currentElement = AddAsSplitView(currentElement, assetNameFilter, 0, assetNameFilter.contentRect.width, TwoPaneSplitViewOrientation.Horizontal);

        templateData = ScriptableObject.CreateInstance<T>();
        SerializedObject serializedObject = new SerializedObject(templateData);

        var iterator = serializedObject.GetIterator();

        iterator.Next(true);
        while (iterator.Next(false))
        {
            if (iterator.HasAttribute<FilterableAttribute>(false))
            {
                var copy = iterator.Copy();
                var button = new Button(() => { ChangeFilteredProperty(copy); }) { text = $"{iterator.displayName}" };
                currentElement = AddAsSplitView(currentElement, button, 0 , button.contentRect.width, TwoPaneSplitViewOrientation.Horizontal);
            }
        }
        currentElement.Add(new VisualElement());
    }
    private void ChangeFilteredProperty(SerializedProperty property)
    {
        filteredProperty = property;
        filterAssetName = false;
    }

    private void FilterAssetName()
    {
        filterAssetName = true;
    }
    private void OnSearch(string value)
    {
        if(filterAssetName)
        {
            SearchAssetName(value);
            return;
        }

        if (filteredProperty == null) return;

        var targetObject = filteredProperty.serializedObject.targetObject;
        var targetObjectClassType = targetObject.GetType();
        var field = targetObjectClassType.GetField(filteredProperty.propertyPath);


        if (field == null) return;

        displayList = dataList.Where(x => field.GetValue(x).ToString().Contains(value)).ToList();
        assetList.itemsSource = displayList;
        assetList.Rebuild();
    }

    private void SearchAssetName(string value)
    {
        displayList = dataList.Where(x => x.name.Contains(value)).ToList();
        assetList.itemsSource = displayList;
        assetList.Rebuild();
    }

    protected abstract string GetSavePath();
    protected abstract string GetFileName();
}
