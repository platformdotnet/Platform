using System;
using System.Threading;
using Platform.References;

namespace Platform.Collections
{
	/// <summary>
	/// A <see cref="ReferenceDictionary{K,V,R}"/> that is based on <see cref="WeakReference{T}"/> objects.
	/// The dictionary will automatically shrink when references in the dictionary become unavailable.
	/// </summary>
	/// <remarks>
	/// Because of the non-deterministic nature of the GC, the size of the dictionary will not necessarily
	/// reflect the number of items actually available in the dictionary.  The count is only an approximation.
	/// Items available one moment may be come unavailable the next.
	/// </remarks>
	/// <typeparam name="K">The key type</typeparam>
	/// <typeparam name="V">The value type</typeparam>
	public class WeakReferenceDictionary<K, V>
		: ReferenceDictionary<K, V, WeakReferenceDictionary<K, V>.KeyedWeakReference>
		where V : class
	{
		/// <summary>
		/// A <see cref="References.WeakReference{T}"/> that contains a key.
		/// </summary>
		public sealed class KeyedWeakReference
			: References.WeakReference<V>, IKeyed<K>			
		{
			object IKeyed.Key => this.Key;

			public K Key
			{
				get; }

			public KeyedWeakReference(K key, V reference)
				: this(key, reference, null)
			{
			}

			public KeyedWeakReference(K key, V reference, IReferenceQueue<V> referenceQueue)
				: base(reference, referenceQueue)
			{
				this.Key = key;
			}

			public override int GetHashCode()
			{
				return Key.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var keyedReference = (KeyedWeakReference)obj;

				if (obj == null)
				{
					return false;
				}

				return obj == this || keyedReference.Key.Equals(this.Key);
			}
		}

		private readonly Timer cleanerTimer;

		public WeakReferenceDictionary()
			: this(TimeSpan.FromMilliseconds(-1), 0)
		{
		}

		public WeakReferenceDictionary(int capacity)
			: this(TimeSpan.FromMilliseconds(-1), capacity)
		{
		}

		/// <summary>
		/// Creates a new <see cref="WeakReferenceDictionary{K,V}"/> with the provided type
		/// as the store for the dictionary.
		/// </summary>
		/// <param name="periodicCleanTimeout">The amount of time</param>
		/// <param name="capacity">The initial capacity of the dictionary</param>
		public WeakReferenceDictionary(TimeSpan periodicCleanTimeout, int capacity)
			: base(capacity)
		{
			if ((int)periodicCleanTimeout.TotalMilliseconds != -1)
			{
				cleanerTimer = new Timer(OnTimer, null, (int)Math.Round(periodicCleanTimeout.TotalMilliseconds / 2), (int)Math.Round(periodicCleanTimeout.TotalMilliseconds / 2));
			}
		}

		~WeakReferenceDictionary()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);

			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.cleanerTimer.Dispose();
			}
		}

		/// <summary>
		/// Called by the periodic clean out timer
		/// </summary>
		protected virtual void OnTimer(object state)
		{
			base.CleanDeadReferences();
		}

		/// <summary>
		/// Overrides <see cref="ReferenceDictionary{K,V,R}.CleanDeadReferences"/> and does not
		/// perform cleaning if a timer has been set.
		/// </summary>
		/// <returns>True if dead references were cleaned out</returns>
		protected override bool CleanDeadReferences()
		{
			if (cleanerTimer != null)
			{
				// Let the timer do the cleaning

				return false;
			}

			return base.CleanDeadReferences();
		}

		/// <summary>
		/// Creates a <see cref="KeyedWeakReference"/> with the supplied key and value.
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="value">The value</param>
		/// <returns>A new <see cref="KeyedWeakReference"/></returns>
		protected override KeyedWeakReference CreateReference(K key, V value)
		{
			return new KeyedWeakReference(key, value, this.referenceQueueBase);
		}
	}
}
