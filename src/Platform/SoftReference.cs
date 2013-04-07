using System;
using System.Threading;

namespace Platform
{
	/// <summary>
	/// Summary description for SoftReference.
	/// </summary>
	public class SoftReference
		: Reference
	{
		WeakReference weakTarget;
		
		public SoftReference(object target)
			: base(target)
		{
			weakTarget = new WeakReference(target);
		}
	}
}
