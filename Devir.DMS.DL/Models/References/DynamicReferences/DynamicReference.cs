using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.References.DynamicReferences
{
    public class DynamicReference: ModelBase
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public List<DynamicReferenceFieldTemplate> FieldTemplates { get; set; }

        public DynamicReference()
        {
            FieldTemplates = new List<DynamicReferenceFieldTemplate>();
        }
    }
}
