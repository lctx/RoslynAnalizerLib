using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoslynAnalizerLib.Model
{
    public class ElementType
    {
        public int ElementTypeId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        [ForeignKey(nameof(ProjectElement))]
        public int ProjectElementId { get; set; }

        // Navigation property
        public ProjectElement ProjectElement { get; set; }
    }

}
