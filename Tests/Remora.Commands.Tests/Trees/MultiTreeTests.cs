//
//  MultiTreeTests.cs
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

using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Commands.Services;
using Remora.Commands.Tests.Data.DummyModules;
using Remora.Commands.Trees.Nodes;
using Xunit;

namespace Remora.Commands.Tests.Trees;

/// <summary>
/// Tests various functionality related to multi-tree usage.
/// </summary>
public class MultiTreeTests
{
    /// <summary>
    /// Tests whether the default tree is available for injection and consumption, even if no command groups have been
    /// registered anywhere.
    /// </summary>
    [Fact]
    public void DefaultTreeIsAvailableWithoutAnyRegisteredGroups()
    {
        var services = new ServiceCollection()
            .AddCommands()
            .BuildServiceProvider(true);

        var accessor = services.GetRequiredService<CommandTreeAccessor>();
        Assert.True(accessor.TryGetNamedTree(null, out var tree));
        Assert.NotNull(tree);
    }

    /// <summary>
    /// Tests whether the default tree is empty when no groups have been registered.
    /// </summary>
    [Fact]
    public void DefaultTreeIsEmptyByDefault()
    {
        var services = new ServiceCollection()
            .AddCommands()
            .BuildServiceProvider(true);

        var accessor = services.GetRequiredService<CommandTreeAccessor>();
        accessor.TryGetNamedTree(null, out var tree);

        Assert.Empty(tree!.Root.Children);
    }

    /// <summary>
    /// Tests whether the "all" tree is available for injection and consumption, even if no command groups have been
    /// registered anywhere.
    /// </summary>
    [Fact]
    public void AllTreeIsAvailableWithoutAnyRegisteredGroups()
    {
        var services = new ServiceCollection()
            .AddCommands()
            .BuildServiceProvider(true);

        var accessor = services.GetRequiredService<CommandTreeAccessor>();
        Assert.True(accessor.TryGetNamedTree(Constants.AllTreeName, out var tree));
        Assert.NotNull(tree);
    }

    /// <summary>
    /// Tests whether the "all" tree is empty when no groups have been registered.
    /// </summary>
    [Fact]
    public void AllTreeIsEmptyByDefault()
    {
        var services = new ServiceCollection()
            .AddCommands()
            .BuildServiceProvider(true);

        var accessor = services.GetRequiredService<CommandTreeAccessor>();
        accessor.TryGetNamedTree(Constants.AllTreeName, out var tree);

        Assert.Empty(tree!.Root.Children);
    }

    /// <summary>
    /// Tests whether the default tree is available for injection and consumption if a command group has been explicitly
    /// added to it.
    /// </summary>
    [Fact]
    public void DefaultTreeIsAvailableWithRegisteredGroups()
    {
        var services = new ServiceCollection()
            .AddCommands()
            .AddCommandTree()
            .WithCommandGroup<NamedGroupWithCommands>()
            .Finish()
            .BuildServiceProvider(true);

        var accessor = services.GetRequiredService<CommandTreeAccessor>();
        Assert.True(accessor.TryGetNamedTree(null, out var tree));
        Assert.NotNull(tree);
    }

    /// <summary>
    /// Tests whether a command group is correctly added to the default tree when explicitly registered with it.
    /// </summary>
    [Fact]
    public void ExplicitlyRegisteredGroupIsCorrectlyAddedToDefaultTree()
    {
        var services = new ServiceCollection()
            .AddCommands()
            .AddCommandTree()
                .WithCommandGroup<NamedGroupWithCommands>()
                .Finish()
            .BuildServiceProvider(true);

        var accessor = services.GetRequiredService<CommandTreeAccessor>();
        accessor.TryGetNamedTree(null, out var tree);

        var root = tree!.Root;
        Assert.Single(root.Children);
        Assert.IsType<GroupNode>(root.Children[0]);

        var groupNode = (GroupNode)root.Children[0];

        Assert.Equal("a", groupNode.Key);

        var groupType = Assert.Single(groupNode.GroupTypes);
        Assert.Equal(typeof(NamedGroupWithCommands), groupType);

        Assert.Equal(3, groupNode.Children.Count);
        Assert.IsType<CommandNode>(groupNode.Children[0]);
        Assert.IsType<CommandNode>(groupNode.Children[1]);
        Assert.IsType<CommandNode>(groupNode.Children[2]);

        var command1 = (CommandNode)groupNode.Children[0];
        var command2 = (CommandNode)groupNode.Children[1];
        var command3 = (CommandNode)groupNode.Children[2];

        Assert.Equal("b", command1.Key);
        Assert.Equal("c", command2.Key);
        Assert.Equal("d", command3.Key);
    }

