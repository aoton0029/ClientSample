using CoreLib.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Dbs.Models
{
    public class DbCondition
    {
        [Key]
        public int ConditionID { get; set; }

        [Required]
        public string TemplateID { get; set; }

        [ForeignKey(nameof(TemplateID))]
        public DbTemplate Template { get; set; }

        [Required]
        public int KeyID { get; set; }

        [ForeignKey(nameof(KeyID))]
        public DbKeyMaster KeyMaster { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
