using System.Collections.Generic;
using NUnit.Framework;
using Platform.Utilities;

namespace Platform.Tests
{
	[TestFixture]
	public class InvocationQueueTests
	{
		[Test]
		public void Test_Queue_Items_And_Run_In_Order()
		{
			var list = new List<int>();
			var expectedList = new List<int>();

			var queue = new InvocationQueue();

			for (var i = 0; i < 100; i++)
			{
				var j = i;

				expectedList.Add(i);

				queue.Enqueue(() => list.Add(j));
			}

			Assert.That(list.Count, Is.EqualTo(0));

			queue.Start();
			queue.Enqueue(queue.Stop);

			queue.WaitForAnyTaskState(TaskState.Stopped, TaskState.Finished);

			Assert.AreEqual(expectedList, list);
		}
	}
}
