using UnityEngine;

[HasEditor("Assets/DataAssets/Examples", new string[] {"a", "d"})]
public class ExampleScriptableObject : ScriptableObject
{
    public int a;
    public float b;
    [Range(0f, 100f)] public float c;
    [Filterable] public string d;
}
