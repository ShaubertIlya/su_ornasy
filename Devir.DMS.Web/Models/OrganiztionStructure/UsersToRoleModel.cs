using Devir.DMS.DL.Models;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.OrganiztionStructure
{
    public class UsersToRoleModel 
    {

        public Role Role;

        public List<User> UsersToRole { get; set; }

    }
}