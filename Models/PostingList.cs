using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class PostingList
    {
        [Key]
        public int ID { get; set; }
        public Guid Guid { get; set; }
        public Indexes Indexs { get; set; }
        public int TermFrequency { get; set; }
        public Links Links { get; set; }
    }
}
