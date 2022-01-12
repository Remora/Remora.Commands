//
//  CommandServiceTests.Preparsed.Basics.cs
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
        /// Tests basic requirements.
        /// </summary>
        public class Basics
        {
            /// <summary>
            /// Tests whether the command service can execute a parameterless command.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteParameterlessCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<BasicCommandGroup>()
                    .Finish()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "test parameterless",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single positional parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteSinglePositionalCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<BasicCommandGroup>()
                    .Finish()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "value", new[] { "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test single-positional",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single optional positional parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteSingleOptionalPositionalCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<BasicCommandGroup>()
                    .Finish()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "test single-optional-positional",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single positional parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteSingleNamedCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<BasicCommandGroup>()
                    .Finish()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "value", new[] { "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test single-named --value booga",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single positional parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteSingleOptionalNamedCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<BasicCommandGroup>()
                    .Finish()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "test single-optional-named",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single short-name parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteSingleShortNameCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<BasicCommandGroup>()
                    .Finish()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "v", new[] { "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test single-named-with-short-name",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a single short and long-name parameter.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteSingleShortAndLongNameCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<BasicCommandGroup>()
                    .Finish()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var shortValues = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "v", new[] { "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "test single-named-with-long-and-short-name",
                    shortValues,
                    services
                );

                Assert.True(executionResult.IsSuccess);

                var longValues = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "value", new[] { "booga" } }
                };

                executionResult = await commandService.TryExecuteAsync
                (
                    "test single-named-with-long-and-short-name",
                    longValues,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }
        }
    }
}
