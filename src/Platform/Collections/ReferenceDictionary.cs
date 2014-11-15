using System;
using System.Collections;
using System.Collections.Generic;
using Platform.References;

namespace Platform.Collections
{
	/// <summary>
	/// Base class for dictionaries that store items using <see cref="Reference{T}"/> objects.
	/// </summary>
	/// <typeparam name="K">The key type</typeparam>
	/// <typeparam name="V">The value type</typeparam>
	/// <typeparam name="R">The <see cref="Reference{T}"/> type to use</typeparam>
	public abstract class ReferenceDictionary<K, V, R>
		: IDictionary<K, V>, IObjectCache<K, V>
		where V : class
		where R : Reference<V>, IKeyed<K>
	{
		protected internal IDictionary<K, R> dictionary;
		private bool suspendCleanDeadReferences = false;
		protected ReferenceQueueBase<V> referenceQueueBase;
		public event EventHandler<ReferenceDictionaryEventArgs<K, V>> ReferenceCleaned;

		public bool IsReadOnly { get { return false; } }

		protected virtual void OnReferenceCleaned(ReferenceDictionaryEventArgs<K, V> eventArgs)
		{
			if (ReferenceCleaned != null)
			{
				ReferenceCleaned(this, eventArgs);
			}
		}

		public virtual bool Contains(KeyValuePair<K, V> item)
		{
			V value;

			if (!this.TryGetValue(item.Key, out value))
			{
				return false;
			}
			
			if (value == null && item.Value == null)
			{
				return true;
			}

			if (value == null)
			{
				return false;
			}

			if (!value.Equals(item.Value))
			{
				return false;
			}

			return true;
		}

		public virtual void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public virtual bool ContainsKey(K key)
		{
			V value;

			if (!this.TryGetValue(key, out value))
			{
				return false;
			}

			return true;
		}

		protected ReferenceDictionary()
			: this(null)
		{	
		}

		protected ReferenceDictionary(int capacity)
			: this(capacity, null)
		{
		}

		protected ReferenceDictionary(IEqualityComparer<K> comparer)
			 : this(0, comparer)
		{
		}

		protected ReferenceDictionary(int capacity, IEqualityComparer<K> comparer)
		{
			this.referenceQueueBase = new ReferenceQueueBase<V>();

			this.dictionary = comparer == null ? new Dictionary<K, R>(capacity) : new Dictionary<K, R>(capacity, comparer);
		}

		protected abstract R CreateReference(K key, V value);

		protected virtual bool CleanDeadReferences()
		{
			if (suspendCleanDeadReferences)
			{
				return false;
			}

			var retval = false;

			lock (this)
			{
				suspendCleanDeadReferences = true;

				IKeyed<K> keyed;
				
				while ((keyed = (IKeyed<K>)this.referenceQueueBase.Dequeue(0)) != null)
				{
					Remove(keyed.Key);

					retval = true;
				}

				suspendCleanDeadReferences = false;
			}

			return retval;
		}

		public virtual void Add(K key, V value)
		{
			lock (this)
			{
				CleanDeadReferences();

				dictionary.Add(key, CreateReference(key, value));
			}
		}

		public virtual bool Remove(K key)
		{
			lock (this)
			{
				CleanDeadReferences();

				return dictionary.Remove(key);
			}
		}

		public virtual V this[K key]
		{
			get
			{
				V value;

				if (!TryGetValue(key, out value))
				{
					throw new KeyNotFoundException(key.ToString());
				}
				else
				{
					return value;
				}
			}
			set
			{
				lock (this)
				{
					dictionary[key] = CreateReference(key, value);
				}
			}
		}

		public virtual bool TryGetValue(K key, out V value)
		{
			lock (this)
			{
				R reference;

				if (!dictionary.TryGetValue(key, out reference))
				{
					value = null;

					return false;
				}

				value = reference.Target;

				return value != null;
			}
		}

		public virtual void Add(KeyValuePair<K, V> item)
		{
			lock (this)
			{
				if (this.ContainsKey(item.Key))
				{
					throw new InvalidOperationException("Already exists");
				}

				this[item.Key] = item.Value;	
			}
			
		}

		public virtual void Clear()
		{
			lock (this)
			{
				dictionary.Clear();
			}
		}

		public int MaximumCapacity { get { return Int32.MaxValue; }}

		public virtual int Count
		{
			get
			{
				lock (this)
				{
					CleanDeadReferences();

					return dictionary.Count;
				}
			}
		}

		public virtual bool Remove(KeyValuePair<K, V> item)
		{
			lock (this)
			{
				return dictionary.Remove(new KeyValuePair<K, R>(item.Key, CreateReference(item.Key, item.Value)));
			}
		}

		public ICollection<K> Keys { get { throw new NotSupportedException(); } }

		public ICollection<V> Values { get { throw new NotSupportedException(); } }

		public virtual IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			foreach (var keyValuePair in dictionary)
			{
				var value = keyValuePair.Value.Target;

				if (value != null)
				{
					yield return new KeyValuePair<K, V>(keyValuePair.Key, value);
				}
			}
		}

		public virtual void Push(K key, V value)
		{
			lock (this)
			{
				this[key] = value;
			}
		}

		public virtual bool Evict(K key)
		{
			lock (this)
			{
				return this.Remove(key);
			}
		}

		public virtual void Flush()
		{
			lock (this)
			{
				this.Clear();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
