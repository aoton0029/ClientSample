﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommLib
{
    public class DbKeyMaster
    {
        [Key]
        public int KeyID { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<DbCondition> Conditions { get; set; }
    }
}
