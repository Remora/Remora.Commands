//
//  PositionalParameterShape.cs
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

using System.Collections.Generic;
using System.Reflection;
using Remora.Commands.Tokenization;
using Remora.Commands.Trees;
using static Remora.Commands.Tokenization.TokenType;

namespace Remora.Commands.Signatures
{
    /// <summary>
    /// Represents a single value without a name.
    /// </summary>
    public class PositionalParameterShape : IParameterShape
    {
        /// <inheritdoc />
        public ParameterInfo Parameter { get; }

        /// <inheritdoc/>
        public virtual object? DefaultValue => this.Parameter.DefaultValue;

        /// <inheritdoc/>
        public string HintName => this.Parameter.Name;

        /// <inheritdoc/>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionalParameterShape"/> class.
        /// </summary>
        /// <param name="parameter">The underlying parameter.</param>
        /// <param name="description">The description of the parameter.</param>
        public PositionalParameterShape(ParameterInfo parameter, string description = "No description set.")
        {
            this.Parameter = parameter;
            this.Description = description;
        }

        /// <inheritdoc />
        public virtual bool Matches
        (
            TokenizingEnumerator tokenizer,
            out ulong consumedTokens,
            TreeSearchOptions? searchOptions = null
        )
        {
            consumedTokens = 0;

            if (!tokenizer.MoveNext())
            {
                return false;
            }

            if (tokenizer.Current.Type != Value)
            {
                return false;
            }

            consumedTokens = 1;
            return true;
        }

        /// <inheritdoc/>
        public virtual bool Matches
        (
            KeyValuePair<string, IReadOnlyList<string>> namedValue,
            out bool isFatal,
            TreeSearchOptions? searchOptions = null
        )
        {
            searchOptions ??= new TreeSearchOptions();
            isFatal = false;

            // This one is a bit of a special case. Since all parameters are named in the case of pre-bound parameters,
            // we'll use the actual parameter name as a hint to match against.
            var (name, value) = namedValue;

            if (!name.Equals(this.Parameter.Name, searchOptions.KeyComparison))
            {
                return false;
            }

            if (value.Count == 1)
            {
                return true;
            }

            isFatal = true;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool IsOmissible(TreeSearchOptions? searchOptions = null) => this.Parameter.IsOptional;
    }
}
