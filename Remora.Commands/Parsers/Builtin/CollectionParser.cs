//
//  CollectionParser.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Extensions;
using Remora.Commands.Services;
using Remora.Results;

namespace Remora.Commands.Parsers;

/// <summary>
/// Parses supported collections.
/// </summary>
public class CollectionParser : AbstractTypeParser
{
    private readonly TypeParserService _typeParserService;
    private readonly IServiceProvider _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionParser"/> class.
    /// </summary>
    /// <param name="typeParserService">The type parser service.</param>
    /// <param name="services">The available services.</param>
    public CollectionParser(TypeParserService typeParserService, IServiceProvider services)
    {
        _typeParserService = typeParserService;
        _services = services;
    }

    /// <inheritdoc/>
    public override bool CanParse(Type type)
    {
        return type.IsSupportedCollection();
    }

    /// <inheritdoc/>
    public override async ValueTask<Result<object?>> TryParseAsync
    (
        IReadOnlyList<string> tokens,
        Type type,
        CancellationToken ct = default
    )
    {
        var elementType = type.GetCollectionElementType();
        var concreteCollectionType = typeof(List<>).MakeGenericType(elementType);

        IList collection;
        var errors = new List<Result<object?>>();

        if (type.IsArray)
        {
            collection = Array.CreateInstance(elementType, tokens.Count);
            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                var tryParse = await _typeParserService.TryParseAsync(_services, token, elementType, ct);
                if (tryParse.IsSuccess)
                {
                    collection[i] = tryParse.Entity;
                }
                else
                {
                    errors.Add(tryParse);
                }
            }
        }
        else
        {
            collection = (IList)Activator.CreateInstance(concreteCollectionType)!;
            foreach (var token in tokens)
            {
                var tryParse = await _typeParserService.TryParseAsync(_services, token, elementType, ct);
                if (tryParse.IsSuccess)
                {
                    collection.Add(tryParse.Entity);
                }
                else
                {
                    errors.Add(tryParse);
                }
            }
        }

        return errors.Count switch
        {
            0 => Result<object?>.FromSuccess(collection),
            1 => errors[0],
            _ => new AggregateError(errors.Cast<IResult>().ToArray())
        };
    }
}
