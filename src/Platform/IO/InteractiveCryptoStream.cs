using System;
using System.IO;
using System.Security.Cryptography;

namespace Platform.IO
{
	/// <summary>
	/// A stream wrapper implementation that implements support cryptography
	/// that supports tream flushing even if the current cryptographic buffer
	/// has not yet been filled.
	/// </summary>
	/// <remarks>
	/// Cryptographic blocks that have not yet been filled when <see cref="Flush()"/>
	/// is called will be filled with random data.  Cryptographic blocks are marked
	/// with the length of the real data.
	/// </remarks>
	public class InteractiveCryptoStream
		: StreamWrapper
	{
		private readonly byte[] readOneByte = new byte[1];
		private readonly byte[] writeOneByte = new byte[1];
		private readonly byte[] readPreBuffer;
		private readonly byte[] readBuffer;
		private int readBufferCount;
		private int readBufferIndex;
		private readonly byte[] writeBuffer;
		private readonly byte[] writeBlockBuffer;
		private int writeBufferCount;
		private readonly ICryptoTransform transform;
		private readonly CryptoStreamMode mode;
		
		public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
		public override long Length { get { throw new NotSupportedException(); } }
		public override bool CanRead { get { return this.mode == CryptoStreamMode.Read; } }
		public override bool CanWrite { get { return this.mode == CryptoStreamMode.Write; } }

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public InteractiveCryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode)
			: this(stream, transform, mode, 255)
		{	
		}

