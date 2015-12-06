using System;

namespace Platform.IO
{
	public interface IStreamWithEvents
	{
		event EventHandler BeforeClose;
		event EventHandler AfterClose;
	}
}
