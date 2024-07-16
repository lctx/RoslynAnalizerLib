using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace RoslynAnalizerLib.Model
{
    public class Solution
    {
        public string Name { get; set; }
        public string LocalPath { get; set; }

        // Navigation property
        public ICollection<Project> Projects { get; set; }

        private Microsoft.CodeAnalysis.Solution RoslynSolution { get; set; }
        internal Solution(Microsoft.CodeAnalysis.Solution solution)
        {
            RoslynSolution = solution;
            LocalPath = RoslynSolution.FilePath;
            Name = System.IO.Path.GetFileNameWithoutExtension(RoslynSolution.FilePath);
            Projects = new List<Project>();
        }
        public async Task AnalizeProjects()
        {
            var tasks = RoslynSolution.Projects.Select(project =>
            {
                var projectData = new Model.Project(project, this);
                projectData.AnalizeClasses();
                Projects.Add(projectData);
                return projectData.AnalizeClassesComponents();
            }).ToList();

            // Ejecutar todas las tareas en paralelo y esperar a que todas terminen
            await Task.WhenAll(tasks);
            //List<Task> tasks = new List<Task>();
            //foreach (var project in RoslynSolution.Projects)
            //{
            //    //Task task = Task.Run(async () =>
            //    await Task.Run(async () =>
            //    {
            //        System.Threading.Thread.CurrentThread.Name = $"thread {project.Name}";
            //        Debug.WriteLine($"Analizando proyecto {project.Name}");
            //        var projectData = new Model.Project(project, this);
            //        projectData.AnalizeClasses();
            //        Projects.Add(projectData);
            //        await projectData.AnalizeClassesComponents();
            //    });
            //    //tasks.Add(task);
            //}
            //Task.WaitAll(tasks.ToArray());
        }

    }
}
