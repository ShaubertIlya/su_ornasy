using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.References.DynamicReferences
{
    public class DynamicRecord: ModelBase
    {
        public Guid RecordId { get; set; }
        public Guid DynamicReferenceId {get;set;}
        public Guid DynamicReferenceFieldTemplateId { get; set; }
        public DynamicValue Value { get; set; }
    
    }
}
