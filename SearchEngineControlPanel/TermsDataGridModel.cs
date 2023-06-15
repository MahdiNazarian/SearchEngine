using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace SearchEngineControlPanel
{
    public class TermsDataGridModel
    {
        public TermsDataGridModel(Indexes index)
        {
            Term = index.Term;
        }
        string Term { get; set; }
    }
}
