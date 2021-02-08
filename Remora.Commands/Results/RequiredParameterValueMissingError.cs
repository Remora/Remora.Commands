//
//  RequiredParameterValueMissingError.cs
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

using JetBrains.Annotations;
using Remora.Commands.Signatures;
using Remora.Results;

namespace Remora.Commands.Results
{
    /// <summary>
    /// Represents the lack of a value for a required parameter.
    /// </summary>
    [PublicAPI]
    public record RequiredParameterValueMissingError : ResultError
    {
        /// <summary>
        /// Gets the shape of the missing parameter.
        /// </summary>
        public IParameterShape ParameterShape { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredParameterValueMissingError"/> class.
        /// </summary>
        /// <param name="parameterShape">The shape of the missing parameter.</param>
        public RequiredParameterValueMissingError(IParameterShape parameterShape)
            : base($"No value was provided for the required parameter \"{parameterShape.HintName}\".")
        {
            this.ParameterShape = parameterShape;
        }
    }
}
