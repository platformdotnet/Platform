#region License
/*
 * StreamWrapper.cs
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
using System.IO;
using System.Threading;

namespace Platform.IO
{
	/// <summary>
	/// Wraps an inner <see cref="Stream"/> class and delegates all calls to the inner class.
	/// Use this as a base class for stream wrappers and override only the methods that need
	/// to be intercepted.
	/// </summary>
	public abstract class StreamWrapper
		: Stream, IWrapperObject<Stream>
	{
		private readonly Stream wrappee;

		/// <summary>
		/// The wrapped stream
		/// </summary>
		public virtual Stream Wrappee
		{
			get
			{
				return wrappee;
			}
		}

		/// <summary>
		/// Constructs a new <see cref="StreamWrapper"/>
		/// </summary>
		/// <param name="wrappee">The inner/wrapped stream</param>
		protected StreamWrapper(Stream wrappee)
		{
			this.wrappee = wrappee;
		}

		private Func<byte[], int, int, int> readFunction;
		private Action<byte[], int, int> writeFunction;

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{		
			if (!this.CanRead)
			{
				throw new NotSupportedException();
			}

			if (readFunction == null)
			{
				lock (this)
				{
					Func<byte[], int, int, int> localReadFunction = this.Read;

					System.Threading.Thread.MemoryBarrier();

					this.readFunction = localReadFunction;
				}
			}

			return readFunction.BeginInvoke(buffer, offset, count, callback, state);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (!this.CanWrite)
			{
				throw new NotSupportedException();
			}

			if (writeFunction == null)
			{
				lock (this)
				{
					Action<Byte[], int, int> localWriteFunction = this.Write;

					System.Threading.Thread.MemoryBarrier();

					this.writeFunction = localWriteFunction;
				}
			}

			return writeFunction.BeginInvoke(buffer, offset, count, callback, state);
		}

		public override bool CanRead
		{
			get
			{
				return this.Wrappee.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return this.Wrappee.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return this.Wrappee.CanWrite;
			}
		}

		public override void Close()
		{
			this.Wrappee.Close();
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			return readFunction.EndInvoke(asyncResult);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			writeFunction.EndInvoke(asyncResult);
		}

		public override bool Equals(object obj)
		{
			return this.Wrappee.Equals(obj);
		}

		public override void Flush()
		{
			this.Wrappee.Flush();	
		}

		public override int GetHashCode()
		{
			return this.Wrappee.GetHashCode ();
		}

		public override long Length
		{
			get
			{
				return this.Wrappee.Length;
			}
		}

		public override long Position
		{
			get
			{
				return this.Wrappee.Position;
			}
			set
			{
				this.Wrappee.Position = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return this.Wrappee.Read(buffer, offset, count);
		}

		public override int ReadByte()
		{
			return this.Wrappee.ReadByte();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.Wrappee.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			this.Wrappee.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.Wrappee.Write(buffer, offset, count);			
		}

		public override void WriteByte(byte value)
		{
			this.Wrappee.WriteByte(value);
		}
	}
}
