using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace RoslynAnalizerLib.Model
{
    public class Project
    {
        public string Name { get; set; }
        public List<CodeFile> Classes { get; set; }

        // Navigation property
        public Solution Solution { get; set; }
        public List<ProjectElement> ProjectElements { get; set; }
        public ICollection<Namespace> Namespaces { get; set; }

        private Microsoft.CodeAnalysis.Project RoslynProject { get; set; }
        /// <summary>
        /// ya fue inicializado
        /// </summary>
        public bool IsInicializated { get; set; }
        internal HashSet<Type> types = new HashSet<Type>();
        internal Project(Microsoft.CodeAnalysis.Project project, Model.Solution solution)
        {
            Solution = solution;
            Name = project.Name;
            RoslynProject = project;
            Namespaces = new List<Namespace>();
            ProjectElements = new List<ProjectElement>();
            Classes = new List<CodeFile>();
            IsInicializated = true;
        }

        /// <summary>
        /// Analiza el proyecto
        /// </summary>
        public async Task AnalizeProject()
        {
            Debug.Assert(IsInicializated);
            var compilation = await RoslynProject.GetCompilationAsync();

        }

        /// <summary>
        /// Analiza las clases del proyecto
        /// </summary>
        /// <returns></returns>
        public void AnalizeClasses()
        {
            Debug.Assert(IsInicializated);
            Classes = new List<CodeFile>();
            foreach (var document in RoslynProject.Documents)
            {
                Debug.WriteLine($"Clase {document.Name}");
                CodeFile _class = new CodeFile(document, this);
                //await _class.AnalizeClass();
                Classes.Add(_class);
            }
        }

        /// <summary>
        /// analiza los componentes de las clases del proyecto
        /// </summary>
        /// <returns></returns>
        public async Task AnalizeClassesComponents()
        {
            foreach (var _class in Classes)
            {
                await _class.AnalyzeDocumentAsync();
                ProjectElements.AddRange(_class.ProjectElements);
            }
        }

        ///// <summary>
        ///// Analiza el proyecto pero compilandolo
        ///// </summary>
        ///// <param name="project"></param>
        ///// <returns></returns>
        //private async Task<Model.Project> AnalyzeProjectAsyncBuild(Microsoft.CodeAnalysis.Project project, Model.Solution solution)
        //{
        //    Debug.WriteLine($"Analizando {project.Name}");
        //    var projectData = new Model.Project(project, solution);
        //    var compilation = await project.GetCompilationAsync();
        //    foreach (var syntaxTree in compilation.SyntaxTrees)
        //    {
        //        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        //        var root = await syntaxTree.GetRootAsync();

        //        var namespaceDeclarations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax>();
        //        foreach (var namespaceDeclaration in namespaceDeclarations)
        //        {
        //            var namespaceData = await AnalyzeNamespaceAsync(namespaceDeclaration, semanticModel);
        //            projectData.Namespaces.Add(namespaceData);
        //        }
        //    }

        //    return projectData;
        //}

    }
}
