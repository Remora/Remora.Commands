//
//  ServiceCollectionExtensions.cs
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
using System.Numerics;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Remora.Commands.Conditions;
using Remora.Commands.Groups;
using Remora.Commands.Parsers;
using Remora.Commands.Services;
using Remora.Commands.Trees;

namespace Remora.Commands.Extensions
{
    /// <summary>
    /// Defines extension methods for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a command module to the available services.
        /// </summary>
        /// <typeparam name="TCommandModule">The command module to register.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>The service collection, with the configured modules.</returns>
        public static IServiceCollection AddCommandGroup<TCommandModule>
        (
            this IServiceCollection serviceCollection
        )
            where TCommandModule : CommandGroup
        {
            serviceCollection.Configure<CommandTreeBuilder>
            (
                builder => builder.RegisterModule<TCommandModule>()
            );

            return serviceCollection;
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
            serviceCollection.TryAddSingleton
            (
                services =>
                {
                    var treeBuilder = services.GetRequiredService<IOptions<CommandTreeBuilder>>();
                    return treeBuilder.Value.Build();
                }
            );

            serviceCollection.TryAddScoped
            (
                services =>
                {
                    var tree = services.GetRequiredService<CommandTree>();
                    var conditionRepository = services.GetRequiredService<IOptions<TypeRepository<ICondition>>>();
                    var parserRepository = services.GetRequiredService<IOptions<TypeRepository<ITypeParser>>>();

                    return new CommandService(tree, conditionRepository, parserRepository);
                }
            );

            serviceCollection.AddOptions<TypeRepository<ICondition>>();
            serviceCollection.AddOptions<TypeRepository<ITypeParser>>();

            serviceCollection
                .AddParser<char, CharParser>()
                .AddParser<bool, BooleanParser>()
                .AddParser<byte, ByteParser>()
                .AddParser<sbyte, SByteParser>()
                .AddParser<ushort, UInt16Parser>()
                .AddParser<short, Int16Parser>()
                .AddParser<uint, UInt32Parser>()
                .AddParser<int, Int32Parser>()
                .AddParser<ulong, UInt64Parser>()
                .AddParser<long, Int64Parser>()
                .AddParser<float, SingleParser>()
                .AddParser<double, DoubleParser>()
                .AddParser<decimal, DecimalParser>()
                .AddParser<BigInteger, BigIntegerParser>()
                .AddParser<string, StringParser>()
                .AddParser<DateTimeOffset, DateTimeOffsetParser>()
                .TryAddSingleton(typeof(ITypeParser<>), typeof(EnumParser<>));

            return serviceCollection;
        }

        /// <summary>
        /// Adds a type parser as a singleton service.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <typeparam name="TType">The type to parse.</typeparam>
        /// <typeparam name="TParser">The type parser.</typeparam>
        /// <returns>The service collection, with the parser.</returns>
        public static IServiceCollection AddParser<TType, TParser>
        (
            this IServiceCollection serviceCollection
        )
            where TType : notnull
            where TParser : AbstractTypeParser<TType>
        {
            serviceCollection.Configure<TypeRepository<ITypeParser>>(tr => tr.RegisterType<TParser>());
            return serviceCollection;
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
        public static IServiceCollection AddCondition<TCondition>
        (
            this IServiceCollection serviceCollection
        ) where TCondition : class, ICondition
        {
            serviceCollection.Configure<TypeRepository<ICondition>>(tr => tr.RegisterType<TCondition>());
            return serviceCollection;
        }
    }
}
