#region License
/*
 * MeterWrapper.cs
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

namespace Platform
{
	public class MeterWrapper
		: IMeter
	{
		public virtual double Percentage
		{
			get
			{
				return this.Wrappee.Percentage;
			}
		}

		public virtual object Value
		{
			get
			{
				return this.CurrentValue;
			}
		}

		public virtual IMeter Wrappee
		{
			get
			{
				return wrappee;
			}
		}
		private IMeter wrappee;

		protected virtual void SetWrappee(IMeter value)
		{
            this.wrappee = value;
		}

		public MeterWrapper(IMeter meter)
		{
			wrappee = meter;
		}

		#region IMeter Members

		public virtual event EventHandler MajorChange
		{
			add
			{
				lock (this)
				{
					if (MajorChangeEvent == null)
					{
						this.Wrappee.MajorChange += new EventHandler(DelegateMajorChange);
					}

					MajorChangeEvent = (EventHandler)Delegate.Combine(MajorChangeEvent, value);
				}
			}
			remove
			{
				lock (this)
				{
					MajorChangeEvent = (EventHandler)Delegate.Remove(MajorChangeEvent, value);

					if (MajorChangeEvent == null)
					{
						this.Wrappee.MajorChange -= new EventHandler(DelegateMajorChange);
					}
				}
			}
		}
		private EventHandler MajorChangeEvent;

		private void DelegateMajorChange(object sender, EventArgs eventArgs)
		{
			if (MajorChangeEvent != null)
			{
				MajorChangeEvent(this, eventArgs);
			}
		}

		public virtual event EventHandler<MeterEventArgs> ValueChanged
		{
			add
			{
				lock (this)
				{
					if (ValueChangedEvent == null)
					{
						this.Wrappee.ValueChanged += DelegateValueChanged;
					}

					ValueChangedEvent = (EventHandler<MeterEventArgs>)Delegate.Combine(ValueChangedEvent, value);
				}
			}
			remove
			{
				lock (this)
				{
					ValueChangedEvent = (EventHandler<MeterEventArgs>)Delegate.Remove(ValueChangedEvent, value);

					if (ValueChangedEvent == null)
					{
						this.Wrappee.ValueChanged -= DelegateValueChanged;
					}
				}
			}
		}
		private EventHandler<MeterEventArgs> ValueChangedEvent;

		protected virtual void OnValueChanged(object sender, MeterEventArgs eventArgs)
		{
			DelegateValueChanged(sender, eventArgs);
		}

		private void DelegateValueChanged(object sender, MeterEventArgs eventArgs)
		{
			if (ValueChangedEvent != null)
			{
				ValueChangedEvent(this, eventArgs);
			}
		}

		public virtual object MaximumValue
		{
			get
			{
				return this.Wrappee.MaximumValue;
			}
		}

		public virtual object MinimumValue
		{
			get
			{
				return this.Wrappee.MinimumValue;
			}
		}

		public object CurrentValue
		{
			get
			{
				return this.Wrappee.CurrentValue;
			}
		}

		public virtual string Units
		{
			get
			{
				return this.Wrappee.Units;
			}
		}

		#endregion

		#region IOwned Members

		public virtual object Owner
		{
			get
			{
				return this.Wrappee.Owner;
			}
		}

		#endregion

		public override string ToString()
		{
			return this.Wrappee.ToString();
		}
	}
}
