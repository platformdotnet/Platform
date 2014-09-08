using System;

namespace Platform.Collections
{
	public class BufferUnderflowException
		: Exception
	{
		public BufferUnderflowException()
		{
		}

		public BufferUnderflowException(string message)
			: base(message)
		{	
		}
	}
}
