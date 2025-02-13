//
//  CommandParameterBuilder.cs
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Conditions;
using Remora.Commands.Extensions;
using Remora.Commands.Signatures;

namespace Remora.Commands.Builders;

/// <summary>
/// A builder class for command parameters.
/// </summary>
public class CommandParameterBuilder
{
    private readonly CommandBuilder _builder;
    private readonly List<Attribute> _attributes;
    private readonly List<ConditionAttribute> _conditions;

    private string _name;
    private bool _isGreedy;
    private bool _isOptional;
    private string? _description;
    private object? _defaultValue;
    private Type _parameterType;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandParameterBuilder"/> class.
    /// </summary>
    /// <param name="builder">The command this parameter belongs to.</param>
    /// <param name="type">The optional type of the parameter.</param>
    /// <remarks>If <paramref name="type"/> is null, <see cref="WithType"/> MUST be called before <see cref="Finish"/>.</remarks>
    public CommandParameterBuilder(CommandBuilder builder, Type type)
    {
        _name = string.Empty;
        _builder = builder;
        _attributes = new();
        _conditions = new();
        builder.Parameters.Add(this);

        _parameterType = type;
    }

    /// <summary>
    /// Sets the name of the parameter.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <returns>The builder to chain calls with.</returns>
    public CommandParameterBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the description of the parameter.
    /// </summary>
    /// <param name="description">The description of the parameter.</param>
    /// <returns>The builder to chain calls with.</returns>
    public CommandParameterBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the default value of the parameter. This must match the parameter's type.
    /// </summary>
    /// <param name="value">The default value.</param>
    /// <typeparam name="T">The parameter type.</typeparam>
    /// <remarks>
    /// Invoking this method will mark the parameter as optional regardless of the value
    /// passed.
    /// </remarks>
    /// <returns>The builder to chain calls with.</returns>
    public CommandParameterBuilder WithDefaultValue<T>([MaybeNull] T value)
    {
        if (typeof(T).IsValueType && !typeof(T).IsNullableStruct() && value is null)
        {
            throw new InvalidOperationException("Non-nullable value types must have a non-null default value.");
        }

        if (value is not null && _parameterType is { } parameterType && !parameterType.IsInstanceOfType(value))
        {
            throw new ArgumentException
            (
                "The default value must match the parameter's type.",
                nameof(value)
            );
        }

        _defaultValue = value;
        _isOptional = true;

        return this;
    }

    /// <summary>
    /// Sets the type of the parameter.
    /// </summary>
    /// <param name="type">The type to set the parameter to.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the type set on the builder does not match the default value (if set).</exception>
    public CommandParameterBuilder WithType(Type type)
    {
        if (_defaultValue is not null && _defaultValue.GetType() != type)
        {
            throw new InvalidOperationException("The type of the parameter must match the default value's type.");
        }

        _parameterType = type;
        return this;
    }

    /// <summary>
    /// Sets the parameter to be greedy.
    /// </summary>
    /// <returns>The builder to chain calls with.</returns>
    public CommandParameterBuilder IsGreedy()
    {
        _isGreedy = true;

        return this;
    }

    /// <summary>
    /// Sets the parameter to be a named switch.
    /// </summary>
    /// <param name="defaultValue">The default value of the switch.</param>
    /// <param name="shortOrLongName">Either the short name (e.g. '-o') or the long name (e.g. '--switch') of the switch.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <remarks>
    /// This method will set a default value for the parameter, which will
    /// implicitly make it optional.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if both tuple values in <paramref name="shortOrLongName"/> are <c>null</c>.</exception>
    public CommandParameterBuilder IsSwitch(bool defaultValue, OneOf<char, string, (char Short, string Long)> shortOrLongName = default)
    {
        _defaultValue = defaultValue;
        _parameterType = typeof(bool);

        var canBeSwitch = !_attributes.Any(r => r is OptionAttribute);

        if (!canBeSwitch)
        {
            throw new InvalidOperationException("A parameter marked as an option cannot be a switch.");
        }

        var attribute = shortOrLongName.Match
        (
            shortName => new SwitchAttribute(shortName),
            longName => new SwitchAttribute(longName),
            shortAndLongName => new SwitchAttribute(shortAndLongName.Short, shortAndLongName.Long)
        );

        _attributes.Add(attribute);

        return this;
    }

    /// <summary>
    /// Marks the parameter as being a named option.
    /// </summary>
    /// <param name="shortOrLongName">Either the short name (e.g. '-o') or the long name (e.g. '--option') of the option.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if both values for <paramref name="shortOrLongName"/> are null..</exception>
    public CommandParameterBuilder IsOption(OneOf<char, string, (char Short, string Long)> shortOrLongName = default)
    {
        var canBeOption = !_attributes.Any(r => r is SwitchAttribute);

        if (!canBeOption)
        {
            throw new InvalidOperationException("A parameter marked as an option cannot be a switch.");
        }

        var attribute = shortOrLongName.Match
        (
            shortName => new OptionAttribute(shortName),
            longName => new OptionAttribute(longName),
            shortAndLongName => new OptionAttribute(shortAndLongName.Short, shortAndLongName.Long)
        );

        _attributes.Add(attribute);

        return this;
    }

    /// <summary>
    /// Adds an attribute to the parameter. Conditions must be added via <see cref="AddCondition"/>.
    /// </summary>
    /// <param name="attribute">The attriubte to add.</param>
    /// <returns>The builder to chain calls with.</returns>
    public CommandParameterBuilder AddAttribute(Attribute attribute)
    {
        if (attribute is ConditionAttribute)
        {
            throw new InvalidOperationException("Conditions must be added via AddCondition.");
        }

        _attributes.Add(attribute);
        return this;
    }

