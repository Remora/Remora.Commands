//
//  NullableStructParser.cs
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Extensions;
using Remora.Commands.Services;
using Remora.Results;

namespace Remora.Commands.Parsers
{
    /// <summary>
    /// Parses nullable structs.
    /// </summary>
    public class NullableStructParser : AbstractTypeParser
    {
        private readonly TypeParserService _typeParserService;
        private readonly IServiceProvider _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableStructParser"/> class.
        /// </summary>
        /// <param name="typeParserService">The type parser service.</param>
        /// <param name="services">The available services.</param>
        public NullableStructParser(TypeParserService typeParserService, IServiceProvider services)
        {
            _typeParserService = typeParserService;
            _services = services;
        }

        /// <inheritdoc />
        public override bool CanParse(Type type)
        {
            return type.IsNullableStruct();
        }

        /// <inheritdoc/>
        public override async ValueTask<Result<object?>> TryParseAsync
        (
            string? token,
            Type type,
            CancellationToken ct = default
        )
        {
            if (token is null or "null")
            {
                return Result<object?>.FromSuccess(null);
            }

            var concreteType = type.GetGenericArguments().Single();

            var tryParse = await _typeParserService.TryParseAsync(_services, token, concreteType, ct);
            return tryParse.IsSuccess
                ? tryParse.Entity
                : Result<object?>.FromError(tryParse);
        }
    }
}
