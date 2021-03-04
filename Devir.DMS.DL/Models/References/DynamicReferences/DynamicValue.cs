using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.References.DynamicReferences
{
    public class DynamicValue
    {
        public Guid Id { get; set; }
        public Document.DocumentFieldValues Value { get; set; }
    }
}
