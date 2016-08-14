using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Collections;

namespace Platform
{
	/// <summary>
	/// Provides extension methods and static utility methods for enumerable types.
	/// </summary>
	public static class EnumerableUtils
	{
		/// <summary>
		/// Returns an empty IEnumerable
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<T> Null<T>()
		{
			yield break;
		}

		/// <summary>
		/// Returns an enumerable without the last item 
		/// </summary>
		public static IEnumerable<T> DropLast<T>(this IEnumerable<T> source)
		{
			var value = default(T);
			var gotValue = false;

			foreach (var item in source)
			{
				if (gotValue)
				{
					yield return value;
				}

				value = item;
				gotValue = true;
			}
		}

		/// <summary>
		/// Returns an enumerable without the last <c>count</c> items.
		/// </summary>
		/// <typeparam name="T">The type of each item in source</typeparam>
		/// <param name="source">The enumerable</param>
		/// <param name="count">The number of items at the end of the enumerable to skip</param>
		/// <returns></returns>
		public static IEnumerable<T> DropLast<T>(this IEnumerable<T> source, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Argument {nameof(count)} should be non-negative");
			}

			var buffer = new Queue<T>(count + 1);

			foreach (var item in source)
			{
				buffer.Enqueue(item);

				if (buffer.Count == count + 1)
				{
					yield return buffer.Dequeue();
				}
			}
		}
		
