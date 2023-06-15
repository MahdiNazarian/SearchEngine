using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Links
    {
        [Key]
        public int ID { get; set; }
        public Guid Guid { get; set; }
        public string Url { get; set; }
        public int Status { get; set; }
        public string Title { get; set; }
        public string Keywords { get; set; }
        public int DocumentType { get; set; }
        [DefaultValue(false)]
        public bool Indexed { get; set; }
        public string Description { get; set; }

    }
}
