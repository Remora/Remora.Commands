//
//  CommandServiceTests.Raw.Specializations.cs
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
    public static partial class CommandServiceTests
    {
        public static partial class Raw
        {
            /// <summary>
            /// Tests specialized behaviour.
            /// </summary>
            public class Specializations
            {
                /// <summary>
                /// Tests whether the command service can execute a command with a single named boolean parameter - that is,
                /// a switch.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteSwitchCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<SpecializedCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();
                    var executionResult = await commandService.TryExecuteAsync("test switch --enable", services);

                    Assert.True(executionResult.IsSuccess);

                    executionResult = await commandService.TryExecuteAsync("test switch", services);

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a single short-named boolean parameter -
                /// that is, a switch.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteShortNameSwitchCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<SpecializedCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();
                    var executionResult = await commandService.TryExecuteAsync("test switch-short-name -e", services);

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
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test switch-short-and-long-name -e",
                        services
                    );

                    Assert.True(executionResult.IsSuccess);

                    executionResult = await commandService.TryExecuteAsync
                    (
                        "test switch-short-and-long-name --enable",
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
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();
                    var executionResult = await commandService.TryExecuteAsync("test option --enable true", services);

                    Assert.True(executionResult.IsSuccess);

                    executionResult = await commandService.TryExecuteAsync("test option", services);

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
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test option-short-name -e true",
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
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test option-short-and-long-name -e true",
                        services
                    );

                    Assert.True(executionResult.IsSuccess);

                    executionResult = await commandService.TryExecuteAsync
                    (
                        "test option-short-and-long-name --enable true",
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
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test nullable-struct -v 0",
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a nullable struct parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteNullableStructCommandWithDefaultValue()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<SpecializedCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test nullable-struct-with-default -v 0",
                        services
                    );

                    Assert.True(executionResult.IsSuccess);

                    executionResult = await commandService.TryExecuteAsync
                    (
                        "test nullable-struct-with-default",
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
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test nullable-struct -v null",
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
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test nullable-reference-type -v 0",
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a nullable reference type parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteNullableReferenceTypeCommandWithDefaultValue()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<SpecializedCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test nullable-reference-type-with-default -v 0",
                        services
                    );

                    Assert.True(executionResult.IsSuccess);

                    executionResult = await commandService.TryExecuteAsync
                    (
                        "test nullable-reference-type-with-default",
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
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test nullable-reference-type -v null",
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }
            }
        }
    }
}
