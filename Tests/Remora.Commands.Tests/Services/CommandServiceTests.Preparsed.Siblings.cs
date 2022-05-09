//
//  CommandServiceTests.Preparsed.Siblings.cs
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
        /// Tests sibling groups and commands.
        /// </summary>
        public class Siblings
        {
            /// <summary>
            /// Tests whether the command service can execute a command that has the same name as a sibling group.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
            [Fact]
            public async Task CanExecuteCommandWithSameNameAsSiblingGroup()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SiblingOverloadCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();

                var executionResult = await commandService.TryExecuteAsync
                (
                    "sibling",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command that is in a group with the same name as a
            /// sibling command.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
            [Fact]
            public async Task CanExecuteCommandInNestedGroupWithSameNameAsSiblingCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SiblingOverloadCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();

                var executionResult = await commandService.TryExecuteAsync
                (
                    "sibling nested",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a parameterized command that has the same name as a
            /// sibling group.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
            [Fact]
            public async Task CanExecuteParameterizedCommandWithSameNameAsSiblingGroup()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SiblingOverloadCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "value", new[] { "0" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "parameter-sibling",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a parameterized command that is in a group with the same name as a
            /// sibling command.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
            [Fact]
            public async Task CanExecuteParameterizedCommandInNestedGroupWithSameNameAsSiblingCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SiblingOverloadCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "value", new[] { "0" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "parameter-sibling nested",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service picks the most specific available command when a command with lesser
            /// specificity matches as well.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
            [Fact]
            public async Task PicksMostSpecificAlternativeWhenSiblingMatches()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<SiblingSpecificityCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();

                var executionResult = await commandService.TryExecuteAsync
                (
                    "sibling specific",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }
        }
    }
}
