//
//  CommandBuilderTests.cs
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
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Builders;
using Remora.Commands.Extensions;
using Remora.Commands.Tests.Data.DummyModules;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Xunit;

namespace Remora.Commands.Tests.Builders;

/// <summary>
/// Tests the <see cref="CommandBuilder"/> class.
/// </summary>
public class CommandBuilderTests
{
    /// <summary>
    /// Asserts that the builder can successfully build a command.
    /// </summary>
    [Fact]
    public void CanBuildCommand()
    {
        var rootNode = new RootNode(Array.Empty<IChildNode>());
        var command = new CommandBuilder()
            .WithName("test")
            .WithDescription("A test command.")
            .WithInvocation((_, _, _) => default)
            .Build(rootNode);

        Assert.Equal("test", command.Key);
        Assert.Equal("A test command.", command.Shape.Description);
        Assert.Equal(rootNode, command.Parent);
    }

    /// <summary>
    /// Tests that the builder can successfully build a command with parameters.
    /// </summary>
    [Fact]
    public void CanBuildParameterizedCommand()
    {
        var rootNode = new RootNode(Array.Empty<IChildNode>());
        var commandBuilder = new CommandBuilder()
            .WithName("test")
            .WithInvocation((_, _, _) => default)
            .WithDescription("A test command.");

        var parameterBuilder = new CommandParameterBuilder(commandBuilder, null)
            .WithName("test")
            .WithDescription("A test parameter.")
            .WithType(typeof(string));

        var command = commandBuilder.Build(rootNode);

        Assert.Equal("test", command.Key);
        Assert.Equal("A test command.", command.Shape.Description);
        Assert.Equal(rootNode, command.Parent);
        Assert.Single(command.Shape.Parameters);
        Assert.Equal("test", command.Shape.Parameters[0].HintName);
    }

    /// <summary>
    /// Asserts that the builder can successfully build a command with multiple parameters.
    /// </summary>
    [Fact]
    public void CanBuildParameterizedCommandWithMultipleParameters()
    {
        var rootNode = new RootNode(Array.Empty<IChildNode>());
        var commandBuilder = new CommandBuilder()
            .WithName("test")
            .WithInvocation((_, _, _) => default)
            .WithDescription("A test command.");

        var parameterBuilder = new CommandParameterBuilder(commandBuilder, null)
            .WithName("test")
            .WithDescription("A test parameter.")
            .WithType(typeof(string));

        var parameterBuilder2 = new CommandParameterBuilder(commandBuilder, null)
            .WithName("test2")
            .WithDescription("A second test parameter.")
            .WithType(typeof(int));

        var command = commandBuilder.Build(rootNode);

        Assert.Equal("test", command.Key);
        Assert.Equal("A test command.", command.Shape.Description);
        Assert.Equal(rootNode, command.Parent);
        Assert.Equal(2, command.Shape.Parameters.Count);
        Assert.Equal("test", command.Shape.Parameters[0].HintName);
        Assert.Equal("test2", command.Shape.Parameters[1].HintName);
    }

    /// <summary>
    /// Asserts that the builder can correctly create a command from a given <see cref="MethodInfo"/>.
    /// </summary>
    [Fact]
    public void CanCreateCommandFromMethod()
    {
        var rootNode = new RootNode(Array.Empty<IChildNode>());
        var builder = CommandBuilder.FromMethod(null, typeof(DescribedGroup).GetMethod(nameof(DescribedGroup.B), BindingFlags.Public | BindingFlags.Instance)!);

        var command = builder.Build(rootNode);

        Assert.Equal("b", command.Key);
        Assert.Equal("Command description", command.Shape.Description);
    }

    /// <summary>
    /// Asserts that the invocation on a built command is functional and preserved.
    /// </summary>
    [Fact]
    public void CanInvokeCommand()
    {
        var services = new ServiceCollection();
        services.AddCommands()
                .AddCommandTree()
                .CreateCommand()
                .WithName("a")
                .WithInvocation
                 (
                     (_, p, _) =>
                     {
                         p[0] = (object)true;
                         return default;
                     }
                 );

        var provider = services.BuildServiceProvider();
        var command = ((CommandNode)provider.GetRequiredService<CommandTree>().Root.Children[0]).Invoke;

        var parameters = new object[1];
        command.Invoke(provider, parameters, default);

        Assert.Equal(parameters[0], true);
    }
}