		public InteractiveCryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode, int bufferSizeInBlocks)
			: base(stream)
		{
			if (bufferSizeInBlocks < 0)
			{
				throw new ArgumentOutOfRangeException("bufferSizeInBlocks", bufferSizeInBlocks, "BufferSize can't be less than 0");
			}

			if (bufferSizeInBlocks > 255)
			{
				bufferSizeInBlocks = 255;
			}

			this.mode = mode;

			this.transform = transform;
			
			this.writeBuffer = new byte[2 + transform.InputBlockSize * bufferSizeInBlocks];
			this.writeBlockBuffer = new byte[transform.OutputBlockSize];

			this.readPreBuffer = new byte[transform.OutputBlockSize * 5];
			this.readBuffer = new byte[transform.InputBlockSize * 5];

			this.writeBufferCount = 2;
			this.readBufferCount = 0;
			this.readBufferIndex = 0;
		}

		public override void WriteByte(byte value)
		{
			this.writeOneByte[0] = value;

			this.Write(this.writeOneByte, 0, 1);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!this.CanWrite)
			{
				throw new NotSupportedException("Reading not supported");
			}

			if (count == 0)
			{
				return;
			}

			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			if (offset + count > buffer.Length)
			{
				throw new ArgumentException("offset and length exceed buffer size");
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", offset, "can not be negative");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", count, "can not be negative");
			}

			while (count > 0)
			{
				var length = Math.Min(count, this.writeBuffer.Length - this.writeBufferCount);

				if (length == 0)
				{
					this.Flush();
	
					continue;
				}

				Array.Copy(buffer, offset, this.writeBuffer, this.writeBufferCount, length);

				this.writeBufferCount += length;
				count -= length;
			}
		}

		private int bytesLeftInSuperBlock = 0;
		private int bytesPaddingSuperBlock = 0;

		public override int ReadByte()
		{
			var x = this.Read(this.readOneByte, 0, 1);

			if (x == -1)
			{
				return -1;
			}

			return this.readOneByte[0];
		}

		private int ReadAndConvertBlocks()
		{
			int x;
			var read = 0;

			while (true)
			{
				x = base.Read(this.readPreBuffer, 0, this.readPreBuffer.Length - read);

				if (x == 0)
				{
					return -1;
				}

				read += x;

				if (read % this.transform.OutputBlockSize == 0)
				{
					break;
				}
			}

			if (this.transform.CanTransformMultipleBlocks)
			{
				return this.transform.TransformBlock(this.readPreBuffer, 0, read, this.readBuffer, 0);
			}
			else
			{
				x = 0;

				for (int i = 0; i < read / this.transform.OutputBlockSize; i++)
				{
					x += this.transform.TransformBlock(this.readPreBuffer,
						i * this.transform.OutputBlockSize,
						this.transform.OutputBlockSize,
					                                this.readBuffer,
						i * this.transform.InputBlockSize);
				}
				
				return x;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (!this.CanRead)
			{
				throw new NotSupportedException("Reading not supported");
			}

			if (count == 0)
			{
				return 0;
			}

			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			if (offset + count > buffer.Length)
			{
				throw new ArgumentException("offset and length exceed buffer size");
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", offset, "can not be negative");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", count, "can not be negative");
			}

			while (true)
			{
				if (this.readBufferCount == 0)
				{
					while (true)
					{
						var x = this.ReadAndConvertBlocks();

						if (x == 0)
						{
							continue;
						}
						else if (x == -1)
						{
							this.readBufferIndex = 0;
							this.readBufferCount = 0;

							return 0;
						}

						this.readBufferIndex = 0;
						this.readBufferCount = x;

						break;
					}
				}

				if (this.bytesLeftInSuperBlock == 0)
				{
					this.bytesLeftInSuperBlock = this.readBuffer[this.readBufferIndex];
					this.bytesLeftInSuperBlock |= this.readBuffer[this.readBufferIndex + 1] << 8;

					this.bytesLeftInSuperBlock += 2;
					
					if (this.bytesLeftInSuperBlock == 2)
					{
						this.bytesLeftInSuperBlock = 0;
						this.readBufferIndex += this.transform.InputBlockSize;
						this.readBufferCount -= this.transform.InputBlockSize;

						continue;
					}

					this.readBufferCount -= 2;
					this.readBufferIndex += 2;
					
					this.bytesPaddingSuperBlock = (((this.bytesLeftInSuperBlock + 15) / 16) * 16) - this.bytesLeftInSuperBlock;

					this.bytesLeftInSuperBlock -= 2;
				}

				var length = MathUtils.Min(this.bytesLeftInSuperBlock, this.readBufferCount, count);

				Array.Copy(this.readBuffer, this.readBufferIndex, buffer, offset, length);

				this.bytesLeftInSuperBlock -= length;
				this.readBufferCount -= length;
				this.readBufferIndex += length;

				if (this.bytesLeftInSuperBlock == 0)
				{
					this.readBufferIndex += this.bytesPaddingSuperBlock;
					this.readBufferCount -= this.bytesPaddingSuperBlock;
				}

				return length;
			}
		}

		private readonly Random random = new Random();

		public override void Flush()
		{
			this.PrivateFlush();

			// The ICryptoTransform always keeps the most recent block
			// in memory so we have to write an empty block to get the
			// result of the encryption of the last useful block.

			this.writeBuffer[0] = 0;
			this.writeBuffer[1] = 0;

			for (int i = 2; i < this.transform.InputBlockSize; i++)
			{
				this.writeBuffer[i] = (byte)this.random.Next(0, 255);
			}
																	   
			this.transform.TransformBlock(this.writeBuffer, 0, this.transform.InputBlockSize,  this.writeBlockBuffer, 0);

			base.Write(this.writeBlockBuffer, 0, this.writeBlockBuffer.Length);
			base.Flush();
		}

		private void PrivateFlush()
		{
			if (this.writeBufferCount == 2)
			{
				return;
			}

			var numberOfBlocks = ((this.writeBufferCount - 1) / this.transform.InputBlockSize) + 1;

			if (numberOfBlocks == 0)
			{
				return;
			}
			
			this.writeBuffer[0] = ((byte)((this.writeBufferCount - 2) & 0xff));
			this.writeBuffer[1] = ((byte)(((this.writeBufferCount - 2) & 0xff00) >> 8));

			for (var i = 0; i < numberOfBlocks; i++)
			{
				if (i == numberOfBlocks - 1)
				{
					if (this.writeBuffer.Length - this.writeBufferCount < this.transform.InputBlockSize)
					{
						Array.Clear(this.writeBuffer, this.writeBufferCount, this.writeBuffer.Length - this.writeBufferCount);
					}
				}

				var x = this.transform.TransformBlock(this.writeBuffer, i * this.transform.InputBlockSize, this.transform.InputBlockSize,  this.writeBlockBuffer, 0);

				if (x != this.transform.InputBlockSize)
				{
					throw new Exception();
				}

				base.Write(this.writeBlockBuffer, 0, this.writeBlockBuffer.Length);
			}

			this.writeBufferCount = 2;
		}
	}
}
