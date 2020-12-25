//
//  TypeRepository.cs
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

namespace Remora.Commands.Services
{
    /// <summary>
    /// Acts as a container for various registered types.
    /// </summary>
    /// <typeparam name="TType">The base type of types that can be registered.</typeparam>
    public class TypeRepository<TType> where TType : class
    {
        private readonly List<Type> _registeredTypes = new();

        /// <summary>
        /// Registers a type in the repository.
        /// </summary>
        /// <typeparam name="TTypeToRegister">The type to register.</typeparam>
        public void RegisterType<TTypeToRegister>() where TTypeToRegister : TType
        {
            if (!_registeredTypes.Contains(typeof(TTypeToRegister)))
            {
                _registeredTypes.Add(typeof(TTypeToRegister));
            }
        }

        /// <summary>
        /// Registers a type in the repository.
        /// </summary>
        /// <param name="typeToRegister">The type to register.</param>
        public void RegisterType(Type typeToRegister)
        {
            if (!typeof(TType).IsAssignableFrom(typeToRegister))
            {
                throw new InvalidOperationException();
            }

            if (!_registeredTypes.Contains(typeToRegister))
            {
                _registeredTypes.Add(typeToRegister);
            }
        }

        /// <summary>
        /// Gets the types in the repository that are assignment compatible with the given type.
        /// </summary>
        /// <typeparam name="TAssignableType">The assignable type.</typeparam>
        /// <returns>The types.</returns>
        public IEnumerable<Type> GetTypes<TAssignableType>() where TAssignableType : TType
        {
            return GetTypes(typeof(TAssignableType));
        }

        /// <summary>
        /// Gets the types in the repository that are assignment compatible with the given type.
        /// </summary>
        /// <param name="assignableType">The assignable type.</param>
        /// <returns>The types.</returns>
        public IEnumerable<Type> GetTypes(Type assignableType)
        {
            if (!typeof(TType).IsAssignableFrom(assignableType))
            {
                throw new InvalidOperationException();
            }

            return _registeredTypes.Where(assignableType.IsAssignableFrom);
        }
    }
}
