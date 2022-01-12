//
//  CommandServiceTests.Preparsed.Conditions.cs
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
using Remora.Commands.Results;
using Remora.Commands.Services;
using Remora.Commands.Tests.Data.Conditions;
using Remora.Commands.Tests.Data.Modules;
using Remora.Commands.Trees.Nodes;
using Xunit;

namespace Remora.Commands.Tests.Services;

public static partial class CommandServiceTests
{
    public static partial class Preparsed
    {
        /// <summary>
        /// Tests conditional commands.
        /// </summary>
        public class Conditions
        {
            /// <summary>
            /// Tests whether the command service can execute a command with a group condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCommandInUnnamedGroupWithGroupCondition()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.UnnamedGroupWithSuccessfulCondition>()
                    .Finish()
                    .AddCondition<GroupCondition>()
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

            /// <summary>
            /// Tests whether the command service produces a correct error condition for a command with a group
            /// condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task UnsuccessfulCommandInUnnamedGroupWithGroupConditionProducesCorrectError()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.UnnamedGroupWithUnsuccessfulCondition>()
                    .Finish()
                    .AddCondition<GroupCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a",
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
                Assert.IsType<ConditionNotSatisfiedError>(executionResult.Error);
                Assert.Null(((ConditionNotSatisfiedError)executionResult.Error!).Node);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a group condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCommandInNamedGroupWithGroupCondition()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.NamedGroupWithSuccessfulCondition>()
                    .Finish()
                    .AddCondition<GroupCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a b",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service produces a correct error condition for a command with a group
            /// condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task UnsuccessfulCommandInNamedGroupWithGroupConditionProducesCorrectError()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.NamedGroupWithUnsuccessfulCondition>()
                    .Finish()
                    .AddCondition<GroupCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a b",
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
                Assert.IsType<ConditionNotSatisfiedError>(executionResult.Error);
                Assert.IsType<GroupNode>(((ConditionNotSatisfiedError)executionResult.Error!).Node);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a command condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCommandInUnnamedGroupWithCommandCondition()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.UnnamedGroupWithSuccessfulCommandCondition>()
                    .Finish()
                    .AddCondition<CommandCondition>()
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

