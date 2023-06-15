using BussinessLogic.Helpers;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineControlPanel
{
    public class LinksDataGridModel
    {
        public LinksDataGridModel(Links link)
        {
            this.Url = link.Url;
            switch (link.Status)
            {
                case (int)StaticValues.LinkStates.Valid:
                    this.Status = "تایید شده";
                    break;
                case (int)StaticValues.LinkStates.Invalid:
                    this.Status = "رد شده";
                    break;
                case (int)StaticValues.LinkStates.Extracted:
                    this.Status = "استخراج شده";
                    break;
                case (int)StaticValues.LinkStates.Indexed:
                    this.Status = "نمایه سازی شده";
                    break;
                default:
                    this.Status = "تایید شده";
                    break;
            }
            this.Title = link.Title;
            this.Keywords = link.Keywords;
            switch (link.DocumentType)
            {
                case (int)StaticValues.SupportedFileTypes.html:
                    this.DocumentTypeString = "HTML";
                    break;
                case (int)StaticValues.SupportedFileTypes.pdf:
                    this.DocumentTypeString = "PDF";
                    break;
                case (int)StaticValues.SupportedFileTypes.txt:
                    this.DocumentTypeString = "TEXT";
                    break;
                case (int)StaticValues.SupportedFileTypes.MSword:
                    this.DocumentTypeString = "Microsoft Word";
                    break;
                default:
                    this.DocumentTypeString = "HTML";
                    break;
            }
            this.Description = link.Description;
            this.Indexed = (link.Indexed)?"بله" : "خیر";
        }
        public string Url { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string DocumentTypeString { get; set; } 
        public string Description { get; set; }
        public string Indexed { get; set; }
    }
}
