using System;

namespace Platform.Collections
{
	public class BufferOverflowException
		: Exception
	{
		public BufferOverflowException()
		{
		}

		public BufferOverflowException(string message)
			: base(message)
		{	
		}
	}
}
