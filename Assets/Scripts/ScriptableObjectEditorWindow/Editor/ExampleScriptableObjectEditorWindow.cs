using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ExampleScriptableObjectEditorWindow : ScriptableObjectEditorWindow<ExampleScriptableObject>
{
    [MenuItem("Editors/Example")]
    public static void OpenExampleWindow()
    {
        ExampleScriptableObjectEditorWindow window = GetWindow<ExampleScriptableObjectEditorWindow>();
        window.titleContent = new GUIContent("Example Editor");
    }


    protected override string GetFileName()
    {
        return $"{templateData.d}{templateData.a}";
    }

    protected override string GetSavePath()
    {
        return "Assets/DataAssets/Examples";
    }

}
