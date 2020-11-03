//
//  ICondition.cs
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

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

#pragma warning disable SA1402

namespace Remora.Commands.Conditions
{
    /// <summary>
    /// Represents the public API of a condition service.
    /// </summary>
    /// <typeparam name="TAttribute">The data attribute type.</typeparam>
    /// <typeparam name="TData">The data type.</typeparam>
    public interface ICondition<in TAttribute, in TData>
        where TAttribute : ConditionAttribute
    {
        /// <summary>
        /// Checks the condition against the given data, using contextual data available in the given attribute.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="data">The data.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A condition result which may or may not have succeeded.</returns>
        ValueTask<DetermineConditionResult> CheckAsync
        (
            TAttribute attribute,
            TData data,
            CancellationToken ct = default
        );
    }

    /// <summary>
    /// Represents the public API of a condition service.
    /// </summary>
    /// <typeparam name="TAttribute">The data attribute type.</typeparam>
    public interface ICondition<in TAttribute>
        where TAttribute : ConditionAttribute
    {
        /// <summary>
        /// Checks the condition against the given data, using contextual data available in the given attribute.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A condition result which may or may not have succeeded.</returns>
        ValueTask<DetermineConditionResult> CheckAsync(TAttribute attribute, CancellationToken ct = default);
    }
}
