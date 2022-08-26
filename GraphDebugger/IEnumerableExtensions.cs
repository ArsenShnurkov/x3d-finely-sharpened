using System;
using System.Collections.Generic;
using System.Linq;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random r)
    {
        var elements = source.ToArray();
        for (var i = elements.Length - 1; i >= 0; i--)
        {
            var swapIndex = r.Next(i + 1);
            yield return elements[swapIndex];
            elements[swapIndex] = elements[i];
        }
    }
}