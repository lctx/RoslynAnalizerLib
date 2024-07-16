using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RoslynAnalizerLib.Model
{
    public class Solution
    {
        public int SolutionId { get; set; }

        [Required]
        [StringLength(200)]
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
    }
}
