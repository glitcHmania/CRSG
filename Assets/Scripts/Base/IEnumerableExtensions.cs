using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
