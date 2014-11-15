// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;

namespace Platform
{
	/// <summary>
	/// Provides extension methods for the <see cref="Int32"/> type.
	/// </summary>
	public static class Int32Utils
	{
		/// <summary>
		/// Gets an integer from a single hex character
		/// </summary>
		/// <param name="digit">The single hex character</param>
		/// <returns>The integer</returns>
		public static int FromHex(char digit)
		{
			if ((((digit < '0') || (digit > '9')) && ((digit < 'A') || (digit > 'F'))) && ((digit < 'a') || (digit > 'f')))
			{
				throw new ArgumentException("digit");
			}

			return FromHexNoCheck(digit);
		}
		/// <summary>
		/// Gets an integer from a single hex character without checking whether the character is
		/// a valid hex character.
		/// </summary>
		/// <param name="digit">The single hex character</param>
		/// <returns>The integer</returns>
		internal static int FromHexNoCheck(char digit)
		{
			if (digit > '9')
			{
				return (((digit <= 'F') ? (digit - 'A') : (digit - 'a')) + '\n');
			}
			return (digit - '0');
		}
	}
}