    /// <summary>
    /// Tests whether adding a command group to the service collection directly adds it to the default, unnamed tree.
    /// </summary>
    [Fact]
    public void AddingCommandGroupToServiceCollectionAddsItToDefaultTree()
    {
        var services = new ServiceCollection()
            .AddCommands()
            #pragma warning disable CS0618
            .AddCommandGroup<NamedGroupWithCommands>()
            #pragma warning restore CS0618
            .BuildServiceProvider(true);

        var accessor = services.GetRequiredService<CommandTreeAccessor>();
        accessor.TryGetNamedTree(null, out var tree);

        var root = tree!.Root;
        Assert.Single(root.Children);
        Assert.IsType<GroupNode>(root.Children[0]);

        var groupNode = (GroupNode)root.Children[0];

        Assert.Equal("a", groupNode.Key);

        var groupType = Assert.Single(groupNode.GroupTypes);
        Assert.Equal(typeof(NamedGroupWithCommands), groupType);

        Assert.Equal(3, groupNode.Children.Count);
        Assert.IsType<CommandNode>(groupNode.Children[0]);
        Assert.IsType<CommandNode>(groupNode.Children[1]);
        Assert.IsType<CommandNode>(groupNode.Children[2]);

        var command1 = (CommandNode)groupNode.Children[0];
        var command2 = (CommandNode)groupNode.Children[1];
        var command3 = (CommandNode)groupNode.Children[2];

        Assert.Equal("b", command1.Key);
        Assert.Equal("c", command2.Key);
        Assert.Equal("d", command3.Key);
    }

    /// <summary>
    /// Tests whether groups are correctly added to their respective trees when registering.
    /// </summary>
    [Fact]
    public void CanAddMultipleTrees()
    {
        var services = new ServiceCollection()
            .AddCommands()
            .AddCommandTree("named")
                .WithCommandGroup<NamedGroupWithCommands>()
                .Finish()
            .AddCommandTree("unnamed")
                .WithCommandGroup<UnnamedGroupWithCommands>()
                .Finish()
            .BuildServiceProvider(true);

        var accessor = services.GetRequiredService<CommandTreeAccessor>();

        Assert.True(accessor.TryGetNamedTree("named", out var namedTree));
        Assert.NotNull(namedTree);

        Assert.True(accessor.TryGetNamedTree("unnamed", out var unnamedTree));
        Assert.NotNull(unnamedTree);
    }

