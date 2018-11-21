using System;
using System.Collections.Generic;
using System.Linq;

namespace Cronical.Misc
{
    public static class ListHelper
    {
        public class Intersection<T>
        {
            public readonly List<T> Left;
            public readonly List<T> Both;
            public readonly List<T> Right;

            public Intersection()
            {
                Left = new List<T>();
                Both = new List<T>();
                Right = new List<T>();
            }
        }

        /// <summary>
        /// Add a value to a list if the value is not null.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List to add values to</param>
        /// <param name="value">Value to add</param>
        public static void AddIfNotNull<T>(this List<T> list, T value) where T : class
        {
            if (list != null && value != null)
                list.Add(value);
        }

        /// <summary>
        /// Add a range of values to a list, checking to see if the values or the enumerable is null.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List to add values to</param>
        /// <param name="values">Values to add</param>
        public static void AddRangeIfNotNull<T>(this List<T> list, IEnumerable<T> values) where T : class
        {
            if (list != null && values != null)
                list.AddRange(values.Where(x => x != null));
        }

        /// <summary>
        /// Extract all values matching a predicate from the list, return a new list of values
        /// and removing them from the original list.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List to extract values from</param>
        /// <param name="match">Predicate condition to satisfy</param>
        /// <returns>The extracted list of values</returns>
        public static List<T> ExtractAll<T>(this List<T> list, Predicate<T> match)
        {
            var result = list.Where(x => match(x)).ToList();

            list.RemoveAll(match);

            return result;
        }

        /// <summary>
        /// Extract a particular item from a position in the list, removing it from the list in the process.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List to extract the value from</param>
        /// <param name="item">0-based position to extract</param>
        /// <returns>The extracted value. If the index is outside of the list bounds, an exception will be thrown</returns>
        public static T ExtractAt<T>(this IList<T> list, int item)
        {
            var result = list[item];
            list.RemoveAt(item);

            return result;
        }

        /// <summary>
        /// Extract a particular item from a position in the list, removing it from the list in the process.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List to extract the value from</param>
        /// <param name="item">0-based position to extract</param>
        /// <returns>The extracted value, or default(T) if the value is outside of the list bounds.</returns>
        public static T ExtractAtOrDefault<T>(this IList<T> list, int item)
        {
            if (item < 0 || item >= list.Count)
                return default(T);

            var result = list[item];
            list.RemoveAt(item);

            return result;
        }

        /// <summary>
        /// Extract (and remove) the first value from the list
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List to extract the value from</param>
        /// <returns>The first value from the list. If the list is empty, an exception will be thrown.</returns>
        public static T ExtractFirst<T>(this IList<T> list)
        {
            return ExtractAt(list, 0);
        }

        /// <summary>
        /// Extract (and remove) the first count values from the list
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List to extract the value from</param>
        /// <param name="count">The number of values to return</param>
        /// <returns>The first count number of values from the list. If the list contains 
        /// less than count items, only those will be returned.</returns>
        public static List<T> ExtractFirstCount<T>(this List<T> list, int count)
        {
            if (count > list.Count)
                count = list.Count;

            var result = list.GetRange(0, count);
            list.RemoveRange(0, count);
            return result;
        }

        /// <summary>
        /// Extract (and remove) the first value from the list
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List to extract the value from</param>
        /// <returns>The first value from the list. If the list is empty, default(T) will be returned.</returns>
        public static T ExtractFirstOrDefault<T>(this IList<T> list)
        {
            return list.Any() ? ExtractAt(list, 0) : default(T);
        }

        /// <summary>
        /// Extract (and remove) the last value from the list
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List to extract the value from</param>
        /// <returns>The last value from the list. If the list is empty, an exception will be thrown.</returns>
        public static T ExtractLast<T>(this IList<T> list)
        {
            return ExtractAt(list, list.Count - 1);
        }

        /// <summary>
        /// Extract (and remove) the last count values from the list
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List to extract the value from</param>
        /// <param name="count">The number of values to return</param>
        /// <returns>The last count number of values from the list. If the list contains 
        /// less than count items, only those will be returned.</returns>
        public static List<T> ExtractLastCount<T>(this List<T> list, int count)
        {
            if (count > list.Count)
                count = list.Count;

            var result = list.GetRange(list.Count - count, count);
            list.RemoveRange(list.Count - count, count);
            return result;
        }

        /// <summary>
        /// Extract (and remove) the last value from the list
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List to extract the value from</param>
        /// <returns>The first last from the list. If the list is empty, default(T) will be returned.</returns>
        public static T ExtractLastOrDefault<T>(this IList<T> list)
        {
            return list.Any() ? ExtractAt(list, list.Count - 1) : default(T);
        }

        /// <summary>
        /// Compare two lists against each other and return an Intersection result from the comparison, 
        /// listing the objects found in only list1, only list2, or both lists.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list1">The first list (left)</param>
        /// <param name="list2">The second list (right)</param>
        /// <returns>An Intersection object with the results of the comparison</returns>
        public static Intersection<T> Intersect<T>(IList<T> list1, IList<T> list2)
        {
            return Intersect(list1, list2, (Comparison<T>)null);
        }

        /// <summary>
        /// Compare two lists against each other and return an Intersection result from the comparison, 
        /// listing the objects found in only list1, only list2, or both lists.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list1">The first list (left)</param>
        /// <param name="list2">The second list (right)</param>
        /// <param name="comparer">A specific comparer to use for comparing the objects</param>
        /// <returns>An Intersection object with the results of the comparison</returns>
        public static Intersection<T> Intersect<T>(IList<T> list1, IList<T> list2, IComparer<T> comparer)
        {
            return Intersect(list1, list2, comparer.Compare);
        }

        /// <summary>
        /// Compare two lists against each other and return an Intersection result from the comparison, 
        /// listing the objects found in only list1, only list2, or both lists.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list1">The first list (left)</param>
        /// <param name="list2">The second list (right)</param>
        /// <param name="comparison">A comparison method for comparing the objects</param>
        /// <returns>An Intersection object with the results of the comparison</returns>
        public static Intersection<T> Intersect<T>(IList<T> list1, IList<T> list2, Comparison<T> comparison)
        {
            var result = new Intersection<T>();

            bool empty1 = list1 == null || list1.Count == 0;
            bool empty2 = list2 == null || list2.Count == 0;

            if (empty1 && empty2)
                return result;

            if (empty1)
            {
                result.Right.AddRange(list2);
                return result;
            }

            if (empty2)
            {
                result.Left.AddRange(list1);
                return result;
            }

            var search2 = new List<T>(list2);

            // Divide array1 into Left and Both
            foreach (var item1 in list1)
            {
                var n = search2.FindIndex(x => DoCompare(comparison, item1, x));
                if (n == -1)
                {
                    result.Left.Add(item1);
                }
                else
                {
                    result.Both.Add(item1);
                    search2.RemoveAt(n);
                }
            }

            // Any remaining items in array2 (=search2) must now fall to right.
            result.Right.AddRange(search2);

            return result;
        }

        private static bool DoCompare<T>(Comparison<T> comparison, T item1, T item2)
        {
            if (comparison != null)
                return comparison(item1, item2) == 0;
            if (item1 is IComparable<T>)
                return ((IComparable<T>)item1).CompareTo(item2) == 0;
            if (item1 is IEquatable<T>)
                return ((IEquatable<T>)item1).Equals(item2);

            return Comparer<T>.Default.Compare(item1, item2) == 0;
        }
    }
}
