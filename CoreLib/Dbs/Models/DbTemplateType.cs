using CoreLib.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Dbs.Models
{
    public class DbTemplateType
    {
        [Key]
        public int TemplateTypeID { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Template> Templates { get; set; }
    }
}
