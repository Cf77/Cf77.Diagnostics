using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cf77.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AbpComplexTypedParamsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Cf770001";
        private const string _category = "Abp services";
        private const string _abpServiceInterfaceName = "Abp.Application.Services.IApplicationService";

        private static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor
            (
                DiagnosticId,
                "Abp service method contains more than one complex-typed parameter",
                "Abp service method '{0}' contains more than one complex-typed parameter",
                _category,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: "Abp service methods should not have more then one parameter of complex type"
            );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSemanticModelAction(ctx => Analyze(ctx.ReportDiagnostic, ctx.SemanticModel));
        }

        private void Analyze(Action<Diagnostic> report, SemanticModel semanticModel)
        {
            var root = semanticModel.SyntaxTree.GetCompilationUnitRoot();
            var abpServices = root
                .DescendantNodes()
                .Where(node =>
                {
                    var isClassOrInterface = node is ClassDeclarationSyntax || node is InterfaceDeclarationSyntax;
                    if (!isClassOrInterface)
                    {
                        return false;
                    }

                    var typeInfo = semanticModel.GetDeclaredSymbol(node as BaseTypeDeclarationSyntax);
                    if (typeInfo == default)
                    {
                        return false;
                    }

                    return typeInfo.AllInterfaces.Any(@interface =>
                    {
                        var @namespace = @interface.ContainingNamespace.ToDisplayString();
                        return $"{@namespace}.{@interface.Name}" == _abpServiceInterfaceName;
                    });
                });

            foreach (var abpService in abpServices)
            {
                var targetMethods = abpService
                    .ChildNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Select(method =>
                    (
                        method,
                        parameters: method.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault()
                    ))
                    .Where(methodAndParameters =>
                    {
                        var (method, parameters) = methodAndParameters;
                        return parameters != default && parameters.Parameters.Count > 1;
                    });

                foreach (var methodAndParameters in targetMethods)
                {
                    var (method, parameters) = methodAndParameters;
                    var complexParametersCount = parameters
                        .Parameters
                        .Where(p => IsComplexParameterType(p))
                        .TakeWhile((p, i) => i < 2)
                        .Count();

                    if (complexParametersCount > 1)
                    {
                        var methodSymbol = semanticModel.GetDeclaredSymbol(method);
                        if (methodSymbol == default)
                        {
                            continue;
                        }

                        var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name);
                        report(diagnostic);
                    }
                }
            }

            bool IsComplexParameterType(ParameterSyntax syntax)
                => !syntax.Type.IsKind(SyntaxKind.PredefinedType);
        }
    }
}
