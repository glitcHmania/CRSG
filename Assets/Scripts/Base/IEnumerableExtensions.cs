using System;
using System.Collections.Generic;

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
    }
}
