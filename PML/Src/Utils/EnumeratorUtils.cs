using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PML
{
    static class EnumeratorUtils
    {
        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            return ScrambledEquals(list1, list2, EqualityComparer<T>.Default);
        }

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2, IEqualityComparer<T> comparer)
        {
            var cnt = new Dictionary<T, int>(comparer);
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else {
                    cnt.Add(s, 1);
                }
            }

            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        public static int GetOrderIndependentHashCode<T>(IEnumerable<T> source)
        {
            int hash = 0;
            foreach (T element in source)
            {
                hash = hash ^ EqualityComparer<T>.Default.GetHashCode(element);
            }
            return hash;
        }

        public static int GetOrderIndependentHashCode<T>(IEnumerable<T> source, IEqualityComparer<T> comparar)
        {
            int hash = 0;
            foreach (T element in source)
            {
                hash = hash ^ comparar.GetHashCode(element);
            }
            return hash;
        }

        public static int GetOrderDependentHashCode<T>(IEnumerable<T> source)
        {
            if (source.Count() == 0)
                return 0;

            unchecked
            {
                int hash = 17;
                foreach (T element in source)
                {
                    hash = hash * 31 + EqualityComparer<T>.Default.GetHashCode(element);
                }
                return hash;
            }
        }

        public static int GetOrderDependentHashCode<T>(IEnumerable<T> source, IEqualityComparer<T> comparar)
        {
            if (source.Count() == 0)
                return 0;

            unchecked
            {
                int hash = 17;
                foreach (T element in source)
                {
                    hash = hash * 31 + comparar.GetHashCode(element);
                }
                return hash;
            }
        }
    }
}