    /// <summary>
    /// Tests whether groups are correctly added to their respective trees when registering.
    /// </summary>
    [Fact]
    public void GroupsAreAddedToCorrectTrees()
    {
        var services = new ServiceCollection()
            .AddCommands()
            .AddCommandTree("named")
                .WithCommandGroup<NamedGroupWithCommands>()
                .Finish()
            .AddCommandTree("unnamed")
                .WithCommandGroup<UnnamedGroupWithCommands>()
                .Finish()
            .BuildServiceProvider(true);

        var accessor = services.GetRequiredService<CommandTreeAccessor>();

        accessor.TryGetNamedTree("named", out var namedTree);
        {
            var namedRoot = namedTree!.Root;
            Assert.Single(namedRoot.Children);
            Assert.IsType<GroupNode>(namedRoot.Children[0]);

            var groupNode = (GroupNode)namedRoot.Children[0];

            Assert.Equal("a", groupNode.Key);

            var groupType = Assert.Single(groupNode.GroupTypes);
            Assert.Equal(typeof(NamedGroupWithCommands), groupType);

            Assert.Equal(3, groupNode.Children.Count);
            Assert.IsType<CommandNode>(groupNode.Children[0]);
            Assert.IsType<CommandNode>(groupNode.Children[1]);
            Assert.IsType<CommandNode>(groupNode.Children[2]);

            var command1 = (CommandNode)groupNode.Children[0];
            var command2 = (CommandNode)groupNode.Children[1];
            var command3 = (CommandNode)groupNode.Children[2];

            Assert.Equal("b", command1.Key);
            Assert.Equal("c", command2.Key);
            Assert.Equal("d", command3.Key);
        }

        accessor.TryGetNamedTree("unnamed", out var unnamedTree);
        {
            var unnamedRoot = unnamedTree!.Root;
            Assert.Equal(4, unnamedRoot.Children.Count);
            Assert.IsType<CommandNode>(unnamedRoot.Children[0]);
            Assert.IsType<CommandNode>(unnamedRoot.Children[1]);
            Assert.IsType<CommandNode>(unnamedRoot.Children[2]);
            Assert.IsType<CommandNode>(unnamedRoot.Children[3]);

            var command1 = (CommandNode)unnamedRoot.Children[0];
            var command2 = (CommandNode)unnamedRoot.Children[1];
            var command3 = (CommandNode)unnamedRoot.Children[2];
            var command4 = (CommandNode)unnamedRoot.Children[3];

            Assert.Equal("a", command1.Key);
            Assert.Equal("b", command2.Key);
            Assert.Equal("c", command3.Key);
            Assert.Equal("d", command4.Key);
        }
    }

    /// <summary>
    /// Tests whether all registered groups, regardless of tree, are added to the "all" tree.
    /// </summary>
    [Fact]
    public void AllGroupsAreAddedToAllTree()
    {
        var services = new ServiceCollection()
            .AddCommands()
            .AddCommandTree("named")
                .WithCommandGroup<NamedGroupWithCommands>()
                .Finish()
            .AddCommandTree("unnamed")
                .WithCommandGroup<UnnamedGroupWithCommands>()
                .Finish()
            .BuildServiceProvider(true);

        var accessor = services.GetRequiredService<CommandTreeAccessor>();

        accessor.TryGetNamedTree(Constants.AllTreeName, out var allTree);
        {
            var root = allTree!.Root;
            Assert.Equal(5, root.Children.Count);
            Assert.IsType<GroupNode>(root.Children[0]);

            var groupNode = (GroupNode)root.Children[0];

            Assert.Equal("a", groupNode.Key);

            var groupType = Assert.Single(groupNode.GroupTypes);
            Assert.Equal(typeof(NamedGroupWithCommands), groupType);

            Assert.Equal(3, groupNode.Children.Count);
            Assert.IsType<CommandNode>(groupNode.Children[0]);
            Assert.IsType<CommandNode>(groupNode.Children[1]);
            Assert.IsType<CommandNode>(groupNode.Children[2]);

            var command1 = (CommandNode)groupNode.Children[0];
            var command2 = (CommandNode)groupNode.Children[1];
            var command3 = (CommandNode)groupNode.Children[2];

            Assert.Equal("b", command1.Key);
            Assert.Equal("c", command2.Key);
            Assert.Equal("d", command3.Key);

            Assert.IsType<CommandNode>(root.Children[1]);
            Assert.IsType<CommandNode>(root.Children[2]);
            Assert.IsType<CommandNode>(root.Children[3]);
            Assert.IsType<CommandNode>(root.Children[4]);

            var command4 = (CommandNode)root.Children[1];
            var command5 = (CommandNode)root.Children[2];
            var command6 = (CommandNode)root.Children[3];
            var command7 = (CommandNode)root.Children[4];

            Assert.Equal("a", command4.Key);
            Assert.Equal("b", command5.Key);
            Assert.Equal("c", command6.Key);
            Assert.Equal("d", command7.Key);
        }
    }
}
