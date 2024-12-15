using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CoreLib.Dbs.Models
{
    public class DbTemplate
    {
        [Key]
        public string TemplateID { get; set; }

        [Required]
        public int TemplateTypeID { get; set; }

        [ForeignKey(nameof(TemplateTypeID))]
        public DbTemplateType TemplateType { get; set; }

        public int? ConditionTypeID { get; set; }

        [ForeignKey(nameof(ConditionTypeID))]
        public DbConditionType ConditionType { get; set; }

        public string SuccessNextTemplateID { get; set; }

        public string FailureNextTemplateID { get; set; }

        public ICollection<DbCondition> Conditions { get; set; }

        public ICollection<DbCommand> Commands { get; set; }
    }
}
