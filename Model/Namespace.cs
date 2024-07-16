using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoslynAnalizerLib.Model
{
    public class Namespace
    {
        public int NamespaceId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        [ForeignKey(nameof(Project))]
        public int ProjectId { get; set; }

        // Navigation property
        public Project Project { get; set; }
        public ICollection<ProjectElement> ProjectElements { get; set; }
    }

}
