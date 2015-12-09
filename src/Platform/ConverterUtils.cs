// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;

namespace Platform
{
	public class ConverterUtils<I, O>
	{
		public static Converter<I, O> NoConvert { get; } = value => (O)(object)value;
	}
}