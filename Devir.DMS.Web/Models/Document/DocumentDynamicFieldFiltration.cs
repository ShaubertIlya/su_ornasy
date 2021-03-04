using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Devir.DMS.Web.Models.Document
{
    public class DocumentDynamicFieldFiltration
    {
        public Guid FieldTemplateId { get; set; }
        public string FiledName { get; set; }
        public List<SelectListItem> Options { get; set; }
    }
}