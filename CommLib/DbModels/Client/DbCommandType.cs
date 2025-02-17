using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommLib
{
    public class DbCommandType
    {
        [Key]
        public int CommandTypeID { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<DbCommand> Commands { get; set; }
    }
}
