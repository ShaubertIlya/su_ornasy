using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.References.OrganizationStructure
{
    public class Role : ModelBase
    {
        public String Name { get; set; }

        public List<Guid> UsersInRoles { get; set; }

        public Role()
        {
            UsersInRoles = new List<Guid>();
        }
    }
}
