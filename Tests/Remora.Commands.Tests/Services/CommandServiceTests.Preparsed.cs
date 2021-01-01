//
//  CommandServiceTests.Preparsed.cs
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
using Remora.Commands.Tests.Data.Conditions;
using Remora.Commands.Tests.Data.Modules;
using Remora.Results;
using Xunit;

namespace Remora.Commands.Tests.Services
{
    public partial class CommandServiceTests
    {
        /// <summary>
        /// Tests functionality of preparsed operations.
        /// </summary>
        public class Preparsed
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
                        .AddCommandGroup<BasicCommandGroup>()
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
                        .AddCommandGroup<BasicCommandGroup>()
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
                        .AddCommandGroup<BasicCommandGroup>()
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
                        .AddCommandGroup<BasicCommandGroup>()
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
                        .AddCommandGroup<BasicCommandGroup>()
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
                        .AddCommandGroup<BasicCommandGroup>()
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
                        .AddCommandGroup<BasicCommandGroup>()
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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "true" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test bool", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test char", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test sbyte", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test byte", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test short", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test ushort", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test int", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test uint", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test long", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test ulong", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test float", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test double", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test decimal", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test big-integer", values, services);

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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "2020/09/1" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test date-time-offset",
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
                        .AddCommandGroup<BuiltinTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "wooga" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync("test enum", values, services);

