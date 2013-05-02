using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Platform
{
	public class HighPerformanceClock
		: Clock
	{
		public static HighPerformanceClock Default = new HighPerformanceClock();

		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceCounter( out long lpPerformanceCount);

		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceFrequency(out long lpFrequency);

		private readonly double frequency;

		private static readonly PlatformID[] windowsPlatforms = new[] { PlatformID.Win32NT, PlatformID.Win32S, PlatformID.Win32Windows, PlatformID.WinCE, PlatformID.Xbox };
		
		public HighPerformanceClock()
		{
			long f;

			if (!windowsPlatforms.Contains(Environment.OSVersion.Platform))
			{
				throw new InvalidOperationException("Windows required");
			}

			if (QueryPerformanceFrequency(out f) == false)
			{
				throw new Win32Exception("High Performance Timer not supported");
			}

			frequency = f / 1000d;
		}

		public override double CurrentMilliseconds
		{
			get
			{
				long count;

				QueryPerformanceCounter(out count);

				return (double)(count / frequency);
			}
		}
	}
}
