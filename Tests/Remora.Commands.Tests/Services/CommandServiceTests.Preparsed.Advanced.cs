//
//  CommandServiceTests.Preparsed.Advanced.cs
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
        /// Tests advanced options.
        /// </summary>
        public class Advanced
        {
            /// <summary>
            /// Tests whether the command service can execute a command with a positional and a named parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecutePositionalAndNamedCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<AdvancedCommandGroup>()
                    .Done()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "first", new[] { "booga" } },
                    { "another", new[] { "wooga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test positional-and-named",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a named and a positional parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNamedAndPositionalCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<AdvancedCommandGroup>()
                    .Done()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "first", new[] { "wooga" } },
                    { "second", new[] { "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test named-and-positional",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with multiple mixed parameters.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteMixedCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<AdvancedCommandGroup>()
                    .Done()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "first", new[] { "wooga" } },
                    { "second", new[] { "booga" } },
                    { "third", new[] { "dooga" } },
                    { "enable", Array.Empty<string>() }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test mixed",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with multiple mixed parameters where named
            /// options are passed out of order.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteOutOfOrderMixedCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<AdvancedCommandGroup>()
                    .Done()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "second", new[] { "booga" } },
                    { "enable", Array.Empty<string>() },
                    { "third", new[] { "dooga" } },
                    { "first", new[] { "wooga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test mixed",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single greedy parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecutePositionalGreedyCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<AdvancedCommandGroup>()
                    .Done()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "greedy", new[] { "ooga", "wooga", "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test positional-greedy",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a greedy parameter, followed by a named
            /// parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecutePositionalGreedyWithNamedAfterCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<AdvancedCommandGroup>()
                    .Done()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "greedy", new[] { "ooga", "wooga", "booga" } },
                    { "second", new[] { "dooga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test positional-greedy-with-named-after",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a greedy parameter, preceded by a named
            /// parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecutePositionalGreedyWithNamedBeforeCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<AdvancedCommandGroup>()
                    .Done()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "first", new[] { "dooga" } },
                    { "greedy", new[] { "ooga", "wooga", "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test positional-greedy-with-named-before",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a greedy parameter, preceded by a named
            /// parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecutePositionalGreedyWithPositionalBeforeCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<AdvancedCommandGroup>()
                    .Done()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "first", new[] { "dooga" } },
                    { "greedy", new[] { "ooga", "wooga", "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test positional-greedy-with-positional-before",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single named greedy parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNamedGreedyCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<AdvancedCommandGroup>()
                    .Done()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "greedy", new[] { "ooga", "wooga", "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test named-greedy",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a greedy parameter, followed by a named
            /// parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNamedGreedyWithNamedAfterCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<AdvancedCommandGroup>()
                    .Done()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "greedy", new[] { "ooga", "wooga", "booga" } },
                    { "second", new[] { "dooga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test named-greedy-with-named-after",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a greedy parameter, preceded by a named
            /// parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNamedGreedyWithNamedBeforeCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<AdvancedCommandGroup>()
                    .Done()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "first", new[] { "dooga" } },
                    { "greedy", new[] { "ooga", "wooga", "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test named-greedy-with-named-before",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a greedy parameter, preceded by a named
            /// parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNamedGreedyWithPositionalBeforeCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<AdvancedCommandGroup>()
                    .Done()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "first", new[] { "dooga" } },
                    { "greedy", new[] { "ooga", "wooga", "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test named-greedy-with-positional-before",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }
        }
    }
}
