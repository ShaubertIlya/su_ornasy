using Devir.DMS.DL.Models.References.OrganizationStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document
{
    public class Instruction:Document
    {
        public Guid RootDocumentId { get; set; }
        public User UserFor { get; set; }
        public bool isForInstruction { get; set; }

        public Guid RootDocumentTypeId { get; set; }


    }
}
