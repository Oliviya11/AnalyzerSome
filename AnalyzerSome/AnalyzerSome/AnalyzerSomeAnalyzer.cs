using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzerSome
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AnalyzerSomeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AnalyzerSome";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {

            ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
            if (classDeclaration == null) return;

            BaseListSyntax baseList = classDeclaration.BaseList;
            if (baseList != null)
            {
                IEnumerable<SyntaxNode> childNodes = baseList.ChildNodes();
                if (childNodes != null)
                {
                    foreach (SyntaxNode node in childNodes)
                    {
                        var simpleBaseType = (SimpleBaseTypeSyntax)node;
                        if (simpleBaseType != null)
                        {
                            IEnumerable<SyntaxNode> baseNodes = simpleBaseType.ChildNodes();
                            if (baseNodes == null) return;

                            foreach (SyntaxNode baseNode in baseNodes)
                            {
                                IdentifierNameSyntax identifierName = (IdentifierNameSyntax)baseNode;
                                if (identifierName == null) return;

                                if (identifierName.Identifier == null) return;

                                string textName = identifierName.Identifier.Text;
                                if (string.IsNullOrEmpty(textName)) return;

                                if (textName == "ISome" || textName == "Some")
                                {
                                    IEnumerable<SyntaxToken> syntaxTokens = classDeclaration.ChildTokens();
                                    if (syntaxTokens == null) return;
                                    bool isSealedFound = false;
                                    Location classLocation = null;

                                    foreach (SyntaxToken token in syntaxTokens)
                                    {
                                        if (token.IsKind(SyntaxKind.ClassKeyword))
                                        {
                                            classLocation = token.GetLocation();
                                            break;
                                        }
                                    }

                                    foreach (SyntaxToken token in syntaxTokens)
                                    {
                                        if (token.IsKind(SyntaxKind.SealedKeyword))
                                        {
                                            isSealedFound = true;
                                            break;
                                        }
                                    }

                                    if (!isSealedFound)
                                    {
                                        var diagnostic = Diagnostic.Create(Rule, classLocation, "Class");
                                        context.ReportDiagnostic(diagnostic);
                                    }
                                }
                            }

                        }
                    }
                }
            }

        }
    }
}
