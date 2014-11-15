// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;

namespace Platform
{
	public class ConverterUtils<I, O>
	{
		public static Converter<I, O> NoConvert
		{
			get
			{
				return noConvert;
			}
		}

		private static readonly Converter<I, O> noConvert = value => (O) (object) value;
	}
}