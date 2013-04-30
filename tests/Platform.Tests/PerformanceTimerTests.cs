using System;
using System.Threading;
using NUnit.Framework;

namespace Platform.Tests
{
	[TestFixture]
	public class PerformanceTimerTests
	{
		[Test]
		public void Test_Time_With_DateTime_Based_Clock()
		{
			PerformanceTimer timer;
			var period = TimeSpan.FromMilliseconds(250);

			using (timer = new PerformanceTimer())
			{
				Thread.Sleep(period);
			}

			Console.WriteLine(timer.TimeElapsed);
			Assert.IsTrue(timer.TimeElapsed > (period - TimeSpan.FromMilliseconds(100)) && (timer.TimeElapsed < period + TimeSpan.FromMilliseconds(100)));
		}

		[Test]
		public void Test_Timer_With_HighPerformanceClock()
		{
			PerformanceTimer timer;
			var period = TimeSpan.FromMilliseconds(250);

			using (timer = new PerformanceTimer(HighPerformanceClock.Default))
			{
				Thread.Sleep(period);
			}

			Console.WriteLine(timer.TimeElapsed);
			Assert.IsTrue(timer.TimeElapsed > (period - TimeSpan.FromMilliseconds(100)) && (timer.TimeElapsed < period + TimeSpan.FromMilliseconds(100)));
		}
	}
}
