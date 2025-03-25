using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extensions : MonoBehaviour
{
    static public T GetFirstComponentInAncestor<T>(Transform child) where T : Component
    {
        Transform parent = child.parent; // Start from the parent

        while (parent != null)
        {
            T component = parent.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
            parent = parent.parent; // Move up in the hierarchy
        }

        return null; // Return null if no ancestor has the component
    }
}
