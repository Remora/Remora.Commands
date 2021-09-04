//
//  ParameterInfoExtensionTests.cs
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
using JetBrains.Annotations;
using Remora.Commands.Extensions;
using Xunit;

// ReSharper disable SA1600
#pragma warning disable 1591, SA1600

namespace Remora.Commands.Tests.Extensions
{
    /// <summary>
    /// Tests the <see cref="ParameterInfoExtensions"/> class.
    /// </summary>
    public static class ParameterInfoExtensionTests
    {
        public class AllowsNull
        {
            /// <summary>
            /// Does nothing. This method is used to get various nullability-annotated parameters.
            /// </summary>
            [PublicAPI]
            private void Method
            (
                int valueType,
                int? nullableValueType,
                string referenceType,
                string? nullableReferenceType,
                List<int> valueTypeList,
                List<int>? nullableValueTypeList,
                List<int?> nonNullableNullableValueTypeList,
                List<string> referenceTypeList,
                List<string>? nullableReferenceTypeList,
                List<string?> nonNullableNullableReferenceTypeList
            )
            {
            }

            [Fact]
            public void ReturnsFalseForValueType()
            {
                var parameter = GetType().GetMethod
                    (
                        nameof(Method),
                        BindingFlags.Instance | BindingFlags.NonPublic
                    )!
                    .GetParameters()[0];

                Assert.NotNull(parameter);
                Assert.False(parameter.AllowsNull());
            }

            [Fact]
            public void ReturnsTrueForNullableValueType()
            {
                var parameter = GetType().GetMethod
                    (
                        nameof(Method),
                        BindingFlags.Instance | BindingFlags.NonPublic
                    )!
                    .GetParameters()[1];

                Assert.NotNull(parameter);
                Assert.True(parameter.AllowsNull());
            }

            [Fact]
            public void ReturnsFalseForReferenceType()
            {
                var parameter = GetType().GetMethod
                    (
                        nameof(Method),
                        BindingFlags.Instance | BindingFlags.NonPublic
                    )!
                    .GetParameters()[2];

                Assert.NotNull(parameter);
                Assert.False(parameter.AllowsNull());
            }

            [Fact]
            public void ReturnsTrueForNullableReferenceType()
            {
                var parameter = GetType().GetMethod
                    (
                        nameof(Method),
                        BindingFlags.Instance | BindingFlags.NonPublic
                    )!
                    .GetParameters()[3];

                Assert.NotNull(parameter);
                Assert.True(parameter.AllowsNull());
            }

            [Fact]
            public void ReturnsFalseForGenericTypeWithValueTypeArgument()
            {
                var parameter = GetType().GetMethod
                    (
                        nameof(Method),
                        BindingFlags.Instance | BindingFlags.NonPublic
                    )!
                    .GetParameters()[4];

                Assert.NotNull(parameter);
                Assert.False(parameter.AllowsNull());
            }

            [Fact]
            public void ReturnsTrueForNullableGenericTypeWithValueTypeArgument()
            {
                var parameter = GetType().GetMethod
                    (
                        nameof(Method),
                        BindingFlags.Instance | BindingFlags.NonPublic
                    )!
                    .GetParameters()[5];

                Assert.NotNull(parameter);
                Assert.True(parameter.AllowsNull());
            }

            [Fact]
            public void ReturnsFalseForNonNullableGenericTypeWithNullableValueTypeArgument()
            {
                var parameter = GetType().GetMethod
                    (
                        nameof(Method),
                        BindingFlags.Instance | BindingFlags.NonPublic
                    )!
                    .GetParameters()[6];

                Assert.NotNull(parameter);
                Assert.False(parameter.AllowsNull());
            }

            [Fact]
            public void ReturnsFalseForGenericTypeWithReferenceTypeArgument()
            {
                var parameter = GetType().GetMethod
                    (
                        nameof(Method),
                        BindingFlags.Instance | BindingFlags.NonPublic
                    )!
                    .GetParameters()[7];

                Assert.NotNull(parameter);
                Assert.False(parameter.AllowsNull());
            }

            [Fact]
            public void ReturnsTrueForNullableGenericTypeWithReferenceTypeArgument()
            {
                var parameter = GetType().GetMethod
                    (
                        nameof(Method),
                        BindingFlags.Instance | BindingFlags.NonPublic
                    )!
                    .GetParameters()[8];

                Assert.NotNull(parameter);
                Assert.True(parameter.AllowsNull());
            }

            [Fact]
            public void ReturnsFalseForNonNullableGenericTypeWithNullableReferenceTypeArgument()
            {
                var parameter = GetType().GetMethod
                    (
                        nameof(Method),
                        BindingFlags.Instance | BindingFlags.NonPublic
                    )!
                    .GetParameters()[9];

                Assert.NotNull(parameter);
                Assert.False(parameter.AllowsNull());
            }
        }
    }
}
