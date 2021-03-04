using Devir.DMS.DL.Models.References.OrganizationStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models
{
    public class ModelBase
    {
        public Guid Id { get; set; }
        public Guid BaseId { get; set; }
        public bool isDeleted { get; set; }
        public Guid CreatorGuid { get; set; }
        public DateTime CreateDate {get;set;}
        public Guid DeletedByUserGuid { get; set; }
        public DateTime DeleteDate { get; set; }
    }
}
