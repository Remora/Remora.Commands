//
//  TokenizingEnumerator.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using JetBrains.Annotations;

namespace Remora.Commands.Tokenization
{
    /// <summary>
    /// Tokenizes a char-containing <see cref="ReadOnlySpan{T}"/> into a set of names and values.
    /// </summary>
    [PublicAPI]
    public ref struct TokenizingEnumerator
    {
        private readonly TokenizerOptions _tokenizerOptions;
        private bool _isInCombinedShortNameSegment;
        private ReadOnlySpan<char> _segment;
        private SpanSplitEnumerator _splitEnumerator;
        private Token _current;

        /// <summary>
        /// Gets the current value of the enumerator.
        /// </summary>
        public readonly Token Current => _current;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenizingEnumerator"/> struct.
        /// </summary>
        /// <param name="value">The value to tokenize.</param>
        /// <param name="tokenizerOptions">The tokenizer options.</param>
        public TokenizingEnumerator(ReadOnlySpan<char> value, TokenizerOptions? tokenizerOptions = null)
        {
            _tokenizerOptions = tokenizerOptions ?? new TokenizerOptions();

            _isInCombinedShortNameSegment = default;
            _segment = default;
            _splitEnumerator = new SpanSplitEnumerator(value, _tokenizerOptions);
            _current = default;
        }

        /// <summary>
        /// Gets the enumerator for this type.
        /// </summary>
        /// <returns>The instance itself.</returns>
        public readonly TokenizingEnumerator GetEnumerator() => this;

        /// <summary>
        /// Attempts to advance the enumerator.
        /// </summary>
        /// <returns>true if the enumerator advanced and a new value is available; otherwise, false.</returns>
        public bool MoveNext()
        {
            if (_segment.Length == 0)
            {
                if (!_splitEnumerator.MoveNext())
                {
                    return false;
                }

                _isInCombinedShortNameSegment = false;
                _segment = _splitEnumerator.Current;
            }

            var span = _segment;
            var remainder = ReadOnlySpan<char>.Empty;

            var type = TokenType.Value;
            if (span.StartsWith("--"))
            {
                type = TokenType.LongName;
                span = span.Slice(2);
            }
            else if (span.StartsWith("-"))
            {
                // Workaround; names must be identifiers (i.e, not start with a digit)
                if (span.Length <= 1 || !char.IsDigit(span[1]))
                {
                    type = TokenType.ShortName;
                    span = span.Slice(1);
                }
            }
            else if (span.StartsWith("="))
            {
                span = span.Slice(1);
            }
            else if (_isInCombinedShortNameSegment)
            {
                type = TokenType.ShortName;
            }

            if (type != TokenType.Value)
            {
                if (type == TokenType.ShortName && span.Length > 1 && char.IsLetterOrDigit(span[1]))
                {
                    // A combined short-name option, it looks like. We'll return here.
                    _isInCombinedShortNameSegment = true;

                    _current = new Token(type, span.Slice(0, 1));
                    _segment = span.Slice(1);

                    return true;
                }

                var assignmentIndex = span.IndexOf('=');
                if (assignmentIndex > 0)
                {
                    remainder = span.Slice(assignmentIndex);
                    span = span.Slice(0, assignmentIndex);
                }
            }

            // Remove quotes, if any
            if (!span.IsEmpty && !_tokenizerOptions.RetainQuotationMarks)
            {
                foreach ((string start, string end) in Quotations.Pairs)
                {
                    if (span[0] != start[0])
                    {
                        continue;
                    }

                    if (span[^1] == end[0])
                    {
                        span = span.Slice(1, span.Length - 2);
                    }
                }
            }

            _current = new Token(type, span);
            _segment = remainder;

            return true;
        }
    }
}
