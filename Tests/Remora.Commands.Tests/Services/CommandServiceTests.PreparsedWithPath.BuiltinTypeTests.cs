//
//  CommandServiceTests.PreparsedWithPath.BuiltinTypeTests.cs
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
using System.Numerics;
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
        public static partial class PreparsedWithPath
        {
            /// <summary>
            /// Tests commands that use builtin type conversions.
            /// </summary>
            public class BuiltinTypeTests
            {
                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="bool"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteBoolCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "true" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "bool" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="char"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteCharCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "char" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="sbyte"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteSByteCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "sbyte" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="byte"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteByteCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "byte" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="short"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteInt16Command()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "short" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="ushort"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteUInt16Command()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "ushort" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="int"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteInt32Command()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "int" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="uint"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteUInt32Command()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "uint" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="long"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteInt64Command()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "long" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="ulong"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteUInt64Command()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "ulong" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="float"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteSingleCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "float" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="double"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteDoubleCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "double" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="decimal"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteDecimalCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "decimal" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="BigInteger"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteBigIntegerCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "big-integer" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="DateTimeOffset"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteDateTimeOffsetCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "2020/09/1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "date-time-offset" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="DateTime"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteDateTimeCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "2020/09/1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "date-time" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a <see cref="DateTime"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteTimeSpanCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "0" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "time-span" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a
                /// <see cref="BuiltinTypeCommandGroup.TestEnum"/> parameter.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteEnumCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandTree()
                            .WithCommandGroup<BuiltinTypeCommandGroup>()
                            .Done()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "wooga" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        new[] { "test", "enum" },
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }
            }
        }
    }
}
