using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace RoslynAnalizerLib.Model
{
    public class Class
    {
        internal Class(Microsoft.CodeAnalysis.Document document, Project project)
        {
            Project = project;
            Document = document;
            Name = document.Name;
            IsInicializated = true;
            ChildClasses = new List<Class>();
        }

        public int ClassId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        [ForeignKey(nameof(Project))]
        public int ProjectId { get; set; }

        // Navigation property
        public Project Project { get; set; }

        /// <summary>
        /// Clases definidas dentro de otra clase
        /// </summary>
        public List<Class>? ChildClasses { get; set; }

        /// <summary>
        /// ya fue inicializado
        /// </summary>
        public bool IsInicializated { get; set; }
        public List<ProjectElement> ProjectElements { get; set; }
        private Microsoft.CodeAnalysis.Document Document { get; }

        public async Task AnalyzeDocumentAsync()
        {
            ProjectElements = new List<ProjectElement>();

            // Obtener el árbol de sintaxis del documento
            var syntaxTree = await Document.GetSyntaxTreeAsync();
            var root = await syntaxTree.GetRootAsync();

            // Obtener el modelo semántico
            var semanticModel = await Document.GetSemanticModelAsync();

            // Recorrer todos los nodos del árbol de sintaxis
            foreach (var node in root.DescendantNodes())
            {
                Type type = node.GetType();
                Debug.WriteLine($"Analizando {type}");
                Project.types.Add(type);
                if (node is ClassDeclarationSyntax classDeclaration)
                {
                    //necesita un analisis propio para tomar los parametros/tipos, etc
                    var symbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                    ProjectElements.Add(new ProjectElement
                    {
                        Name = symbol.Name,
                        ElementType = ProjectElementType.Class,
                        Project = this.Project
                    });
                }
                else if (node is MethodDeclarationSyntax methodDeclarationSyntax)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
                    ProjectElements.Add(new ProjectElement
                    {
                        Name = symbol.Name,
                        ElementType = ProjectElementType.Method,
                        Project = this.Project
                    });
                }
                else if (node is UsingDirectiveSyntax usingDirectiveSyntax)
                {
                    Debug.Assert(usingDirectiveSyntax.Name != null);
                    string usingName = usingDirectiveSyntax.Name.ToString();
                    ProjectElements.Add(new ProjectElement
                    {
                        Name = usingName,
                        ElementType = ProjectElementType.Using,
                        Project = this.Project
                    });
                }
                else if (node is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(namespaceDeclarationSyntax);
                    ProjectElements.Add(new ProjectElement
                    {
                        Name = namespaceDeclarationSyntax.Name.ToString(),
                        ElementType = ProjectElementType.Namespace,
                        Project = Project
                    });
                }
                else if (node is InterfaceDeclarationSyntax interfaceDeclaration)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(interfaceDeclaration);
                    ProjectElements.Add(new ProjectElement
                    {
                        Name = symbol.Name,
                        ElementType = ProjectElementType.Interface,
                        Project = Project
                    });
                }
                else if (node is ClassDeclarationSyntax)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(node);
                    ProjectElements.Add(new ProjectElement
                    {
                        Name = symbol.Name,
                        ElementType = DetermineElementType(symbol),
                        Project = Project,
                    });
                }
            }
        }
        private ProjectElementType DetermineElementType(ISymbol symbol)
        {
            // Puedes implementar tu propia lógica para determinar el tipo de elemento
            // basado en atributos, interfaces implementadas, etc.
            var name = symbol.Name.ToLower();
            if (name.Contains("service"))
                return ProjectElementType.Service;
            if (name.Contains("controller"))
                return ProjectElementType.Controller;
            if (name.Contains("repository"))
                return ProjectElementType.Repository;

            return ProjectElementType.Other;
        }

    }
}
