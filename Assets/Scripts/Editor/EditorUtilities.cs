using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class EditorUtilities
{
    private const BindingFlags AllBindingFlags = (BindingFlags)(-1);

    /// <summary>
    /// Returns attributes of type <typeparamref name="TAttribute"/> on <paramref name="serializedProperty"/>.
    /// </summary>
    public static TAttribute[] GetAttributes<TAttribute>(this SerializedProperty serializedProperty, bool inherit)
        where TAttribute : Attribute
    {
        if (serializedProperty == null)
        {
            throw new ArgumentNullException(nameof(serializedProperty));
        }

        var targetObjectType = serializedProperty.serializedObject.targetObject.GetType();

        if (targetObjectType == null)
        {
            throw new ArgumentException($"Could not find the {nameof(targetObjectType)} of {nameof(serializedProperty)}");
        }

        foreach (var pathSegment in serializedProperty.propertyPath.Split('.'))
        {
            var fieldInfo = targetObjectType.GetField(pathSegment, AllBindingFlags);
            if (fieldInfo != null)
            {
                return (TAttribute[])fieldInfo.GetCustomAttributes<TAttribute>(inherit);
            }

            var propertyInfo = targetObjectType.GetProperty(pathSegment, AllBindingFlags);
            if (propertyInfo != null)
            {
                return (TAttribute[])propertyInfo.GetCustomAttributes<TAttribute>(inherit);
            }
        }

        throw new ArgumentException($"Could not find the field or property of {nameof(serializedProperty)}");
    }

    public static bool HasAttribute<TAttribute>(this SerializedProperty serializedProperty, bool inherit)
        where TAttribute : Attribute
    {
        if (serializedProperty == null)
        {
            throw new ArgumentNullException(nameof(serializedProperty));
        }

        var targetObjectType = serializedProperty.serializedObject.targetObject.GetType();

        if (targetObjectType == null)
        {
            throw new ArgumentException($"Could not find the {nameof(targetObjectType)} of {nameof(serializedProperty)}");
        }

        foreach (var pathSegment in serializedProperty.propertyPath.Split('.'))
        {
            var fieldInfo = targetObjectType.GetField(pathSegment, AllBindingFlags);
            if (fieldInfo != null)
            {
                return fieldInfo.GetCustomAttributes<TAttribute>(inherit).ToList().Count > 0;
            }

            var propertyInfo = targetObjectType.GetProperty(pathSegment, AllBindingFlags);
            if (propertyInfo != null)
            {
                return propertyInfo.GetCustomAttributes<TAttribute>(inherit).ToList().Count > 0;
            }
        }
        return false;
    }

    public static object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        if (prop == null) return null;

        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }
        }
        return obj;
    }

    private static object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }
    private static object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();
        //while (index-- >= 0)
        //    enm.MoveNext();
        //return enm.Current;

        for (int i = 0; i <= index; i++)
        {
            if (!enm.MoveNext()) return null;
        }
        return enm.Current;
    }
}

