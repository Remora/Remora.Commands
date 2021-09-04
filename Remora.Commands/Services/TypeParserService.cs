//
//  TypeParserService.cs
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Results;

namespace Remora.Commands.Services
{
    /// <summary>
    /// Exposes functionality for parsing various types, based on registered type parsers.
    /// </summary>
    [PublicAPI]
    public class TypeParserService
    {
        private readonly TypeRepository<ITypeParser> _parserRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeParserService"/> class.
        /// </summary>
        /// <param name="parserRepository">The parser repository.</param>
        public TypeParserService(IOptions<TypeRepository<ITypeParser>> parserRepository)
        {
            _parserRepository = parserRepository.Value;
        }

        /// <summary>
        /// Attempts to parse the given string into an instance of <typeparamref name="TType"/>.
        /// </summary>
        /// <param name="services">The available services.</param>
        /// <typeparam name="TType">The type to parse.</typeparam>
        /// <param name="token">The values.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        public async ValueTask<Result<TType>> TryParseAsync<TType>
        (
            IServiceProvider services,
            string token,
            CancellationToken ct = default
        )
        {
            var tryParse = await TryParseAsync(services, token, typeof(TType), ct);
            if (!tryParse.IsSuccess)
            {
                return Result<TType>.FromError(tryParse);
            }

            return (TType?)tryParse.Entity;
        }

        /// <summary>
        /// Attempts to parse the given string into a CLR object.
        /// </summary>
        /// <param name="services">The available services.</param>
        /// <param name="token">The values.</param>
        /// <param name="type">The type to parse the values as.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        public async ValueTask<Result<object?>> TryParseAsync
        (
            IServiceProvider services,
            string token,
            Type type,
            CancellationToken ct = default
        )
        {
            var compatibleParsers = GetCompatibleParsers(services, type);

            var errors = new List<IResultError>();
            foreach (var compatibleParser in compatibleParsers)
            {
                var tryParse = await compatibleParser.TryParseAsync(token, type, ct);
                if (tryParse.IsSuccess)
                {
                    return tryParse;
                }

                errors.Add(tryParse.Error);
            }

            return errors.Count switch
            {
                0 => new MissingParserError(type),
                1 => Result<object?>.FromError(errors[0]),
                _ => new AggregateError(errors)
            };
        }

        /// <summary>
        /// Attempts to parse the given string into an instance of <typeparamref name="TType"/>.
        /// </summary>
        /// <param name="services">The available services.</param>
        /// <typeparam name="TType">The type to parse.</typeparam>
        /// <param name="tokens">The tokens.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        public async ValueTask<Result<TType>> TryParseAsync<TType>
        (
            IServiceProvider services,
            IReadOnlyList<string> tokens,
            CancellationToken ct = default
        )
        {
            var tryParse = await TryParseAsync(services, tokens, typeof(TType), ct);
            if (!tryParse.IsSuccess)
            {
                return Result<TType>.FromError(tryParse);
            }

            return (TType?)tryParse.Entity;
        }

        /// <summary>
        /// Attempts to parse the given string into a CLR object.
        /// </summary>
        /// <param name="services">The available services.</param>
        /// <param name="tokens">The values.</param>
        /// <param name="type">The type to parse the values as.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        public async ValueTask<Result<object?>> TryParseAsync
        (
            IServiceProvider services,
            IReadOnlyList<string> tokens,
            Type type,
            CancellationToken ct = default
        )
        {
            var compatibleParsers = GetCompatibleParsers(services, type);

            var errors = new List<IResultError>();
            foreach (var compatibleParser in compatibleParsers)
            {
                var tryParse = await compatibleParser.TryParseAsync(tokens, type, ct);
                if (tryParse.IsSuccess)
                {
                    return tryParse;
                }

                errors.Add(tryParse.Error);
            }

            return errors.Count switch
            {
                0 => new MissingParserError(type),
                1 => Result<object?>.FromError(errors[0]),
                _ => new AggregateError(errors)
            };
        }

        /// <summary>
        /// Gets a set of instantiated parsers compatible with the given type.
        /// </summary>
        /// <param name="services">The available services.</param>
        /// <param name="type">The type to parse.</param>
        /// <returns>The parsers.</returns>
        internal IEnumerable<ITypeParser> GetCompatibleParsers(IServiceProvider services, Type type)
        {
            var parserType = typeof(ITypeParser<>).MakeGenericType(type);
            var directParsers = _parserRepository.GetTypes(parserType).ToList();
            foreach (var directParser in directParsers)
            {
                var instance = (ITypeParser)ActivatorUtilities.CreateInstance(services, directParser);
                yield return instance;
            }

            var indirectParsers = _parserRepository.GetTypes<ITypeParser>().Except(directParsers);
            foreach (var indirectParser in indirectParsers)
            {
                if (indirectParser.IsGenericTypeDefinition)
                {
                    var genericArgument = indirectParser.GetGenericArguments().Single();
                    var constraints = genericArgument.GetGenericParameterConstraints();
                    if (constraints.Any(c => !c.IsAssignableFrom(type)))
                    {
                        // Constraint violation
                        continue;
                    }

                    var concreteParser = indirectParser.MakeGenericType(type);

                    var parser = (ITypeParser)ActivatorUtilities.CreateInstance(services, concreteParser);
                    if (parser.CanParse(type))
                    {
                        yield return parser;
                    }
                }
                else
                {
                    var parser = (ITypeParser)ActivatorUtilities.CreateInstance(services, indirectParser);
                    if (parser.CanParse(type))
                    {
                        yield return parser;
                    }
                }
            }
        }
    }
}
