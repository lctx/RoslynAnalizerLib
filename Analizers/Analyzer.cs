using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using RoslynAnalizerLib.Model;
using System.Diagnostics;

namespace RoslynAnalizerLib.Analizers
{

    public class Analyzer
    {
        /// <summary>
        /// Analiza una solución asincronicamente
        /// </summary>
        /// <param name="solutionPath">Path de la solución de .net</param>
        /// <returns></returns>
        public async Task<Model.Solution> AnalyzeSolutionAsync(string solutionPath)
        {
            using (var workspace = MSBuildWorkspace.Create())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Debug.WriteLine($"Analizando {solutionPath}");
                var roslynSolution = await workspace.OpenSolutionAsync(solutionPath);
                var solutionData = new Model.Solution(roslynSolution);
                await solutionData.AnalizeProjects();
                stopwatch.Stop();
                return solutionData;
            }
        }

        private async Task<Model.Namespace> AnalyzeNamespaceAsync(Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax namespaceDeclaration, SemanticModel semanticModel)
        {
            var namespaceData = new Model.Namespace
            {
                Name = namespaceDeclaration.Name.ToString(),
                ProjectElements = new List<Model.ProjectElement>()
            };

            var typeDeclarations = namespaceDeclaration.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>();
            foreach (var typeDeclaration in typeDeclarations)
            {
                var projectElementData = await AnalyzeTypeAsync(typeDeclaration, semanticModel);
                namespaceData.ProjectElements.Add(projectElementData);
            }

            return namespaceData;
        }

        private async Task<Model.ProjectElement> AnalyzeTypeAsync(Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
        {
            var elementType = ProjectElementType.Other;
            if (typeDeclaration is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax) elementType = ProjectElementType.Class;
            else if (typeDeclaration is Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax) elementType = ProjectElementType.Interface;

            var projectElementData = new Model.ProjectElement
            {
                Name = typeDeclaration.Identifier.ValueText,
                ElementType = elementType,
                ElementTypes = new List<Model.ElementType>()
            };

            var memberDeclarations = typeDeclaration.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax>();
            foreach (var memberDeclaration in memberDeclarations)
            {
                ITypeSymbol memberTypeSymbol = null;
                if (memberDeclaration is Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax propertyDeclaration)
                {
                    memberTypeSymbol = semanticModel.GetTypeInfo(propertyDeclaration.Type).Type;
                }
                else if (memberDeclaration is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax methodDeclaration)
                {
                    memberTypeSymbol = semanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type;
                }

                if (memberTypeSymbol != null)
                {
                    var elementTypeData = new Model.ElementType
                    {
                        Name = memberTypeSymbol.ToDisplayString(),
                    };

                    projectElementData.ElementTypes.Add(elementTypeData);
                }
            }

            return projectElementData;
        }
    }

}
