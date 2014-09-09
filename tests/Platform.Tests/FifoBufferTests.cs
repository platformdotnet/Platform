using System.Linq;
using System.Text;
using NUnit.Framework;
using Platform.Collections;
using Platform.IO;

namespace Platform.Tests
{
	[TestFixture]
	public class FifoBufferTests
	{
		private static byte[] GetBytes(string s)
		{
			return Encoding.ASCII.GetBytes(s);
		}

		private static string GetString(byte[] bytes)
		{
			return Encoding.ASCII.GetString(bytes);
		}

		[Test]
		public void Test_Single_Size_Buffer()
		{
			var ringBuffer = new BoundedFifoBuffer<byte>(1);

			for (var i = 0; i < 100; i++)
			{
				ringBuffer.Write(GetBytes("A"));
				Assert.AreEqual("A", GetString(ringBuffer.ReadToArray()));
				ringBuffer.Write(GetBytes("B"));
				Assert.AreEqual("B", GetString(ringBuffer.ReadToArray()));
			}
		}

		[Test]
		public void Test_Single_Size_Circular_Buffer()
		{
			var ringBuffer = new CircularFifoBuffer<byte>(1);

			for (var i = 0; i < 100; i++)
			{
				ringBuffer.Write(GetBytes("ABC"));
				Assert.AreEqual("C", GetString(ringBuffer.ReadToArray()));
				ringBuffer.Write(GetBytes("D"));
				Assert.AreEqual("D", GetString(ringBuffer.ReadToArray()));
			}
		}

		[Test]
		public void Test_Simple_RingBuffer_Test_Read_Write_With_Dri1()
		{
			var ringBuffer = new BoundedFifoBuffer<byte>(5);

			for (var i = 0; i < 100; i++)
			{
				ringBuffer.Write(GetBytes("ABC"));
				Assert.AreEqual("ABC", GetString(ringBuffer.ReadToArray()));
				ringBuffer.Write(GetBytes("DEF"));
				Assert.AreEqual("DEF", GetString(ringBuffer.ReadToArray()));
			}
		}

		[Test]
		public void Test_Simple_RingBuffer_Test_Read_Write_With_Drift2()
		{
			var ringBuffer = new BoundedFifoBuffer<byte>(5);

			for (var i = 0; i < 100; i++)
			{
				ringBuffer.Write(GetBytes("ABC"));
				Assert.AreEqual("ABC", GetString(ringBuffer.ReadToArray()));
				ringBuffer.Write(GetBytes("DEF"));
				Assert.AreEqual('D', ringBuffer[0]);
				Assert.AreEqual('E', ringBuffer[1]);
				Assert.AreEqual('F', ringBuffer[2]);
				ringBuffer.Clear();
			}
		}

		[Test, ExpectedException(typeof(BufferOverflowException))]
		public void Test_Overflow()
		{
			var ringBuffer = new BoundedFifoBuffer<byte>(5);

			ringBuffer.Write(GetBytes("ABC"));
			ringBuffer.Write(GetBytes("DEF"));
		}

		[Test, ExpectedException(typeof(BufferOverflowException))]
		public void Test_Overflow_On_Initial()
		{
			var ringBuffer = new BoundedFifoBuffer<byte>(5);

			ringBuffer.Write(GetBytes("ABCDEF"));
		}

		[Test]
		public void Test_Not_Overflow()
		{
			var ringBuffer = new BoundedFifoBuffer<byte>(5);

			ringBuffer.Write(GetBytes("ABC"));
			ringBuffer.Write(GetBytes("DE"));
		}

		[Test]
		public void Test_Circular1()
		{
			var ringBuffer = new CircularFifoBuffer<byte>(5);

			ringBuffer.Write(GetBytes("ABC"));
			ringBuffer.Write(GetBytes("DE"));
			ringBuffer.Write(GetBytes("F"));

			Assert.AreEqual("BCDEF", GetString(ringBuffer.ReadToArray()));
		}

		[Test]
		public void Test_Circular1_Enumerator()
		{
			var ringBuffer = new CircularFifoBuffer<byte>(5);

			ringBuffer.Write(GetBytes("ABC"));
			ringBuffer.Write(GetBytes("DE"));
			ringBuffer.Write(GetBytes("F"));

			Assert.AreEqual("BCDEF", GetString(ringBuffer.ToArray()));
		}

		[Test]
		public void Test_Circular1_As_List()
		{
			var ringBuffer = new CircularFifoBuffer<byte>(5);

			ringBuffer.Write(GetBytes("ABC"));
			ringBuffer.Write(GetBytes("DE"));
			ringBuffer.Write(GetBytes("F"));

			Assert.AreEqual('B', ringBuffer[0]);
			Assert.AreEqual('C', ringBuffer[1]);
			Assert.AreEqual('D', ringBuffer[2]);
			Assert.AreEqual('E', ringBuffer[3]);
			Assert.AreEqual('F', ringBuffer[4]);

			Assert.AreEqual(5, ringBuffer.Count);
		}

		[Test]
		public void Test_Circular2()
		{
			var ringBuffer = new CircularFifoBuffer<byte>(5);

			ringBuffer.Write(GetBytes("ABC"));
			ringBuffer.Write(GetBytes("DEF"));

			Assert.AreEqual("BCDEF", GetString(ringBuffer.ReadToArray()));
		}

		[Test]
		public void Test_Circular_Massive1()
		{
			var ringBuffer = new CircularFifoBuffer<byte>(5);

			ringBuffer.Write(GetBytes("ABC"));
			ringBuffer.Write(GetBytes("DE"));
			ringBuffer.Write(GetBytes("1234567890"));

			Assert.AreEqual("67890", GetString(ringBuffer.ReadToArray()));
		}

		[Test]
		public void Test_Circular_Massive2()
		{
			var ringBuffer = new CircularFifoBuffer<byte>(5);

			ringBuffer.Write(GetBytes("ABC"));
			ringBuffer.Write(GetBytes("DE"));
			ringBuffer.Write(GetBytes("123456789"));

			Assert.AreEqual("56789", GetString(ringBuffer.ReadToArray()));
		}

		[Test]
		public void Test_Circular_Massive1A()
		{
			var ringBuffer = new CircularFifoBuffer<byte>(5);

			ringBuffer.Write(GetBytes("ABC"));
			Assert.AreEqual("ABC", GetString(ringBuffer.ReadToArray()));

			ringBuffer.Write(GetBytes("ABC"));
			ringBuffer.Write(GetBytes("DE"));
			ringBuffer.Write(GetBytes("1234567890"));

			Assert.AreEqual("67890", GetString(ringBuffer.ReadToArray()));
		}

		[Test]
		public void Test_Circular_Massive2B()
		{
			var ringBuffer = new CircularFifoBuffer<byte>(5);

			ringBuffer.Write(GetBytes("ABC"));
			Assert.AreEqual("ABC", GetString(ringBuffer.ReadToArray()));

			ringBuffer.Write(GetBytes("ABC"));
			ringBuffer.Write(GetBytes("DE"));
			ringBuffer.Write(GetBytes("123456789"));

			Assert.AreEqual("56789", GetString(ringBuffer.ReadToArray()));
		}

		[Test]
		public void Test_Circular_FirstItem()
		{
			var ringBuffer = new CircularFifoBuffer<byte>(5);

			ringBuffer.Write(GetBytes("ABCDEFG"));
			
			Assert.AreEqual("CDEFG", GetString(ringBuffer.ReadToArray()));
		}
	}
}
