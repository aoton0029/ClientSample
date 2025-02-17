using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommLib
{
    public class DbCommand
    {
        [Key]
        public int CommandID { get; set; }

        [Required]
        public string TemplateID { get; set; }

        [ForeignKey(nameof(TemplateID))]
        public DbTemplate Template { get; set; }

        [Required]
        public int CommandTypeID { get; set; }

        [ForeignKey(nameof(CommandTypeID))]
        public DbCommandType CommandType { get; set; }

        [Required]
        public string TargetDeviceID { get; set; }

        public string Data { get; set; }
    }
}
