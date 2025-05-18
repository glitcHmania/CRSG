using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Base
{
    public static class IEnumerableExtensions
    {
        public static void ForEachDo<T>(this IEnumerable<T> enumerable, Action<T> action) 
        {
            foreach (T child in enumerable)
            {
                action.Invoke(child);
            }
        }

        public static void DestroyAll(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}
