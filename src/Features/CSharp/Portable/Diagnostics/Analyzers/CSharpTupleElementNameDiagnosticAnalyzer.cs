﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Options;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Diagnostics.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class CSharpTupleElementNameDiagnosticAnalyzer : DiagnosticAnalyzer, IBuiltInAnalyzer
    {
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(CSharpFeaturesResources.ERR_CantChangeTupleNamesOnOverride), WorkspacesResources.ResourceManager, typeof(WorkspacesResources));
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(CSharpFeaturesResources.ERR_CantChangeTupleNamesOnOverride_Title), FeaturesResources.ResourceManager, typeof(FeaturesResources));
        private static readonly DiagnosticDescriptor s_descriptor = new DiagnosticDescriptor(
            IDEDiagnosticIds.TupleElementNamesMatchBaseDiagnosticId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Style,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true);

        private static readonly Func<ITypeSymbol, ITypeSymbol, object, bool> CompareTupleElements =
            (left, right, _) =>
            {
                if (right.IsTupleType && !left.IsTupleType)
                {
                    return true;
                }

                if (!right.IsTupleType)
                {
                    return false;
                }

                var rightElements = ((INamedTypeSymbol)right).TupleElements;
                return rightElements.Any(field => !field.IsImplicitlyDeclared && field.Name != null);
            };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(s_descriptor);

        public bool OpenFileOnly(Workspace workspace)
        {
            var preferTupleElementNamesMatchBase = workspace.Options.GetOption(
                CodeStyleOptions.PreferTupleNamesMatchBase, LanguageNames.CSharp).Notification;

            return !(preferTupleElementNamesMatchBase == NotificationOption.Warning || preferTupleElementNamesMatchBase == NotificationOption.Error);
        }

        public DiagnosticAnalyzerCategory GetAnalyzerCategory()
            => DiagnosticAnalyzerCategory.SemanticSpanAnalysis;

        public sealed override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
            context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
            context.RegisterSymbolAction(AnalyzeEvent, SymbolKind.Event);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var symbol = (IMethodSymbol)context.Symbol;
            if (!ContainsTupleType(symbol))
            {
                // Stop immediately if no tuple types are involved
                return;
            }

            if (ContainsTupleTypeWithNames(symbol))
            {
                // This is handled by the compiler
                return;
            }

            var overriddenMethod = symbol.OverriddenMethod;
            if (overriddenMethod != null && ContainsTupleTypeWithNames(overriddenMethod))
            {
                var optionSet = context.Options.GetDocumentOptionSetAsync(symbol.Locations[0].SourceTree, context.CancellationToken).GetAwaiter().GetResult();
                context.ReportDiagnostic(Diagnostic.Create(GetDescriptor(optionSet), symbol.Locations[0]));
                return;
            }

            foreach (var implementedInterface in symbol.ContainingType.Interfaces)
            {
                foreach (var member in implementedInterface.GetMembers(symbol.Name).OfType<IMethodSymbol>())
                {
                    if (symbol.ContainingType.FindImplementationForInterfaceMember(member) == symbol)
                    {
                        if (ContainsTupleTypeWithNames(member))
                        {
                            var optionSet = context.Options.GetDocumentOptionSetAsync(symbol.Locations[0].SourceTree, context.CancellationToken).GetAwaiter().GetResult();
                            context.ReportDiagnostic(Diagnostic.Create(GetDescriptor(optionSet), symbol.Locations[0]));
                            return;
                        }

                        break;
                    }
                }
            }
        }

        private static void AnalyzeProperty(SymbolAnalysisContext context)
        {
            var symbol = (IPropertySymbol)context.Symbol;
            if (!ContainsTupleType(symbol))
            {
                // Stop immediately if no tuple types are involved
                return;
            }

            if (ContainsTupleTypeWithNames(symbol))
            {
                // This is handled by the compiler
                return;
            }

            var overriddenProperty = symbol.OverriddenProperty;
            if (overriddenProperty != null && ContainsTupleTypeWithNames(overriddenProperty))
            {
                var optionSet = context.Options.GetDocumentOptionSetAsync(symbol.Locations[0].SourceTree, context.CancellationToken).GetAwaiter().GetResult();
                context.ReportDiagnostic(Diagnostic.Create(GetDescriptor(optionSet), symbol.Locations[0]));
                return;
            }

            foreach (var implementedInterface in symbol.ContainingType.Interfaces)
            {
                foreach (var member in implementedInterface.GetMembers(symbol.Name).OfType<IPropertySymbol>())
                {
                    if (symbol.ContainingType.FindImplementationForInterfaceMember(member) == symbol)
                    {
                        if (ContainsTupleTypeWithNames(member))
                        {
                            var optionSet = context.Options.GetDocumentOptionSetAsync(symbol.Locations[0].SourceTree, context.CancellationToken).GetAwaiter().GetResult();
                            context.ReportDiagnostic(Diagnostic.Create(GetDescriptor(optionSet), symbol.Locations[0]));
                            return;
                        }

                        break;
                    }
                }
            }
        }

        private static void AnalyzeEvent(SymbolAnalysisContext context)
        {
            var symbol = (IEventSymbol)context.Symbol;
            if (!ContainsTupleType(symbol))
            {
                // Stop immediately if no tuple types are involved
                return;
            }

            if (ContainsTupleTypeWithNames(symbol))
            {
                // This is handled by the compiler
                return;
            }

            var overriddenEvent = symbol.OverriddenEvent;
            if (overriddenEvent != null && ContainsTupleType(overriddenEvent))
            {
                var optionSet = context.Options.GetDocumentOptionSetAsync(symbol.Locations[0].SourceTree, context.CancellationToken).GetAwaiter().GetResult();
                context.ReportDiagnostic(Diagnostic.Create(GetDescriptor(optionSet), symbol.Locations[0]));
                return;
            }

            foreach (var implementedInterface in symbol.ContainingType.Interfaces)
            {
                foreach (var member in implementedInterface.GetMembers(symbol.Name).OfType<IEventSymbol>())
                {
                    if (symbol.ContainingType.FindImplementationForInterfaceMember(member) == symbol)
                    {
                        if (ContainsTupleType(member))
                        {
                            var optionSet = context.Options.GetDocumentOptionSetAsync(symbol.Locations[0].SourceTree, context.CancellationToken).GetAwaiter().GetResult();
                            context.ReportDiagnostic(Diagnostic.Create(GetDescriptor(optionSet), symbol.Locations[0]));
                            return;
                        }

                        break;
                    }
                }
            }
        }

        private static DiagnosticDescriptor GetDescriptor(OptionSet optionSet)
        {
            var optionValue = optionSet.GetOption(CodeStyleOptions.PreferTupleNamesMatchBase, LanguageNames.CSharp);
            if (optionValue.Notification.Value != DiagnosticSeverity.Hidden)
            {
                return new DiagnosticDescriptor(
                    IDEDiagnosticIds.TupleElementNamesMatchBaseDiagnosticId,
                    s_localizableTitle,
                    s_localizableMessage,
                    DiagnosticCategory.Style,
                    optionValue.Notification.Value,
                    isEnabledByDefault: true);
            }

            return s_descriptor;
        }

        internal static bool ContainsTupleType(ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Method:
                    var method = (IMethodSymbol)symbol;
                    return ContainsTupleType(method.ReturnType) || method.Parameters.Any(parameter => ContainsTupleType(parameter.Type));

                case SymbolKind.Property:
                    return ContainsTupleType(((IPropertySymbol)symbol).Type);

                case SymbolKind.Event:
                    return ContainsTupleType(((IEventSymbol)symbol).Type);

                case SymbolKind.ArrayType:
                case SymbolKind.DynamicType:
                case SymbolKind.ErrorType:
                case SymbolKind.NamedType:
                case SymbolKind.PointerType:
                case SymbolKind.TypeParameter:
                    return VisitType((ITypeSymbol)symbol, type => IsTupleType(type)) != null;

                default:
                    // We currently don't need to use this method for fields or locals
                    throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
            }
        }

        private static bool IsTupleType(ITypeSymbol symbol)
        {
            if (symbol.IsTupleType)
            {
                return true;
            }

            if (symbol.TypeKind != TypeKind.Struct)
            {
                return false;
            }

            var namedTypeSymbol = (INamedTypeSymbol)symbol;
            if (!namedTypeSymbol.IsGenericType)
            {
                return false;
            }

            return namedTypeSymbol.Name == nameof(ValueTuple)
                && namedTypeSymbol.ContainingNamespace.Name == nameof(System)
                && namedTypeSymbol.ContainingNamespace.ContainingNamespace.IsGlobalNamespace;
        }

        internal static bool ContainsTupleTypeWithNames(ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Method:
                    var method = (IMethodSymbol)symbol;
                    return ContainsTupleTypeWithNames(method.ReturnType) || method.Parameters.Any(parameter => ContainsTupleTypeWithNames(parameter.Type));

                case SymbolKind.Property:
                    return ContainsTupleTypeWithNames(((IPropertySymbol)symbol).Type);

                case SymbolKind.Event:
                    return ContainsTupleTypeWithNames(((IEventSymbol)symbol).Type);

                case SymbolKind.ArrayType:
                case SymbolKind.DynamicType:
                case SymbolKind.ErrorType:
                case SymbolKind.NamedType:
                case SymbolKind.PointerType:
                case SymbolKind.TypeParameter:
                    return VisitType((ITypeSymbol)symbol, type => type.IsTupleType && ((INamedTypeSymbol)type).TupleElements.Any(field => !field.IsImplicitlyDeclared && field.Name != null)) != null;

                default:
                    // We currently don't need to use this method for fields or locals
                    throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
            }
        }

        internal static ITypeSymbol VisitType(ITypeSymbol type, Func<ITypeSymbol, bool> predicate)
        {
            // In order to handle extremely "deep" types like "int[][][][][][][][][]...[]"
            // or int*****************...* we implement manual tail recursion rather than 
            // doing the natural recursion.

            ITypeSymbol current = type;

            while (true)
            {
                if (predicate(current))
                {
                    return current;
                }

                switch (current.TypeKind)
                {
                    case TypeKind.Error:
                    case TypeKind.Dynamic:
                    case TypeKind.TypeParameter:
                    case TypeKind.Submission:
                    case TypeKind.Enum:
                        return null;

                    case TypeKind.Class:
                    case TypeKind.Struct:
                    case TypeKind.Interface:
                    case TypeKind.Delegate:
                        if (current.IsTupleType)
                        {
                            // turn tuple type elements into parameters
                            current = ((INamedTypeSymbol)current).TupleUnderlyingType;
                        }

                        foreach (var nestedType in ((INamedTypeSymbol)current).TypeArguments)
                        {
                            var result = VisitType(nestedType, predicate);
                            if (result != null)
                            {
                                return result;
                            }
                        }
                        return null;

                    case TypeKind.Array:
                        current = ((IArrayTypeSymbol)current).ElementType;
                        continue;

                    case TypeKind.Pointer:
                        current = ((IPointerTypeSymbol)current).PointedAtType;
                        continue;

                    default:
                        throw ExceptionUtilities.UnexpectedValue(current.TypeKind);
                }
            }
        }
    }
}
