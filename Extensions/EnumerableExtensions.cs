using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamAccountCreator.Extensions
{
    /// <summary>
    /// Enumerable extensions.
    /// </summary>
    public static class EnumerableExtensions
    {
        static Random random;

        /// <summary>
        /// Checks wether the collection is empty.
        /// </summary>
        /// <param name="enumerable">The collection.</param>
        /// <returns>True if the collection is empty, false otherwise.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return true;
            }

            return enumerable.Count() < 1;
        }

        /// <summary>
        /// Gets a random element.
        /// </summary>
        /// <returns>The element.</returns>
        /// <param name="enumerable">Enumerable.</param>
        public static T GetRandomElement<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable.IsNullOrEmpty())
            {
                throw new NullReferenceException();
            }

            if (random == null)
            {
                random = new Random();
            }

            return enumerable.ElementAt(random.Next(enumerable.Count()));
        }

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            if (list.IsNullOrEmpty())
            {
                throw new NullReferenceException();
            }

            if (random == null)
            {
                random = new Random();
            }
            
            List<T> shuffled = list.ToList();
            int n = shuffled.Count();

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);

                T value = shuffled[k];
                shuffled[k] = shuffled[n];
                shuffled[n] = value;
            }

            return shuffled;
        }
    }
}