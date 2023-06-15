using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLogic.Helpers
{
    public static class StaticValues
    {
        public enum LinkStates
        {
            Valid = 0,
            Invalid = 1,
            Extracted = 2,
            Indexed = 3,
        }
        public enum SupportedFileTypes
        {
            html = 0,
            pdf = 1,
            txt = 2,
            MSword = 3,
        }
        public static string[] StopWords = {"I","a","about","an","are","as","at","be","by","com","for","from","in","is","it","of","on","or","that","the","this","to","the","www","s"};
    }
}
