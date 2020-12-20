//
//  PositionalCollectionParameterShape.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Remora.Commands.Extensions;
using Remora.Commands.Tokenization;
using static Remora.Commands.Tokenization.TokenType;

namespace Remora.Commands.Signatures
{
    /// <summary>
    /// Represents a single value without a name.
    /// </summary>
    internal class PositionalCollectionParameterShape : PositionalParameterShape, ICollectionParameterShape
    {
        private static readonly MethodInfo EmptyArrayMethod;
        private readonly object _emptyCollection;

        /// <inheritdoc />
        public ulong? Min { get; }

        /// <inheritdoc />
        public ulong? Max { get; }

        /// <inheritdoc/>
        public override object? DefaultValue
        {
            get
            {
                if (this.Parameter.IsOptional)
                {
                    return this.Parameter.DefaultValue;
                }

                if (this.Min is null or 0)
                {
                    return _emptyCollection;
                }

                throw new InvalidOperationException();
            }
        }

        static PositionalCollectionParameterShape()
        {
            var emptyArrayMethod = typeof(Array).GetMethod(nameof(Array.Empty));
            EmptyArrayMethod = emptyArrayMethod ?? throw new MissingMethodException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionalCollectionParameterShape"/> class.
        /// </summary>
        /// <param name="parameter">The underlying parameter.</param>
        /// <param name="min">The minimum number of elements.</param>
        /// <param name="max">The maximum number of elements.</param>
        public PositionalCollectionParameterShape(ParameterInfo parameter, ulong? min, ulong? max)
            : base(parameter)
        {
            this.Min = min;
            this.Max = max;

            var elementType = this.Parameter.ParameterType.GetCollectionElementType();

            var emptyArrayMethod = EmptyArrayMethod.MakeGenericMethod(elementType);
            _emptyCollection = emptyArrayMethod.Invoke(null, null);
        }

        /// <inheritdoc />
        public override bool Matches(TokenizingEnumerator tokenizer, out ulong consumedTokens)
        {
            consumedTokens = 0;

            ulong itemCount = 0;
            while (this.Max is null || itemCount < this.Max.Value)
            {
                if (!tokenizer.MoveNext())
                {
                    break;
                }

                if (tokenizer.Current.Type != Value)
                {
                    break;
                }

                ++itemCount;
            }

            if (!(this.Min is null))
            {
                if (itemCount < this.Min.Value)
                {
                    return false;
                }
            }

            consumedTokens = itemCount;
            return true;
        }

        /// <inheritdoc/>
        public override bool Matches(KeyValuePair<string, IReadOnlyList<string>> namedValue, out bool isFatal)
        {
            isFatal = false;

            // This one is a bit of a special case. Since all parameters are named in the case of pre-bound parameters,
            // we'll use the actual parameter name as a hint to match against.
            var (name, value) = namedValue;

            if (!name.Equals(this.Parameter.Name, StringComparison.Ordinal))
            {
                return false;
            }

            var count = (ulong)value.LongCount();
            if (count < this.Min)
            {
                isFatal = true;
                return false;
            }

            if (this.Max is null)
            {
                return true;
            }

            if (count <= this.Max)
            {
                return true;
            }

            isFatal = true;
            return false;
        }

        /// <inheritdoc/>
        public override bool IsOmissible()
        {
            if (this.Parameter.IsOptional)
            {
                return true;
            }

            if (this.Min is null or 0)
            {
                return true;
            }

            return false;
        }
    }
}
