using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoslynAnalizerLib.Model
{
    public class ProjectElement
    {
        public string Name { get; set; }

        public ProjectElementType ElementType { get; set; }
        public Project Project { get; set; }
        public Namespace Namespace { get; set; } // Añadimos esta propiedad

        public ICollection<ElementType> ElementTypes { get; set; }
    }



}
