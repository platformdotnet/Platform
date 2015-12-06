// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Text.RegularExpressions;

namespace Platform.Text.RegularExpressions
{
	/// <summary>
	/// A helper class for creating predicate based on a regular expression.
	/// Use the <see cref="PredicateUtils"/> to create actual regular expression predicate. 
	/// </summary>
	public class RegexBasedPredicateHelper
	{
		/// <summary>
		/// The regular expression
		/// </summary>
		public virtual Regex Regex { get; }

		/// <summary>
		/// Creates a new <see cref="RegexBasedPredicateHelper"/> from the given <see cref="regex"/>.
		/// </summary>
		/// <param name="regex">The <see cref="regex"/></param>
		protected internal RegexBasedPredicateHelper(string regex)
			: this(new Regex(regex))
		{
		}

		/// <summary>
		/// Creates a new <see cref="RegexBasedPredicateHelper"/> from the given <see cref="regex"/>.
		/// </summary>
		/// <param name="regex">The <see cref="regex"/></param>
		protected internal RegexBasedPredicateHelper(Regex regex)
		{
			this.Regex = regex;
		}

		public static bool IsRegexBasedPredicate(Predicate<string> regexBasedPredicate)
		{
			return regexBasedPredicate.Target is RegexBasedPredicateHelper;
		}

		/// <summary>
		/// Gets the regular expression from the given predicate.
		/// </summary>
		/// <param name="regexBasedPredicate"></param>
		/// <exception cref="ArgumentException">The given predicate was not created by<see cref="RegexBasedPredicateHelper"/></exception>
		/// <returns>The <see cref="Regex"/> associated with the <see cref="Predicate{T}"/></returns>
		public static Regex GetRegexFromPredicate(Predicate<string> regexBasedPredicate)
		{
			var predicateHelper = regexBasedPredicate.Target as RegexBasedPredicateHelper;

			if (predicateHelper == null)
			{
				throw new ArgumentException($"Must be a predicateHelper created from {typeof(RegexBasedPredicateHelper).Name}");
			}

			return predicateHelper.Regex;
		}

		public virtual bool Accept(string value)
		{
			return this.Regex.IsMatch(value);
		}

		public virtual Predicate<string> ToPredicate()
		{
			return this;
		}

		public static implicit operator Predicate<string>(RegexBasedPredicateHelper predicateHelper)
		{
			return predicateHelper.Accept;
		}
	}
}