            /// <summary>
            /// Tests whether the command service produces a correct error condition for a command with a group
            /// condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task UnsuccessfulCommandInUnnamedGroupWithCommandConditionProducesCorrectError()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.UnnamedGroupWithUnsuccessfulCommandCondition>()
                    .Finish()
                    .AddCondition<CommandCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a",
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
                Assert.IsType<ConditionNotSatisfiedError>(executionResult.Error);
                Assert.IsType<CommandNode>(((ConditionNotSatisfiedError)executionResult.Error!).Node);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a command condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCommandInNamedGroupWithCommandCondition()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.NamedGroupWithSuccessfulCommandCondition>()
                    .Finish()
                    .AddCondition<CommandCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a b",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service produces a correct error condition for a command with a group
            /// condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task UnsuccessfulCommandInNamedGroupWithCommandConditionProducesCorrectError()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.NamedGroupWithUnsuccessfulCommandCondition>()
                    .Finish()
                    .AddCondition<CommandCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a b",
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
                Assert.IsType<ConditionNotSatisfiedError>(executionResult.Error);
                Assert.IsType<CommandNode>(((ConditionNotSatisfiedError)executionResult.Error!).Node);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a parameter condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCommandInUnnamedGroupWithParameterCondition()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.UnnamedGroupWithSuccessfulParameterCondition>()
                    .Finish()
                    .AddCondition<ParameterCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "value", new[] { "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "a",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service produces a correct error condition for a command with a group
            /// condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task UnsuccessfulCommandInUnnamedGroupWithParameterConditionProducesCorrectError()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.UnnamedGroupWithUnsuccessfulParameterCondition>()
                    .Finish()
                    .AddCondition<ParameterCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "value", new[] { "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "a",
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
                Assert.IsType<ConditionNotSatisfiedError>(executionResult.Error);
                Assert.IsType<CommandNode>(((ConditionNotSatisfiedError)executionResult.Error!).Node);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a parameter condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCommandInNamedGroupWithParameterCondition()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.NamedGroupWithSuccessfulParameterCondition>()
                    .Finish()
                    .AddCondition<ParameterCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "value", new[] { "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "a b",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service produces a correct error condition for a command with a group
            /// condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task UnsuccessfulCommandInNamedGroupWithParameterConditionProducesCorrectError()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.NamedGroupWithUnsuccessfulParameterCondition>()
                    .Finish()
                    .AddCondition<ParameterCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "value", new[] { "booga" } }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    "a b",
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
                Assert.IsType<ConditionNotSatisfiedError>(executionResult.Error);
                Assert.IsType<CommandNode>(((ConditionNotSatisfiedError)executionResult.Error!).Node);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a group condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCommandInNestedUnnamedGroupInsideNamedGroupWithCondition()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.NamedGroupWithSuccessfulConditionAndInnerUnnamedGroup>()
                    .Finish()
                    .AddCondition<GroupCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a b",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a group condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCommandInNestedNamedGroupInsideNamedGroupWithCondition()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.NamedGroupWithSuccessfulConditionAndInnerNamedGroup>()
                    .Finish()
                    .AddCondition<GroupCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a b c",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a group condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCommandInNestedUnnamedGroupInsideUnnamedGroupWithCondition()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.UnnamedGroupWithSuccessfulConditionAndInnerUnnamedGroup>()
                    .Finish()
                    .AddCondition<GroupCondition>()
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

            /// <summary>
            /// Tests whether the command service can execute a command with a group condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCommandInNestedNamedGroupInsideUnnamedGroupWithCondition()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.UnnamedGroupWithSuccessfulConditionAndInnerNamedGroup>()
                    .Finish()
                    .AddCondition<GroupCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a b",
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service produces a correct error condition for a command with a group
            /// condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task UnsuccessfulCommandInNestedUnnamedGroupInsideNamedGroupWithConditionProducesCorrectError()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.NamedGroupWithUnsuccessfulConditionAndInnerUnnamedGroup>()
                    .Finish()
                    .AddCondition<GroupCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a b",
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
                Assert.IsType<ConditionNotSatisfiedError>(executionResult.Error);
                Assert.IsType<GroupNode>(((ConditionNotSatisfiedError)executionResult.Error!).Node);
            }

            /// <summary>
            /// Tests whether the command service produces a correct error condition for a command with a group
            /// condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task UnsuccessfulCommandInNestedNamedGroupInsideNamedGroupWithConditionProducesCorrectError()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.NamedGroupWithUnsuccessfulConditionAndInnerNamedGroup>()
                    .Finish()
                    .AddCondition<GroupCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a b c",
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
                Assert.IsType<ConditionNotSatisfiedError>(executionResult.Error);
                Assert.IsType<GroupNode>(((ConditionNotSatisfiedError)executionResult.Error!).Node);
            }

            /// <summary>
            /// Tests whether the command service produces a correct error condition for a command with a group
            /// condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task UnsuccessfulCommandInNestedUnnamedGroupInsideUnnamedGroupWithConditionProducesCorrectError()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.UnnamedGroupWithUnsuccessfulConditionAndInnerUnnamedGroup>()
                    .Finish()
                    .AddCondition<GroupCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a",
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
                Assert.IsType<ConditionNotSatisfiedError>(executionResult.Error);
                Assert.Null(((ConditionNotSatisfiedError)executionResult.Error!).Node);
            }

            /// <summary>
            /// Tests whether the command service produces a correct error condition for a command with a group
            /// condition.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task UnsuccessfulCommandInNestedNamedGroupInsideUnnamedGroupWithConditionProducesCorrectError()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ConditionalGroups.UnnamedGroupWithUnsuccessfulConditionAndInnerNamedGroup>()
                    .Finish()
                    .AddCondition<GroupCondition>()
                    .BuildServiceProvider();

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    "a b",
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
                Assert.IsType<ConditionNotSatisfiedError>(executionResult.Error);
                Assert.Null(((ConditionNotSatisfiedError)executionResult.Error!).Node);
            }
        }
    }
}
