//
//  ParameterConditionAttribute.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
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
using Remora.Commands.Conditions;

namespace Remora.Commands.Tests.Data.Attributes;

/// <summary>
/// Represents simple condition data attached to a Parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class ParameterConditionAttribute : ConditionAttribute
{
    /// <summary>
    /// Gets the data of the attribute.
    /// </summary>
    public string Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterConditionAttribute"/> class.
    /// </summary>
    /// <param name="data">The data.</param>
    public ParameterConditionAttribute(string data)
    {
        this.Data = data;
    }
}
