using System;
using UnityEngine;

public static class GameObjectExtensions
{
    public static void DestroyAll(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public static void ForEachChildDo(this Transform transform, Action<Transform> action)
    {
        foreach (Transform child in transform)
        {
            action(child);
        }
    }
}
