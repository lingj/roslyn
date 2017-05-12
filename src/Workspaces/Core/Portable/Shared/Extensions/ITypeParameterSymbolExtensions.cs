﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Shared.Extensions
{
    internal static class ITypeParameterSymbolExtensions
    {
        public static INamedTypeSymbol GetNamedTypeSymbolConstraint(this ITypeParameterSymbol typeParameter)
        {
            return typeParameter.ConstraintTypes.Select(s_getNamedTypeSymbol).WhereNotNull().FirstOrDefault();
        }

        private static readonly Func<ITypeSymbol, INamedTypeSymbol> s_getNamedTypeSymbol = type =>
        {
            return type is INamedTypeSymbol
                ? (INamedTypeSymbol)type
                : type is ITypeParameterSymbol
                    ? GetNamedTypeSymbolConstraint((ITypeParameterSymbol)type)
                    : null;
        };
    }
}