    /// <summary>
    /// Adds a condition to the parameter.
    /// </summary>
    /// <param name="condition">The condition to add.</param>
    /// <returns>The builder to chain with.</returns>
    public CommandParameterBuilder AddCondition(ConditionAttribute condition)
    {
        _conditions.Add(condition);
        return this;
    }

    /// <summary>
    /// Finishes creating the parameter, and returns the original builder.
    /// </summary>
    /// <returns>The builder this parameter belongs to.</returns>
    public CommandBuilder Finish()
    {
        return _builder;
    }

    /// <summary>
    /// Builds the current builder into a <see cref="IParameterShape"/>.
    /// </summary>
    /// <returns>The constructed <see cref="IParameterShape"/>.</returns>
    public IParameterShape Build()
    {
        var rangeAttribute = _attributes.OfType<RangeAttribute>().SingleOrDefault();
        var optionAttribute = _attributes.OfType<OptionAttribute>().SingleOrDefault();

        if (optionAttribute is SwitchAttribute)
        {
            if (_defaultValue is not bool || _parameterType != typeof(bool))
            {
                throw new InvalidOperationException
                (
                 $"A switch parameter must be a {typeof(bool)}" +
                 $" (The parameter \"{_name}\" was marked as ({_parameterType})."
                );
            }
        }

        return optionAttribute is null
                   ? CreatePositionalParameterShape(rangeAttribute)
                   : CreateNamedParameterShape(optionAttribute, rangeAttribute);
    }

    private IParameterShape CreateNamedParameterShape(OptionAttribute optionAttribute, RangeAttribute? rangeAttribute)
    {
        var isCollection = _parameterType.IsSupportedCollection();

        IParameterShape newNamedParameter;
        if (optionAttribute is SwitchAttribute)
        {
            newNamedParameter = CreateNamedSwitchParameterShape(optionAttribute);
        }
        else if (!isCollection)
        {
            newNamedParameter = _isGreedy
                ? CreateNamedSingleValueParameterShape(optionAttribute)
                : CreateGreedyNamedSingleValueParameterShape(optionAttribute);
        }
        else
        {
            newNamedParameter = CreateNamedCollectionParameterShape(optionAttribute, rangeAttribute);
        }

        return newNamedParameter;
    }

    private IParameterShape CreateNamedCollectionParameterShape
    (
        OptionAttribute optionAttribute,
        RangeAttribute? rangeAttribute
    )
    {
        var description = _description ?? Constants.DefaultDescription;

        IParameterShape newNamedParameter = new NamedCollectionParameterShape
        (
            optionAttribute.ShortName,
            optionAttribute.LongName,
            rangeAttribute?.GetMin(),
            rangeAttribute?.GetMax(),
            _name,
            _parameterType,
            _isOptional,
            _defaultValue,
            _attributes,
            _conditions,
            description
        );

        return newNamedParameter;
    }

    private IParameterShape CreateNamedSwitchParameterShape(OptionAttribute optionAttribute)
    {
        if (!_isOptional)
        {
            throw new InvalidOperationException("Switches must have a default value.");
        }

        if (_parameterType != typeof(bool))
        {
            throw new InvalidOperationException("Switches must be booleans.");
        }

        var description = _description ?? Constants.DefaultDescription;

        IParameterShape newNamedParameter = new SwitchParameterShape
        (
            optionAttribute.ShortName,
            optionAttribute.LongName,
            _name,
            _parameterType,
            _isOptional,
            _defaultValue,
            _attributes,
            _conditions,
            description
        );

        return newNamedParameter;
    }

    private IParameterShape CreateNamedSingleValueParameterShape
    (
        OptionAttribute optionAttribute
    )
    {
        var description = _description ?? Constants.DefaultDescription;

        IParameterShape newNamedParameter = new NamedParameterShape
        (
            optionAttribute.ShortName,
            optionAttribute.LongName,
            _name,
            _parameterType,
            _isOptional,
            _defaultValue,
            _attributes,
            _conditions,
            description
        );

        return newNamedParameter;
    }

    private IParameterShape CreateGreedyNamedSingleValueParameterShape(OptionAttribute optionAttribute)
    {
        var description = _description ?? Constants.DefaultDescription;

        IParameterShape newNamedParameter = new NamedGreedyParameterShape
        (
            optionAttribute.ShortName,
            optionAttribute.LongName,
            _name,
            _parameterType,
            _isOptional,
            _defaultValue,
            _attributes,
            _conditions,
            description
        );

        return newNamedParameter;
    }

    private IParameterShape CreatePositionalParameterShape(RangeAttribute? rangeAttribute)
    {
        var description = _description ?? Constants.DefaultDescription;
        var isCollection = _parameterType.IsSupportedCollection();

        IParameterShape newPositionalParameter;
        if (!isCollection)
        {
            var greedyAttribute = _attributes.OfType<GreedyAttribute>().SingleOrDefault();

            newPositionalParameter = greedyAttribute is null
                ? new PositionalParameterShape
                  (
                      _name,
                      _parameterType,
                      _isOptional,
                      _defaultValue,
                      _attributes,
                      _conditions,
                      description
                  )
                : new PositionalGreedyParameterShape
                  (
                      _name,
                      _parameterType,
                      _isOptional,
                      _defaultValue,
                      _attributes,
                      _conditions,
                      description
                  );
        }
        else
        {
            newPositionalParameter = new PositionalCollectionParameterShape
            (
                rangeAttribute?.GetMin(),
                rangeAttribute?.GetMax(),
                _name,
                _parameterType,
                _isOptional,
                _defaultValue,
                _attributes,
                _conditions,
                description
            );
        }

        return newPositionalParameter;
    }
}