                    Assert.True(executionResult.IsSuccess);
                }
            }

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
                        .AddCommandGroup<SpecializedCommandGroup>()
                        .BuildServiceProvider();

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
                        .AddCommandGroup<SpecializedCommandGroup>()
                        .BuildServiceProvider();

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
                        .AddCommandGroup<SpecializedCommandGroup>()
                        .BuildServiceProvider();

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
                        .AddCommandGroup<SpecializedCommandGroup>()
                        .BuildServiceProvider();

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
                        .AddCommandGroup<SpecializedCommandGroup>()
                        .BuildServiceProvider();

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
                        .AddCommandGroup<SpecializedCommandGroup>()
                        .BuildServiceProvider();

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
            }

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
                        .AddCommandGroup<AdvancedCommandGroup>()
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
                        .AddCommandGroup<AdvancedCommandGroup>()
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
                        .AddCommandGroup<AdvancedCommandGroup>()
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
            }

            /// <summary>
            /// Tests collection arguments.
            /// </summary>
            public class Collections
            {
                /// <summary>
                /// Tests whether the command service can execute a command with a positional collection.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecutePositionalCollectionCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandGroup<CollectionCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "values", new[] { "ra", "ra", "rasputin" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test positional-collection",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a named collection.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteNamedCollectionCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandGroup<CollectionCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "values", new[] { "ra", "ra", "rasputin" } }
                    };
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test named-collection",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a positional collection and named value.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteNamedValueAndPositionalCollectionCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandGroup<CollectionCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "values", new[] { "ra", "rasputin" } },
                        { "named", new[] { "ra" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test positional-collection-and-named-value",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a positional collection and named value.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteOutOfOrderNamedValueAndPositionalCollectionCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandGroup<CollectionCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "named", new[] { "ra" } },
                        { "values", new[] { "ra", "rasputin" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test positional-collection-and-named-value",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a min-constrained collection.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteCollectionWithMinCountCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandGroup<CollectionCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "values", new[] { "ra" } },
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test collection-with-min-count",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);

                    values = new Dictionary<string, IReadOnlyList<string>>();
                    executionResult = await commandService.TryExecuteAsync
                    (
                        "test collection-with-min-count",
                        values,
                        services
                    );

                    Assert.False(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a max-constrained collection.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteCollectionWithMaxCountCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandGroup<CollectionCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test collection-with-max-count",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);

                    values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "values", new[] { "ra", "ra", "rasputin" } },
                    };

                    executionResult = await commandService.TryExecuteAsync
                    (
                        "test collection-with-max-count ra ra rasputin",
                        values,
                        services
                    );

                    Assert.False(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a min and max-constrained collection.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteCollectionWithMinAndMaxCountCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandGroup<CollectionCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "values", new[] { "ra" } },
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test collection-with-min-and-max-count",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);

                    values = new Dictionary<string, IReadOnlyList<string>>();
                    executionResult = await commandService.TryExecuteAsync
                    (
                        "test collection-with-min-and-max-count",
                        values,
                        services
                    );

                    Assert.False(executionResult.IsSuccess);

                    values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "values", new[] { "ra", "ra", "rasputin" } },
                    };

                    executionResult = await commandService.TryExecuteAsync
                    (
                        "test collection-with-min-and-max-count",
                        values,
                        services
                    );

                    Assert.False(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a max-constrained collection and a
                /// following positional argument.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteConstrainedCollectionWithPositionalValue()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandGroup<CollectionCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "values", new[] { "ra", "ra" } },
                        { "value", new[] { "rasputin" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test constrained-collection-with-positional-value",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);

                    values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "values", new[] { "ra", "ra" } }
                    };

                    executionResult = await commandService.TryExecuteAsync
                    (
                        "test constrained-collection-with-positional-value ra ra",
                        values,
                        services
                    );

                    Assert.False(executionResult.IsSuccess);
                }
            }

            /// <summary>
            /// Tests overloads.
            /// </summary>
            public class Overloads
            {
                /// <summary>
                /// Tests whether the command service can execute an overloaded command.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteOverloadedCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandGroup<OverloadCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test overload",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                    Assert.Equal("overload-1", ((RetrieveEntityResult<string>)executionResult.InnerResult!).Entity);

                    values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "booga" } }
                    };

                    executionResult = await commandService.TryExecuteAsync
                    (
                        "test overload",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                    Assert.Equal("overload-2", ((RetrieveEntityResult<string>)executionResult.InnerResult!).Entity);

                    values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value-2", new[] { "booga" } },
                        { "value1", new[] { "wooga" } }
                    };

                    executionResult = await commandService.TryExecuteAsync
                    (
                        "test overload",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                    Assert.Equal("overload-3", ((RetrieveEntityResult<string>)executionResult.InnerResult!).Entity);
                }
            }

            /// <summary>
            /// Tests conditional commands.
            /// </summary>
            public class Conditions
            {
                /// <summary>
                /// Tests whether the command service can execute a command with a method condition.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteCommandWithMethodCondition()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandGroup<ConditionalCommandGroup>()
                        .AddCondition<MethodCondition>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test method-condition",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }

                /// <summary>
                /// Tests whether the command service can execute a command with a parameter condition.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteCommandWithParameterCondition()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandGroup<ConditionalCommandGroup>()
                        .AddCondition<ParameterCondition>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "booga" } }
                    };

                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "test parameter-condition",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);

                    values = new Dictionary<string, IReadOnlyList<string>>
                    {
                        { "value", new[] { "wooga" } }
                    };

                    executionResult = await commandService.TryExecuteAsync
                    (
                        "test parameter-condition",
                        values,
                        services
                    );

                    Assert.False(executionResult.IsSuccess);
                }
            }

            /// <summary>
            /// Tests nonstandard return types.
            /// </summary>
            public class ReturnType
            {
                /// <summary>
                /// Tests whether a method that returns a ValueTask{IResult} can be executed.
                /// </summary>
                /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
                [Fact]
                public async Task CanExecuteValueTaskCommand()
                {
                    var services = new ServiceCollection()
                        .AddCommands()
                        .AddCommandGroup<ReturnTypeCommandGroup>()
                        .BuildServiceProvider();

                    var commandService = services.GetRequiredService<CommandService>();

                    var values = new Dictionary<string, IReadOnlyList<string>>();
                    var executionResult = await commandService.TryExecuteAsync
                    (
                        "a",
                        values,
                        services
                    );

                    Assert.True(executionResult.IsSuccess);
                }
            }
        }
    }
}
