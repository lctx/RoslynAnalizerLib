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
        public async Task<Model.Solution> AnalyzeSolutionAsync(string solutionPath)
        {
            using (var workspace = MSBuildWorkspace.Create())
            {
                Debug.WriteLine($"Analizando {solutionPath}");
                var roslynSolution = await workspace.OpenSolutionAsync(solutionPath);
                var solutionData = new Model.Solution(roslynSolution);

                foreach (var project in roslynSolution.Projects)
                {
                    //Todo: Hacerlo con Hilos para que sea mas rapido
                    var projectData = AnalyzeProjectAsync(project, solutionData);
                    //var projectData = await AnalyzeProjectAsyncBuild(project);
                    await projectData.AnalizeClassesComponents();
                    solutionData.Projects.Add(projectData);
                }

                return solutionData;
            }
        }
        private Model.Project AnalyzeProjectAsync(Microsoft.CodeAnalysis.Project project, Model.Solution solution)
        {
            Debug.WriteLine($"Analizando {project.Name}");
            var projectData = new Model.Project(project,solution);
            projectData.AnalizeClasses();

            return projectData;
        }

        /// <summary>
        /// Analiza el proyecto pero compilandolo
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private async Task<Model.Project> AnalyzeProjectAsyncBuild(Microsoft.CodeAnalysis.Project project, Model.Solution solution)
        {
            Debug.WriteLine($"Analizando {project.Name}");
            var projectData = new Model.Project(project, solution);
            var compilation = await project.GetCompilationAsync();
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var root = await syntaxTree.GetRootAsync();

                var namespaceDeclarations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax>();
                foreach (var namespaceDeclaration in namespaceDeclarations)
                {
                    var namespaceData = await AnalyzeNamespaceAsync(namespaceDeclaration, semanticModel);
                    projectData.Namespaces.Add(namespaceData);
                }
            }

            return projectData;
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
