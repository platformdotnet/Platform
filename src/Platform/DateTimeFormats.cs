// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;

namespace Platform
{
	/// <summary>
	/// Provides useful format strings for <see cref="DateTime.ToString(string)"/>.
	/// </summary>
	public static class DateTimeFormats
	{
		/// <summary>
		/// A date time in sortable format: "yyyy-MM-dd HH:mm"
		/// </summary>
		public const string FullSortableUtcDateTimeString = "yyyy-MM-dd HH:mm";

		/// <summary>
		/// A date time in sortable format: "yyyy-MM-dd HH:mm:ss"
		/// </summary>
		public const string SortableUtcDateTimeFormatWithSecondsString = "yyyy-MM-dd HH:mm:ss";

		/// <summary>
		/// A date time in sortable format: "yyyy-MM-dd HH:mm:ss.fffffff"
		/// </summary>
		public const string SortableUtcDateTimeFormatWithFractionSecondsString = "yyyy-MM-dd HH:mm:ss.fffffff";
	}
}
