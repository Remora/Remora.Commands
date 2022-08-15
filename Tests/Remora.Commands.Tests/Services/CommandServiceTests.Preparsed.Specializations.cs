//
//  CommandServiceTests.Preparsed.Specializations.cs
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Commands.Services;
using Remora.Commands.Tests.Data.Modules;
using Xunit;

namespace Remora.Commands.Tests.Services;

public static partial class CommandServiceTests
{
    public static partial class Preparsed
    {
        /// <summary>
        /// Tests specialized behaviour.
        /// </summary>
        public class Specializations
        {
            /// <summary>
            /// Tests whether the command service can execute a command with a single named boolean parameter - that
            /// is, a switch.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteSwitchCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SpecializedCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "enable", Array.Empty<string>() }
                };

                var executionResult = await commandService.TryExecuteAsync("test switch", values, services);

                Assert.True(executionResult.IsSuccess);

                values = new Dictionary<string, IReadOnlyList<string>>();
                executionResult = await commandService.TryExecuteAsync("test switch", values, services);

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single short-named boolean parameter
            /// - that is, a switch.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteShortNameSwitchCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SpecializedCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "e", Array.Empty<string>() }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test switch-short-name",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single short and long-named boolean
            /// parameter - that is, a switch.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteShortAndLongNameSwitchCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SpecializedCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "e", Array.Empty<string>() }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test switch-short-and-long-name",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);

                values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "enable", Array.Empty<string>() }
                };

                executionResult = await commandService.TryExecuteAsync
                (
                    "test switch-short-and-long-name",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single named boolean parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNamedBoolCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SpecializedCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "enable", new[] { "true" } }
                };

                var executionResult = await commandService.TryExecuteAsync("test option", values, services);

                Assert.True(executionResult.IsSuccess);

                values = new Dictionary<string, IReadOnlyList<string>>();
                executionResult = await commandService.TryExecuteAsync("test option", values, services);

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single short-named boolean parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteShortNameNamedBoolCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SpecializedCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "e", new[] { "true" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test option-short-name",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single short and long-named boolean
            /// parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteShortAndLongNameNamedBoolCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SpecializedCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "e", new[] { "true" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test option-short-and-long-name",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);

                values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "enable", new[] { "true" } }
                };

                executionResult = await commandService.TryExecuteAsync
                (
                    "test option-short-and-long-name",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a nullable struct parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNullableStructCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SpecializedCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "v", new[] { "0" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test nullable-struct",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a nullable struct parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNullableStructWithCommandDefaultValue()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SpecializedCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "v", new[] { "0" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test nullable-struct-with-default",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);

                values = new Dictionary<string, IReadOnlyList<string>>();

                executionResult = await commandService.TryExecuteAsync
                (
                    "test nullable-struct-with-default",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a nullable struct parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNullableStructCommandWithNullLiteral()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SpecializedCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "v", new[] { "null" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test nullable-struct",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a nullable reference type parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNullableReferenceTypeCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SpecializedCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "v", new[] { "wooga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test nullable-reference-type",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a nullable reference type parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNullableReferenceTypeWithCommandDefaultValue()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SpecializedCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "v", new[] { "wooga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test nullable-reference-type-with-default",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);

                values = new Dictionary<string, IReadOnlyList<string>>();

                executionResult = await commandService.TryExecuteAsync
                (
                    "test nullable-reference-type-with-default",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a nullable reference type parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNullableReferenceTypeCommandWithNullLiteral()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SpecializedCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "v", new[] { "null" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test nullable-reference-type",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }
        }
    }
}
