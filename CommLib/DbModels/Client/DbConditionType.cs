using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommLib
{
    public class DbConditionType
    {
        [Key]
        public int ConditionTypeID { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<DbTemplate> Templates { get; set; }
    }
}
