using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CoreLib.Dbs.Models
{
    public class DbCommandType
    {
        [Key]
        public int CommandTypeID { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Command> Commands { get; set; }
    }
}
