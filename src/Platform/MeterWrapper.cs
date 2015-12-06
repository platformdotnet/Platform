// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;

namespace Platform
{
	public class MeterWrapper
		: IMeter
	{
		public virtual double Percentage => this.Wrappee.Percentage;
		public virtual object Value => this.CurrentValue;
		public virtual IMeter Wrappee => this.wrappee;
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
			this.MajorChangeEvent?.Invoke(this, eventArgs);
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
			this.ValueChangedEvent?.Invoke(this, eventArgs);
		}

		public virtual object MaximumValue => this.Wrappee.MaximumValue;
		public virtual object MinimumValue => this.Wrappee.MinimumValue;
		public object CurrentValue => this.Wrappee.CurrentValue;
		public virtual string Units => this.Wrappee.Units;

		#endregion

		#region IOwned Members

		public virtual object Owner => this.Wrappee.Owner;

		#endregion

		public override string ToString()
		{
			return this.Wrappee.ToString();
		}
	}
}
