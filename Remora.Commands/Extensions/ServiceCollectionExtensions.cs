//
//  ServiceCollectionExtensions.cs
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Remora.Commands.Conditions;
using Remora.Commands.DependencyInjection;
using Remora.Commands.Groups;
using Remora.Commands.Parsers;
using Remora.Commands.Services;

namespace Remora.Commands.Extensions;

/// <summary>
/// Defines extension methods for the <see cref="IServiceCollection"/> interface.
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a named command tree to the service collection.
    /// </summary>
    /// <remarks>
    /// This method may be called multiple times using the same name; the effects will be cumulative.
    /// </remarks>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="treeName">The name of the tree to configure.</param>
    /// <returns>A builder type that can be used to configure the tree.</returns>
    public static TreeRegistrationBuilder AddCommandTree
    (
        this IServiceCollection serviceCollection,
        string? treeName = null
    )
    {
        serviceCollection.Configure<CommandTreeAccessorOptions>(o => o.AddTreeName(treeName));
        return new TreeRegistrationBuilder(treeName, serviceCollection);
    }

    /// <summary>
    /// Adds a command module to the available services.
    /// </summary>
    /// <typeparam name="TCommandModule">The command module to register.</typeparam>
    /// <param name="serviceCollection">The service collection.</param>
    /// <returns>The service collection, with the configured modules.</returns>
    [Obsolete("Call AddCommandTree with no arguments instead, and add the group there.")]
    public static IServiceCollection AddCommandGroup
    <
        [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        TCommandModule
    >
    (
        this IServiceCollection serviceCollection
    ) where TCommandModule : CommandGroup
        => serviceCollection.AddCommandGroup(typeof(TCommandModule));

    /// <summary>
    /// Adds a command module to the available services.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="commandModule">The command module to register.</param>
    /// <returns>The service collection with the configured modules.</returns>
    [Obsolete("Call AddCommandTree with no arguments instead, and add the group there.")]
    public static IServiceCollection AddCommandGroup
    (
        this IServiceCollection serviceCollection,
        Type commandModule
    )
    {
        return serviceCollection
            .AddCommandTree()
            .WithCommandGroup(commandModule)
            .Finish();
    }

    /// <summary>
    /// Adds the services needed by the command subsystem.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <returns>The service collection, with the required command services.</returns>
    public static IServiceCollection AddCommands
    (
        this IServiceCollection serviceCollection
    )
    {
        // This empty configuration is done to ensure the options are available even
        // when no trees have been added
        serviceCollection.Configure<CommandTreeAccessorOptions>(_ => { });

        serviceCollection.TryAddSingleton<CommandTreeAccessor>();
        serviceCollection.TryAddScoped<CommandService>();

        // Default tree access (injecting CommandTree directly)
        serviceCollection.TryAddSingleton
        (
            services =>
            {
                var treeAccessor = services.GetRequiredService<CommandTreeAccessor>();
                if (!treeAccessor.TryGetNamedTree(null, out var tree))
                {
                    throw new InvalidOperationException("No default tree available. Bug?");
                }

                return tree;
            }
        );

        serviceCollection.TryAddSingleton<TypeParserService>();
        serviceCollection
            .AddParser<CharParser>()
            .AddParser<BooleanParser>()
            .AddParser<ByteParser>()
            .AddParser<SByteParser>()
            .AddParser<UInt16Parser>()
            .AddParser<Int16Parser>()
            .AddParser<UInt32Parser>()
            .AddParser<Int32Parser>()
            .AddParser<UInt64Parser>()
            .AddParser<Int64Parser>()
            .AddParser<SingleParser>()
            .AddParser<DoubleParser>()
            .AddParser<DecimalParser>()
            .AddParser<BigIntegerParser>()
            .AddParser<StringParser>()
            .AddParser<DateTimeOffsetParser>()
            .AddParser<DateTimeParser>()
            .AddParser<TimeSpanParser>()
            .AddParser<CollectionParser>()
            .AddParser<NullableStructParser>()
            .AddParser(typeof(EnumParser<>))
            .TryAddSingleton(typeof(ITypeParser<>), typeof(EnumParser<>));

        return serviceCollection;
    }

    /// <summary>
    /// Adds a type parser.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <typeparam name="TParser">The type parser.</typeparam>
    /// <returns>The service collection, with the parser.</returns>
    public static IServiceCollection AddParser
    <
        [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        TParser
    >
    (
        this IServiceCollection services
    )
        where TParser : ITypeParser
    {
        services.AddParser(typeof(TParser));
        return services;
    }

    /// <summary>
    /// Adds a type parser.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="parserType">The type parser.</param>
    /// <returns>The service collection, with the parser.</returns>
    public static IServiceCollection AddParser(this IServiceCollection services, Type parserType)
    {
        if (!parserType.IsTypeParser())
        {
            throw new InvalidOperationException($"The parser type must implement {nameof(ITypeParser)}.");
        }

        switch (parserType.IsGenericTypeDefinition)
        {
            case true when parserType.GetGenericArguments().Length != 1:
            {
                throw new InvalidOperationException("An open parser type may accept one and only one generic type.");
            }
            case true:
            {
                // This is an open parser type
                services.AddTransient(typeof(ITypeParser<>), parserType);
                break;
            }
            default:
            {
                var interfaces = parserType.GetInterfaces();
                var concreteTypeParserInterfaces = interfaces.Where
                (
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeParser<>)
                )
                .ToList();

                if (concreteTypeParserInterfaces.Count > 0)
                {
                    // This type implements one or more direct parsing interfaces
                    foreach (var concreteTypeParserInterface in concreteTypeParserInterfaces)
                    {
                        services.AddTransient(concreteTypeParserInterface, parserType);
                    }
                }
                else
                {
                    // This type is an indirect parser
                    services.AddTransient(typeof(ITypeParser), parserType);
                }

                break;
            }
        }

        return services;
    }

    /// <summary>
    /// Adds a condition to the service container.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <typeparam name="TCondition">The condition type.</typeparam>
    /// <returns>The collection, with the condition.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the type <typeparamref name="TCondition"/> does not implement any versions of either
    /// <see cref="ICondition{TAttribute}"/> or <see cref="ICondition{TAttribute,TData}"/>.
    /// </exception>
    public static IServiceCollection AddCondition
    <
        [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        TCondition
    >
    (
        this IServiceCollection serviceCollection
    ) where TCondition : class, ICondition
    {
        serviceCollection.TryAddScoped<TCondition>();
        foreach (var implementedInterface in typeof(TCondition).GetInterfaces())
        {
            if (!implementedInterface.IsGenericType)
            {
                continue;
            }

            var genericType = implementedInterface.GetGenericTypeDefinition();
            if (genericType == typeof(ICondition<>) || genericType == typeof(ICondition<,>))
            {
                serviceCollection.TryAddScoped(implementedInterface, typeof(TCondition));
            }
        }

        return serviceCollection;
    }
}
