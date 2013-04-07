#region License
/*
 * WeakObjectCache.cs
 * 
 * Copyright (c) 2004 Thong Nguyen (tum@veridicus.com)
 * 
 * This program is free software; you can redistribute it and/or modify it under
 * the terms of the GNU General Public License as published by the Free Software
 * Foundation; either version 2 of the License, or (at your option) any later
 * version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU General Public License along with
 * this program; if not, write to the Free Software Foundation, Inc., 59 Temple
 * Place, Suite 330, Boston, MA 02111-1307 USA
 * 
 * The license is packaged with the program archive in a file called LICENSE.TXT
 * 
 * You can also view a copy of the license online at:
 * http://www.opensource.org/licenses/gpl-license.php
 */
#endregion

using System;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using Platform;

namespace Platform.Utilities
{
	/// <summary>
	/// An <see cref="IObjectCache"/> implementation that uses an inderlying <c>IObjectCache</c> implementation
	/// and periodically destroys all unreferenced objects.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Often, a cache will require objects to be kept in the cache as long as there are live references to the
	/// object.  Developers can do this by storing <see cref="WeakReference"/>s inside an object cache.  The
	/// disadvantage of this technique however is that it is possible for <c>WeakReferences</c> may be cleared 
	/// immediately after the <c>target</c> object goes out of scope.  Sometimes it is desirable to allow
	/// a grace period where the object may be out of scope everywhere except for inside the cache.  After the
	/// grace period, the object should will be released by the cache and allowed to be collected by the GC.
	/// This <c>IObjectCache</c> implementation implements a cache with these semantics.
	/// </p>
	/// <p>
	/// Currently, the object cache will guarantee that objects still referenced will never be removed from the 
	/// cache.  The cache will try to keep objects alive for the entire <c>gracePeriod</c> of the cache but this
	/// is not guaranteed but will guarantee that unreferenced objects won't be kept alive longer than the
	/// <c>gracePeriod</c>.
	/// </p>
	/// </remarks>
	public class WeakPeriodicTrashingObjectCache
		: OldObjectCacheWrapper
	{
		private Timer m_Timer;

		private class Entry
		{
			public object Key;
			public object Value;
			public DateTime Time;
			public TimeSpan GracePeriod;

			public Entry(object key, object value, DateTime time, TimeSpan gracePeriod)
			{
				this.Key = key;
				this.Value = value;
				this.Time = time;
				this.GracePeriod = gracePeriod;
			}
		}

		private TimeSpan m_GracePeriod;

		/// <summary>
		/// Create a new <c>WeakPeriodicTrashingObjectCache</c> that uses a hashtable based <see cref="DictionaryBackedObjectCache"/>
		/// with a <c>gracePeriod</c> of 30 minutes.
		/// </summary>
		public WeakPeriodicTrashingObjectCache()
			: this(new DictionaryBackedObjectCache(), TimeSpan.FromMinutes(30))
		{
		}

		/// <summary>
		/// Create a new <c>WeakPeriodicTrashingObjectCache</c> that uses a <see cref="DictionaryBackedObjectCache"/>
		/// with the supplied <c>trashPeriod</c>.
		/// </summary>
		/// <param name="gracePeriod">
		/// The period of time an object is permitted to be unreferenced before it is trashed.
		/// </param>
		public WeakPeriodicTrashingObjectCache(TimeSpan gracePeriod)
			: this(new DictionaryBackedObjectCache(), gracePeriod)
		{
		}

		/// <summary>
		/// Create a new <c>WeakPeriodicTrashingObjectCache</c> that uses the supplied <c>innerCache</c>
		/// with the supplied <c>trashPeriod</c>.
		/// </summary>
		/// <param name="gracePeriod">
		/// The period of time between trashing.
		/// </param>
		public WeakPeriodicTrashingObjectCache(IObjectCache innerCache, TimeSpan gracePeriod)
			: base(innerCache)
		{
			m_GracePeriod = gracePeriod;

			m_Timer = new Timer(new TimerCallback(Timer_Callback), null, gracePeriod, gracePeriod);
		}

		/// <summary>
		/// Releases resources.
		/// </summary>
		~WeakPeriodicTrashingObjectCache()
		{
			m_Timer.Dispose();
		}

		private class KeysCollection
			: KeyValuesCollection
		{
			public KeysCollection(WeakPeriodicTrashingObjectCache cache)
				: base(cache)
			{
			}

			public override IEnumerator GetEnumerator()
			{
				foreach (object value in m_Outer.Wrappee.Values)
				{
					yield return ((Entry)value).Value;
				}
			}
		}

		private abstract class KeyValuesCollection
			: ICollection
		{
			protected WeakPeriodicTrashingObjectCache m_Outer;

			public KeyValuesCollection(WeakPeriodicTrashingObjectCache outer)
			{
				m_Outer = outer;
			}

			#region ICollection Members

			public virtual void CopyTo(Array array, int index)
			{
				foreach (object value in this)
				{
					array.SetValue(value, index++);
				}
			}

			public virtual int Count
			{
				get
				{
					return m_Outer.Count;
				}
			}

			public virtual bool IsSynchronized
			{
				get
				{
					return false;
				}
			}

			public virtual object SyncRoot
			{
				get
				{
					return this;
				}
			}

			#endregion

			#region IEnumerable Members

			public abstract IEnumerator GetEnumerator();

			#endregion
		}

		public override object this[object key]
		{
			get
			{
				lock (this.SyncRoot)
				{
					Entry entry = (Entry)base[key];

					if (entry == null)
					{
						return null;
					}

					entry.Time = DateTime.Now;

					return entry.Value;
				}
			}
			set
			{
				lock (this.SyncRoot)
				{
					base[key] = new Entry(key, value, DateTime.Now, m_GracePeriod);
				}
			}
		}

		public virtual void Add(object key, object value, TimeSpan gracePeriod)
		{
			lock (this.SyncRoot)
			{
				base[key] = new Entry(key, value, DateTime.Now, gracePeriod);
			}
		}

		/// <summary>
		/// Callback for Timer.
		/// </summary>
		protected virtual void Timer_Callback(object state)
		{
			lock (this.SyncRoot)
			{
				int count;
				IList weakList;
				IList strongList;
				
				Trace.WriteLine("Starting Collection", GetType().Name);

				weakList = new ArrayList(this.Count / 2);
				strongList = new ArrayList(this.Count);

				// Copy the objects from the cache into the list through WeakReferences.
				Fill(weakList, strongList);
				
				Trace.WriteLine(String.Format("{0} weak objects found", weakList.Count), GetType().Name);
				Trace.WriteLine(String.Format("{0} string objects found", strongList.Count), GetType().Name);

				// Flush the cache.
				this.Flush();

				// Collect all objects.
				GC.Collect(GC.MaxGeneration);
				GC.WaitForPendingFinalizers();

				count = 0;

				// Add all objects that haven't been collected back into the cache.
				foreach (Entry entry in weakList)
				{
					object value;

					value = ((WeakReference)entry.Value).Target;

					if (value != null)
					{
						entry.Value = value;

						count++;

						entry.Time = DateTime.Now;
						base[entry.Key] = entry;
					}
				}

				Trace.WriteLine(String.Format("{0} out of {1} weak objects still referenced", count, weakList.Count), GetType().Name);

				foreach (Entry entry in strongList)
				{
					base[entry.Key] = entry;
				}

				Trace.WriteLine("Finished Collection", GetType().Name);
			}
		}

		/// <summary>
		/// Fills the supplied temporary dictionary with items from the cache.
		/// </summary>
		private void Fill(IList weakList, IList strongList)
		{
			foreach (object key in this.Keys)
			{
				Entry entry;
				TimeSpan span;

				entry = (Entry)base[key];
				
				span = DateTime.Now - entry.Time;
				
				if (span > entry.GracePeriod)
				{
					entry.Value = new WeakReference(entry.Value);

					weakList.Add(entry);
				}
				else
				{
					strongList.Add(entry);
				}
			}
		}

		private KeysCollection m_KeysCollection;

		public override ICollection Values
		{
			get
			{
				if (m_KeysCollection == null)
				{
					KeysCollection keysCollection;

					keysCollection = new KeysCollection(this);

					System.Threading.Thread.MemoryBarrier();

					m_KeysCollection = keysCollection;
				}

				return m_KeysCollection;
			}
		}
	}
}
