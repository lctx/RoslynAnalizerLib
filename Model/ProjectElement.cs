using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoslynAnalizerLib.Model
{
    public class ProjectElement
    {
        public int ProjectElementId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public ProjectElementType ElementType { get; set; }
        [ForeignKey(nameof(Project))]
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        [ForeignKey(nameof(Namespace))]
        public int NamespaceId { get; set; } // Añadimos esta propiedad
        public Namespace Namespace { get; set; } // Añadimos esta propiedad

        public ICollection<ElementType> ElementTypes { get; set; }
    }



}
