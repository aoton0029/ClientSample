using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Dbs.Models
{
    public class DbKeyMaster
    {
        [Key]
        public int KeyID { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Condition> Conditions { get; set; }
    }
}
