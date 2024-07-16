using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoslynAnalizerLib.Model
{
    public class ElementType
    {
        public string Name { get; set; }
        public ProjectElement ProjectElement { get; set; }
    }

}
