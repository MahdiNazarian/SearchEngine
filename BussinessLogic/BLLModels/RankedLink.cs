using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLogic.BLLModels
{
    public class RankedLink
    {
        public Links link { get; set; }
        public double score { get; set; }
    }
}
