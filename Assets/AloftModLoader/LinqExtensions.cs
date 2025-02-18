using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AloftModLoader
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
            {
                action(element);
            }
            return source;
        }
        
        public static IEnumerable<T> FilterAndCast<T>(this IEnumerable<UnityEngine.Object> source)
        {
            return source.Where(x => x is T).Cast<T>();
        }
    }
}
