using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace RoslynAnalizerLib.Model
{
    public class Project
    {
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        [ForeignKey(nameof(Solution))]
        public int SolutionId { get; set; }

        public List<Class> Classes { get; set; }

        // Navigation property
        public Solution Solution { get; set; }
        public ICollection<ProjectElement> ProjectElements { get; set; }
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
            SolutionId = solution.SolutionId;
            Name = project.Name;
            RoslynProject = project;
            Namespaces = new List<Namespace>();
            ProjectElements = new List<ProjectElement>();
            Classes = new List<Class>();
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
            Classes = new List<Class>();
            foreach (var document in RoslynProject.Documents)
            {
                Debug.WriteLine($"Clase {document.Name}");
                Class _class = new Class(document, this);
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
            }
        }



    }
}