		/// <summary>
		/// Returns a <see cref="ReadOnlyList{T}"/>
		/// </summary>
		/// <remarks>
		/// If the source is already a ReadOnlyList then the method returns the source.
		/// </remarks>
		/// <typeparam name="T">The element type of the source</typeparam>
		/// <param name="source">The source enumerable</param>
		/// <returns>A <see cref="ReadOnlyList{T}"/></returns>
		internal static ReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source)
		{
			if (source == null)
			{
				return null;
			}

			var list = source as ReadOnlyList<T>;

			return list ?? new ReadOnlyList<T>(source.ToList());
		}

		public static IEnumerable<T> Concat<T>(this IEnumerable<T> values, T value)
		{
			foreach (var obj in values)
			{
				yield return obj;
			}

			yield return value;
		}

        public static IEnumerable<T> ConcatUnlessNull<T>(this IEnumerable<T> values, T value)
        {
            foreach (var obj in values)
            {
                yield return obj;
            }

            if (value != null)
            {
                yield return value;
            }
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> values,  T value)
		{
			yield return value;

			foreach (var obj in values)
			{
				yield return obj;
			}
		}

        public static IEnumerable<T> PrependUnlessNull<T>(this IEnumerable<T> values, T value)
        {
            if (value != null)
            {
                yield return value;
            }

            foreach (var obj in values)
            {
                yield return obj;
            }
        }

        /// <summary>
        /// Compares two enumerables to see if their elements are equal
        /// </summary>
        /// <param name="left">The first enumerable to compare</param>
        /// <param name="right">The second enumerable to compare</param>
        /// <returns>
        /// True if the elements in both enumerables are equal
        /// </returns>
        public static bool ElementsAreEqual(this IEnumerable left, IEnumerable right)
		{
			var rightEnumerator = right.GetEnumerator();

			foreach (var value in left)
			{
				if (!rightEnumerator.MoveNext())
				{
					return false;
				}

				if (!object.Equals(value, rightEnumerator.Current))
				{
					return false;
				}
			}

			return !rightEnumerator.MoveNext();
		}

		/// <summary>
		/// Performs an action on each item in the supplied <see cref="IEnumerable"/>
		/// </summary>
		/// <param name="enumerables">The source of items</param>
		/// <param name="action">The action to perform on each item</param>
		public static void ForEach<T>(this IEnumerable<T> enumerables, Action<T> action)
		{
			foreach (var value in enumerables)
			{
				action(value);
			}
		}

		/// <summary>
		/// Prints all elements in the enumerable to the Console.
		/// </summary>
		/// <typeparam name="T">The type of enumerable</typeparam>
		/// <param name="enumerable">The enumerable to print</param>
		public static void Print<T>(this IEnumerable<T> enumerable)
		{
			ForEach(enumerable, value => Console.WriteLine(value) );
		}

		/// <summary>
		/// Joins all the elements in the enumerable as a string
		/// </summary>
		/// <typeparam name="T">The element type of the enumerable</typeparam>
		/// <param name="enumerable">The eumerator</param>
		/// <param name="binder">The string that will appear between elements</param>
		/// <returns>
		/// A string representation of the enumerable
		/// </returns>
		public static string JoinToString<T>(this IEnumerable<T> enumerable, string binder)
		{
			var builder = new StringBuilder();

			return enumerable.Select(c => Convert.ToString(c)).ComplexFold
			(
				delegate(string value)
				{
					if (builder.Length != 0)
					{
						builder.Append(binder);
					}

					builder.Append(value);

					return builder.ToString;
				}
			);
		}

		/// <summary>
		/// Fold all the elements in the enumerable using the given combiner
		/// </summary>
		/// <remarks>
		/// Each element in the enumerable will be passed to the combiner.  The combiner
		/// should return a function that on will evaluate to the current result if called.
		/// The function returned by the combiner will only be called once the last element
		/// of the enumerable has been evaluated by the combiner.
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable">The enumerable</param>
		/// <param name="combiner">The combiner</param>
		/// <returns>
		/// The result of the addition
		/// </returns>
		public static T ComplexFold<T>(this IEnumerable<T> enumerable, Func<T, Func<T>> combiner)
		{
			Func<T> retval = null;

			foreach (var value in enumerable)
			{
				retval = combiner(value);
			}

			return retval != null ? retval() : default(T);
		}

		public static T Fold<T>(this IEnumerable<T> enumerable, Func<T, T, T> operation)
		{
			var enumerator = enumerable.GetEnumerator();

			if (!enumerator.MoveNext())
			{
				return default(T);
			}

			var retval = enumerator.Current;

			while (enumerator.MoveNext())
			{
				retval = operation(retval, enumerator.Current);
			}

			return retval;
		}

		public static T Fold<T>(this IEnumerable<T> enumerable, T initial, Func<T, T, T> operation)
		{
			var retval = initial;

			foreach (var value in enumerable)
			{
				retval = operation(retval, value);
			}

			return retval;
		}

		public static IEnumerable<T> Tail<T>(this IEnumerable<T> enumerable)
		{
			var enumerator = enumerable.GetEnumerator();

			if (!enumerator.MoveNext())
			{
				yield break;
			}

			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
		}

		public static U Fold<T, U>(this IEnumerable<T> enumerable, Converter<T, U> converter, Func<U, U, U> operation)
		{
			var enumerator = enumerable.GetEnumerator();

			if (!enumerator.MoveNext())
			{
				return default(U);
			}

			var retval = converter(enumerator.Current);

			while (enumerator.MoveNext())
			{
				retval = operation(retval, converter(enumerator.Current));
			}

			return retval;
		}

		public static U Fold<T, U>(this IEnumerable<T> enumerable, Converter<T, U> converter, U initial, Func<U, U, U> operation)
		{
			var retval = initial;

			foreach (var value in enumerable)
			{
				retval = operation(retval, converter(value));
			}

			return retval;
		}

		/// <summary>
		/// Walks through an enumeration performing no action on each item
		/// </summary>
		/// <remarks>
		/// This method is equivalent to the following call:
		/// <para>
		/// <code>
		/// ForEach(enumerables, ActionUtils<T>.Null);
		/// </code>
		/// </para>
		/// </remarks>
		/// <param name="enumerables"></param>
		public static void Consume<T>(this IEnumerable<T> enumerables)
		{
			ForEach(enumerables, ActionUtils<T>.Null);
		}

		/// <summary>
		/// Copy an IEnumerable to a collection class
		/// </summary>
		/// <typeparam name="T">
		/// The type of the enumerable to copy into a collection
		/// </typeparam>
		/// <typeparam name="R">
		/// The collection type to return
		/// </typeparam>
		public static R CopyTo<T, R>(this IEnumerable<T> enumerable)
			where R : ICollection<T>, new()
		{
			var retval = new R();

			foreach (var value in enumerable)
			{
				retval.Add(value);
			}

			return retval;
		}

		/// <summary>
		/// Converts an untyped IEnumerable into a typed IEnumerable<T>
		/// </summary>
		/// <param name="enumerable"></param>
		/// <returns></returns>
		public static IEnumerable<T> ToTyped<T>(this IEnumerable enumerable)
		{
			foreach (T value in enumerable)
			{
				yield return value;
			}
		}

		/// <summary>
		/// Checks if an enumerable contains a given value using the given comparison
		/// </summary>
		/// <typeparam name="T">The element type for the enumerable</typeparam>
		/// <param name="enumerable">The enumerable to search</param>
		/// <param name="value">The value to search for</param>
		/// <param name="comparison">The comparison to use when searching</param>
		/// <returns>
		/// True if the <c>enumerable</c> contains the given <c>value</c>.
		/// </returns>
		public static bool Contains<T>(this IEnumerable<T> enumerable, T value, Comparison<T> comparison)
		{
			foreach (var item in enumerable)
			{
				if (comparison(value, item) == 0)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Creates a new IEnumerable with items sorted based on the <see cref="Comparer{T}.Default"/>
		/// </summary>
		public static IEnumerable<T> Sorted<T>(this IEnumerable<T> enumerable)
		{
			return Sorted(enumerable, Comparer<T>.Default);
		}

		/// <summary>
		/// Creates a new IEnumerable with items sorted based on the provided comparison
		/// </summary>
		public static IEnumerable<T> Sorted<T>(this IEnumerable<T> enumerable, Comparison<T> comparison)
		{
			var list = new List<T>(enumerable);

			list.Sort(comparison);

			foreach (var item in list)
			{
				yield return item;
			}
		}

		/// <summary>
		/// Creates a new IEnumerable with items sorted based on the provided Comparer.
		/// Elements will not be calculated lazily.
		/// </summary>
		public static IEnumerable<T> Sorted<T>(this IEnumerable<T> enumerable, IComparer<T> comparer)
		{
			var list = new List<T>(enumerable);

			list.Sort(comparer);

			foreach (var item in list)
			{
				yield return item;
			}
		}

		/// <summary>
		/// Creates a new sorted IEnumerable where duplicate items are removed.
		/// Elements will not be calculated lazily.
		/// </summary>
		public static IEnumerable<T> DistinctSorted<T>(this IEnumerable<T> enumerable)
		{
			return DistinctSorted(enumerable, Comparer<T>.Default);
		}

		/// <summary>
		/// Creates a new sorted IEnumerable where duplicate items are removed
		/// </summary>
		public static IEnumerable<T> DistinctSorted<T>(this IEnumerable<T> enumerable, IComparer<T> comparer)
		{
			IDictionary<T, T> dictionary = new SortedDictionary<T, T>(comparer);

			foreach (var value in enumerable)
			{
				dictionary[value] = value;
			}

			foreach (var value in dictionary.Values)
			{
				yield return value;
			}
		}

		/// <summary>
		/// Trys to get the last value of an IEnumerable
		/// </summary>
		/// <param name="enumerable">The enumeration to get the last value from</param>
		/// <param name="lastValue">The variable to store the last value</param>
		/// <returns>
		/// True if the last element was returned or False if the enumerable was empty.
		/// </returns>
		public static bool TryGetLast<T>(this IEnumerable<T> enumerable, out T lastValue)
		{
			var empty = true;

			lastValue = default(T);

			foreach (var value in enumerable)
			{
				lastValue = value;
				empty = false;
			}

			return !empty;
		}

		/// <summary>
		/// Trys to get the first value of an IEnumerable
		/// </summary>
		/// <param name="enumerable">The enumeration to get the last value from</param>
		/// <param name="firstValue">The variable to store the first value</param>
		/// <returns>
		/// True if the first element was returned or False if the enumerable was empty.
		/// </returns>
		public static bool TryGetFirst<T>(this IEnumerable<T> enumerable, out T firstValue)
		{
			foreach (T value in enumerable)
			{
				firstValue = value;

				return true;
			}

			firstValue = default(T);

			return false;
		}

		/// <summary>
		/// Finds an item based on a predicate
		/// </summary>
		/// <typeparam name="T">The element type of the enumerable</typeparam>
		/// <param name="enumerable">The enumerable to search</param>
		/// <param name="match">The predicate that will validate if the element is found</param>
		/// <param name="retval">The variable to store the element if found</param>
		/// <returns>True if the element was found otherwise False</returns>
		public static bool TryFind<T>(this IEnumerable<T> enumerable, Predicate<T> match, out T retval)
		{
			foreach (var item in enumerable)
			{
				if (match(item))
				{
					retval = item;

					return true;
				}
			}

			retval = default(T);

			return false;
		}

		/// <summary>
		/// Converts a indexed range of items from an array to an IEnumerable.
		/// Elements will be calculated lazily.
		/// </summary>
		/// <remarks>
		/// The returned enumerable will by empty or smaller than expected if the range is invalid or overflows.
		/// </remarks>
		/// <param name="array">The array to get elements from</param>
		/// <param name="startOffset">The offset to the first element in the array to return</param>
		/// <param name="count">The number of elements from the array the enumerable should return</param>
		/// <returns>A new enumerable</returns>
		public static IEnumerable<T> Range<T>(T[] array, int startOffset, int count)
		{
			for (var i = startOffset; i < startOffset + count; i++)
			{
				yield return array[i];
			}
		}

		/// <summary>
		/// Returns a new enumerable that will be made up of the elements of another
		/// eumerable within a given range. Elements will be calculated lazily.
		/// </summary>
		/// <remarks>
		/// The returned enumerable will by empty or smaller than expected if the range is invalid or overflows.
		/// </remarks>
		/// <param name="enumerable">The enumerable to get elements from</param>
		/// <param name="startOffset">The offset to the first element in the array to return</param>
		/// <param name="count">The number of elements from the array the enumerable should return</param>
		/// <returns>A new enumerable</returns>
		public static IEnumerable<T> Range<T>(this IEnumerable<T> enumerable, int startOffset, int count)
		{
			var enumerator = enumerable.GetEnumerator();

			using (enumerator)
			{
				for (var i = 0; i < startOffset; i++)
				{
					if (!enumerator.MoveNext())
					{
						yield break;
					}
				}

				while (enumerator.MoveNext())
				{
					yield return enumerator.Current;
				}
			}
		}
	}
}