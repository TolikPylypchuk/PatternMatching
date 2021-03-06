using System;

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
        /// This list contains value tuples which contain the pattern, the fallthrough behaviour,
        /// and the action which is to be executed if the pattern is matched successfully.
        /// </remarks>
        private readonly Lst<(dynamic, bool, dynamic)> patterns;

        /// <summary>
        /// The default fallthrough behaviour.
        /// </summary>
        private readonly bool fallthroughByDefault;

        /// <summary>
        /// Initializes a new instance of the <see cref="Match{TInput}" /> class.
        /// </summary>
        /// <param name="fallthroughByDefault">The default fallthrough behaviour.</param>
        internal Match(bool fallthroughByDefault)
            => this.fallthroughByDefault = fallthroughByDefault;

        /// <summary>
        /// Initializes a new instance of the <see cref="Match{TInput}" /> class
        /// with the specified patterns.
        /// </summary>
        /// <param name="patterns">The patterns of this expression.</param>
        /// <param name="fallthroughByDefault">The default fallthrough behaviour.</param>
        private Match(Lst<(dynamic, bool, dynamic)> patterns, bool fallthroughByDefault)
            => (this.patterns, this.fallthroughByDefault) = (patterns, fallthroughByDefault);

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
            => this.Case(pattern, this.fallthroughByDefault, action);

        /// <summary>
        /// Returns a new matcher which includes the specified pattern and action to execute if this
        /// pattern is matched successfully.
        /// </summary>
        /// <typeparam name="TMatchResult">The type of the result of the pattern's match.</typeparam>
        /// <param name="pattern">The pattern to match with.</param>
        /// <param name="fallthrough">The fallthrough behaviour.</param>
        /// <param name="action">The action to execute if the match is successful.</param>
        /// <returns>
        /// A new matcher which includes the specified pattern and action to execute if this
        /// pattern is matched successfully.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="pattern" /> or <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        public Match<TInput> Case<TMatchResult>(
            IPattern<TInput, TMatchResult> pattern,
            bool fallthrough,
            Action<TMatchResult> action)
            => pattern != null
                ? action != null
                    ? new Match<TInput>(this.patterns.Add((pattern, fallthrough, action)), this.fallthroughByDefault)
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
            => this.Case(this.fallthroughByDefault, action);

        /// <summary>
        /// Returns a new matcher which includes the pattern for the specified type and action to execute if this
        /// pattern is matched successfully.
        /// </summary>
        /// <typeparam name="TType">The type of the result of the pattern's match.</typeparam>
        /// <param name="fallthrough">The fallthrough behaviour.</param>
        /// <param name="action">The action to execute if the match is successful.</param>
        /// <returns>
        /// A new matcher which includes the type pattern and action to execute if this
        /// pattern is matched successfully.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        public Match<TInput> Case<TType>(bool fallthrough, Action<TType> action)
            where TType : TInput
            => this.Case(Pattern.Type<TInput, TType>(), fallthrough, action);

        /// <summary>
        /// Executes the match expression on the specified input.
        /// </summary>
        /// <param name="input">The input value of the expression.</param>
        /// <returns>
        /// <see langword="true" />, if the match was successful.
        /// Otherwise, <see langword="false" />.
        /// </returns>
        public bool ExecuteOn(TInput input)
        {
            foreach (var (pattern, _, action) in this.patterns)
            {
                var matchResult = pattern.Match(input);
                if (matchResult.IsSome)
                {
                    action(matchResult.ToList()[0]);
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
        public void ExecuteStrict(TInput input)
        {
            bool isMatched = this.ExecuteOn(input);

            if (!isMatched)
            {
                throw new MatchException($"Cannot match {input}.");
            }
        }

        /// <summary>
        /// Executes the match expression on the specified input with fallthrough.
        /// </summary>
        /// <param name="input">The input value of the expression.</param>
        /// <returns>
        /// The number of patterns that were matched successfully.
        /// </returns>
        public int ExecuteWithFallthrough(TInput input)
        {
            int numberOfMatches = 0;

            foreach ((var pattern, bool fallthrough, var action) in this.patterns)
            {
                var matchResult = pattern.Match(input);
                if (matchResult.IsSome)
                {
                    action(matchResult.ToList()[0]);
                    numberOfMatches++;

                    if (!fallthrough)
                    {
                        break;
                    }
                }
            }

            return numberOfMatches;
        }

        /// <summary>
        /// Executes the match expression strictly on the specified input with fallthrough.
        /// </summary>
        /// <param name="input">The input value of the expression.</param>
        /// <returns>
        /// The number of patterns that were matched successfully.
        /// </returns>
        /// <exception cref="MatchException">
        /// The match failed for all cases.
        /// </exception>
        public int ExecuteStrictWithFallthrough(TInput input)
        {
            int numberOfMatches = this.ExecuteWithFallthrough(input);

            if (numberOfMatches == 0)
            {
                throw new MatchException($"Cannot match {input}.");
            }

            return numberOfMatches;
        }

        /// <summary>
        /// Returns a function which, when called, will match the specified value.
        /// </summary>
        /// <returns>A function which, when called, will match the specified value.</returns>
        public Func<TInput, bool> ToFunction()
            => this.ExecuteOn;

        /// <summary>
        /// Returns an action which, when called, will match the specified value strictly.
        /// </summary>
        /// <returns>An action which, when called, will match the specified value strictly.</returns>
        public Action<TInput> ToStrictFunction()
            => this.ExecuteStrict;

        /// <summary>
        /// Returns a function which, when called, will match the specified value with fallthrough.
        /// </summary>
        /// <returns>A function which, when called, will match the specified value with fallthrough.</returns>
        public Func<TInput, int> ToFunctionWithFallthrough()
            => this.ExecuteWithFallthrough;

        /// <summary>
        /// Returns a function which, when called, will match the specified value strictly with fallthrough.
        /// </summary>
        /// <returns>An action which, when called, will match the specified value strictly with fallthrough.</returns>
        public Func<TInput, int> ToStrictFunctionWithFallthrough()
            => this.ExecuteStrictWithFallthrough;
    }
}
