using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.Reference
{
    public class DynamicReferenceEditorTemplateViewModel
    {
        public Guid ReferenceId { get; set; }
        public Guid SelectedItemGuid { get; set; }
        public String FieldName { get; set; }
        public string FieldStringValue { get; set; }
        public string FieldStringHeader { get; set; }
        public int Width { get; set; }

    }
}