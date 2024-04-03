
using UnityEngine;

public class ExampleScriptableObject : ScriptableObject
{
    public int a;
    public float b;
    [Range(0f, 100f)] public float c;
    public string d;
}
