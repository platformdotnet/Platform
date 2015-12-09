// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Platform.IO;

namespace Platform
{
	/// <summary>
	/// Provides useful methods for the Environment.
	/// </summary>
	public static class EnvironmentUtils
	{
		[StructLayoutAttribute(LayoutKind.Sequential)]
		private struct SYSTEMTIME
		{
			public short year;
			public short month;
			public short dayOfWeek;
			public short day;
			public short hour;
			public short minute;
			public short second;
			public short milliseconds;
		}

		[DllImport("kernel32.dll")]
		private static extern bool SetLocalTime(ref SYSTEMTIME time);

		[DllImport("kernel32.dll")]
		private static extern bool SetSystemTime(ref SYSTEMTIME time);

		/// <summary>
		/// Sets the system time to the provided value.
		/// </summary>
		/// <param name="dateTime">The <see cref="DateTime"/> value to set the system clock to.</param>
		public static void SetSystemTime(DateTime dateTime)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT
				|| Environment.OSVersion.Platform == PlatformID.Win32S
				|| Environment.OSVersion.Platform == PlatformID.Win32Windows
				|| Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				Win32SetSystemTime(dateTime);
			}
			else
			{
				// Assume Linux (not quite right).

				LinuxSetSystemTime(dateTime);
			}
		}

		/// <summary>
		/// Sets the system time for Linux systems.
		/// </summary>
		/// <param name="dateTime">The <see cref="DateTime"/></param>
		internal static void LinuxSetSystemTime(DateTime dateTime)
		{
			var dateTimeString = dateTime.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss");

			var startInfo = new ProcessStartInfo("/bin/date", $@"--utc --set=""{dateTimeString}""")
			{
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			var process = Process.Start(startInfo);

			process.StandardOutput.DiscardToEnd();
			process.StandardError.DiscardToEnd();
			process.WaitForExit();

			startInfo = new ProcessStartInfo("/sbin/hwclock", "--utc --systohc");

			startInfo.UseShellExecute = false;			
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;

			process = Process.Start(startInfo);
			process.StandardOutput.DiscardToEnd();
			process.StandardError.DiscardToEnd();
			process.WaitForExit();
		}

		/// <summary>
		/// Sets the system time for Windows systems.
		/// </summary>
		/// <param name="dateTime">The <see cref="DateTime"/></param>
		internal static void Win32SetSystemTime(DateTime dateTime)
		{
			SYSTEMTIME st;

			DateTime trts = dateTime;

			trts = dateTime.ToUniversalTime();

			st.year = (short)trts.Year;
			st.month = (short)trts.Month;
			st.dayOfWeek = (short)trts.DayOfWeek;
			st.day = (short)trts.Day;
			st.hour = (short)trts.Hour;
			st.minute = (short)trts.Minute;
			st.second = (short)trts.Second;
			st.milliseconds = (short)trts.Millisecond;

			SetSystemTime(ref st);
		}

		/// <summary>
		/// Gets the path that the current application resides in
		/// </summary>
		public static string ApplicationPath
		{
			get
			{
				// GetEntryAssembly may return null if the application is started
				// from native code (by TestDrive.NET for example)

				var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

				var uriCodebase = new Uri(assembly.CodeBase);

				return Path.GetDirectoryName(uriCodebase.LocalPath);
			}
		}
	}
}
