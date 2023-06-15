using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Indexes
    {
        [Key]
        public int ID { get; set; }
        public Guid Guid { get; set; }
        public string Term { get; set; }
    }
}
