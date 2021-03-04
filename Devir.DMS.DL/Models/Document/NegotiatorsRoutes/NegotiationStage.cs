using Devir.DMS.DL.Models.Document.StorageTypes;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document.NegotiatorsRoutes
{
    public class NegotiationStage
    { 
        public int Order { get; set; }
        public List<UserForNegotiation> UsersForNegotiation { get; set; }       
        public NegotiationStageTypes StageType { get; set; }
    }
}
