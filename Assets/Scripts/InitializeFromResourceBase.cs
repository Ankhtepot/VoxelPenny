using System;
using System.Reflection;
using UnityEngine;

public abstract class InitializeFromResourceBase : MonoBehaviour
{
    protected virtual void Awake()
    {
        AssignInitializeFromResourceFields();
    }

    private void AssignInitializeFromResourceFields()
    {
        FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            if (!(Attribute.GetCustomAttribute(field, typeof(InitializeFromResource), false) is
                    InitializeFromResource attribute)) continue;

            Debug.Log($"Detected attribute \"{nameof(InitializeFromResource)}\" " +
                      $"on a field: {field} " +
                      $"and source path is: {attribute.SourcePath}.");

            GameObject prefab = Resources.Load<GameObject>(attribute.SourcePath);

            if (!prefab)
            {
                throw new ArgumentException("Prefab was not loaded on given path.");
            }

            if (!((attribute.RequiredPrefabType).IsSubclassOf(typeof(Component))))
            {
                throw new ArgumentException("Required type on prefab is not inheriting from Component class");
            }

            if (attribute.RequiredPrefabType != typeof(MonoBehaviour) &&
                !prefab.GetComponent(attribute.RequiredPrefabType))
            {
                throw new ArgumentException("Loaded prefab does not contain required component.");
            }

            field.SetValue(this, prefab);

            if (!attribute.InstantiateImmediately) continue;

            Instantiate(prefab);
        }
    }
}