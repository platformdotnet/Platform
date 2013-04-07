using System;

namespace Platform
{
	/// <summary>
	/// Summary description for Reference.
	/// </summary>
	public abstract class Reference
	{
		public object Target
		{
			get
			{
				return m_Target;
			}
			set
			{
				m_Target = value;
			}
		}
		private object m_Target;

		protected Reference(object target)
		{
			m_Target = target;
		}
	}
}
