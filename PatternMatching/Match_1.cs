﻿using System;
using LanguageExt;

namespace PatternMatching
{
	/// <summary>
	/// Represents a match expression, which doesn't yield a value.
	/// </summary>
	/// <typeparam name="TInput">The type of the input value of the expression.</typeparam>
	/// <remarks>A match expression which doesn't yield a value is also known as a match statement.</remarks>
	/// <seealso cref="Match{TInput, TOutput}" />
	/// <seealso cref="Match" />
	/// <seealso cref="MatchException" />
	public sealed class Match<TInput>
	{
		/// <summary>
		/// The list of patterns that will be matched in this expression.
		/// </summary>
		/// <remarks>
		/// This list contains value tuples which contain the pattern and the action which is to be executed
		/// if the pattern is matched successfully.
		/// </remarks>
		private readonly Lst<dynamic> patterns;

		/// <summary>
		/// Initializes a new instance of the <see cref="Match{TInput}" /> class.
		/// </summary>
		internal Match() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Match{TInput}" /> class
		/// with the specified patterns.
		/// </summary>
		/// <param name="patterns">The patterns of this expression.</param>
		private Match(Lst<dynamic> patterns)
			=> this.patterns = patterns;

		/// <summary>
		/// Returns a new matcher which includes the specified pattern and action to execute if this
		/// pattern is matched successfully.
		/// </summary>
		/// <typeparam name="TMatchResult">The type of the result of the pattern's match.</typeparam>
		/// <param name="pattern">The pattern to match with.</param>
		/// <param name="action">The action to execute if the match is successful.</param>
		/// <returns>
		/// A new matcher which includes the specified pattern and action to execute if this
		/// pattern is matched successfully.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="pattern" /> or <paramref name="action" /> is <see langword="null" />.
		/// </exception>
		public Match<TInput> Case<TMatchResult>(IPattern<TInput, TMatchResult> pattern, Action<TMatchResult> action)
			=> pattern != null
				? action != null
					? new Match<TInput>(this.patterns.Add((pattern, action)))
					: throw new ArgumentNullException(nameof(action))
				: throw new ArgumentNullException(nameof(pattern));

		/// <summary>
		/// Returns a new matcher which includes the pattern for the specified type and action to execute if this
		/// pattern is matched successfully.
		/// </summary>
		/// <typeparam name="TType">The type of the result of the pattern's match.</typeparam>
		/// <param name="action">The action to execute if the match is successful.</param>
		/// <returns>
		/// A new matcher which includes the type pattern and action to execute if this
		/// pattern is matched successfully.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="action" /> is <see langword="null" />.
		/// </exception>
		public Match<TInput> Case<TType>(Action<TType> action)
			where TType : TInput
			=> this.Case(Pattern.Type<TInput, TType>(), action);

		/// <summary>
		/// Executes the match expression on the specified input.
		/// </summary>
		/// <param name="input">The input value of the expression.</param>
		/// <returns>
		/// <see langword="true" />, if the match was successful.
		/// Otherwise, <see langword="false" />.
		/// </returns>
		/// <remarks>
		/// This method uses some non-conventional manipulation of the option type.
		/// It is used because the conventional approach uses lambda expressions,
		/// and those are forbidden to use with dynamic types.
		/// </remarks>
		/// <seealso cref="ExecuteOnStrict(TInput)" />
		public bool ExecuteOn(TInput input)
		{
			foreach (var pattern in this.patterns)
			{
				var matchResult = pattern.Item1.Match(input);
				if (matchResult.IsSome)
				{
					pattern.Item2(matchResult.ToList()[0]);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Executes the match expression strictly on the specified input.
		/// </summary>
		/// <param name="input">The input value of the expression.</param>
		/// <exception cref="MatchException">
		/// The match failed for all cases.
		/// </exception>
		/// <seealso cref="ExecuteOn(TInput)" />
		public void ExecuteOnStrict(TInput input)
		{
			bool isMatched = this.ExecuteOn(input);

			if (!isMatched)
			{
				throw new MatchException($"Cannot match {input}.");
			}
		}

		/// <summary>
		/// Compiles this matcher into a function which, when called, will match the specified value.
		/// </summary>
		/// <returns>A function which, when called, will match the specified value.</returns>
		public Func<TInput, bool> ToFunction()
			=> this.ExecuteOn;

		/// <summary>
		/// Compiles this matcher into an action which, when called, will match the specified value.
		/// </summary>
		/// <returns>An action which, when called, will match the specified value.</returns>
		public Action<TInput> ToStrictFunction()
			=> this.ExecuteOnStrict;
	}
}