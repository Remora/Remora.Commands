//
//  CommandServiceTests.Raw.Advanced.cs
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

using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Commands.Services;
using Remora.Commands.Tests.Data.Modules;
using Xunit;

namespace Remora.Commands.Tests.Services
{
    public partial class CommandServiceTests
    {
        public partial class Raw
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
                        .AddCommandGroup<AdvancedCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test positional-and-named booga --another wooga",
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
                        .AddCommandGroup<AdvancedCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test named-and-positional --first wooga booga ",
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
                        .AddCommandGroup<AdvancedCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test mixed --first wooga booga --third dooga --enable",
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
                        .AddCommandGroup<AdvancedCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test mixed booga --enable --third dooga --first wooga",
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }
            }
        }
    }
}
