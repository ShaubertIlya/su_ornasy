using Devir.DMS.DL.Models.References.OrganizationStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document.Route
{
    public class SignUser
    {
        public Guid Id { get; set; }
        public User UserToSign {get;set;}
        public DateTime? SignBefore {get;set;}
        public bool? IsMust { get; set; }       
        public UserSignResult Result { get; set; }
        
    }
}
