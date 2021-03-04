using Devir.DMS.DL.Models.References.OrganizationStructure;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document.StorageTypes
{
    public class UserForNegotiation
    {
        public int Order { get; set; }
        public Guid UserId { get; set; }      

        public DateTime? DateBeforeSign { get; set; }
        public bool IsMustSign { get; set; }
    }
}
