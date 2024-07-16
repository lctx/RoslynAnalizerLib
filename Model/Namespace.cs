using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoslynAnalizerLib.Model
{
    public class Namespace
    {
        public string Name { get; set; }
        public Project Project { get; set; }
        public ICollection<ProjectElement> ProjectElements { get; set; }
    }

}
