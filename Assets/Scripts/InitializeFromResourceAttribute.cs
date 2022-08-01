using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class InitializeFromResource : Attribute
{
    public readonly string SourcePath;
    public readonly bool InstantiateImmediately;
    public Type RequiredPrefabType;

    public InitializeFromResource(string resourcePath = "") : this(resourcePath, typeof(MonoBehaviour), false)
    {
    }

    public InitializeFromResource(string resourcePath, Type requiredPrefabType) : this(resourcePath,
        typeof(MonoBehaviour), false)
    {
    }

    public InitializeFromResource(string resourcePath, Type requiredPrefabType, bool instantiateImmediately)
    {
        SourcePath = resourcePath;
        InstantiateImmediately = instantiateImmediately;
        RequiredPrefabType = requiredPrefabType;
    }
}