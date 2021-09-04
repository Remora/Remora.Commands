//
//  ParameterInfoExtensions.cs
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
using JetBrains.Annotations;

namespace Remora.Commands.Extensions
{
    /// <summary>
    /// Defines extension methods for the <see cref="ParameterInfo"/> class.
    /// </summary>
    [PublicAPI]
    public static class ParameterInfoExtensions
    {
        /// <summary>
        /// Enumerates the various nullability possibilities.
        /// </summary>
        private enum Nullability
        {
            Oblivious = 0,
            NotNull = 1,
            Nullable = 2
        }

        /// <summary>
        /// Determines whether the given parameter allows null as a value.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>true if the parameter allows null; otherwise, false.</returns>
        public static bool AllowsNull(this ParameterInfo parameter)
        {
            var parameterType = parameter.ParameterType;
            if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return true;
            }

            var nullableAttributeType = Type.GetType("System.Runtime.CompilerServices.NullableAttribute");
            if (nullableAttributeType is null)
            {
                // If we don't have access to nullability attributes, assume that we're not in a nullable context.
                return !parameterType.IsValueType;
            }

            // We're in a nullable context, and we can assume that the lack of an attribute means the parameter is not
            // nullable.
            var nullableAttribute = parameter.CustomAttributes.FirstOrDefault
            (
                s => s.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute"
            );

            var topLevelNullability = Nullability.Oblivious;

            if (nullableAttribute is not null)
            {
                var nullableArgument = nullableAttribute.ConstructorArguments.Single();
                switch (nullableArgument.Value)
                {
                    case byte singleArg:
                    {
                        topLevelNullability = (Nullability)singleArg;
                        break;
                    }
                    case IReadOnlyCollection<CustomAttributeTypedArgument> multiArg:
                    {
                        if (multiArg.First().Value is not byte firstArg)
                        {
                            throw new InvalidOperationException();
                        }

                        topLevelNullability = (Nullability)firstArg;
                        break;
                    }
                    default:
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            switch (topLevelNullability)
            {
                case Nullability.Oblivious:
                {
                    // Check the context instead
                    var nullableContextAttribute = parameter.Member.CustomAttributes.FirstOrDefault
                    (
                        s => s.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute"
                    );

                    if (nullableContextAttribute is null)
                    {
                        return !parameterType.IsValueType;
                    }

                    var nullableArgument = nullableContextAttribute.ConstructorArguments.Single();
                    if (nullableArgument.Value is byte singleArg)
                    {
                        return singleArg == 2;
                    }

                    throw new InvalidOperationException();
                }
                case Nullability.NotNull:
                {
                    return false;
                }
                case Nullability.Nullable:
                {
                    return true;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
