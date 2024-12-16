using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSample.Dbs.Models
{
    public class DbTemplateType
    {
        [Key]
        public int TemplateTypeID { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<DbTemplate> Templates { get; set; }
    }
}
