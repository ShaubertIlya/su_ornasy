using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.OrganiztionStructure
{
    public class DepartmentViewModel
    {
        public Guid Id { get; set; }

        public String Name { get; set; }

        public String OU { get; set; }

        public Guid? ParentDepartmentId { get; set; }

        public String ParentDepartmentName { get; set; }

        public Guid? ChiefId { get; set; }

        public String ChiefName { get; set; }

        public String Code { get; set; }

        public static void CreateNew(DepartmentViewModel data)
        {
            var depRep = RepositoryFactory.GetRepository<Department>();
            var userRep = RepositoryFactory.GetRepository<User>();

            var newDep = new Department();
            newDep.Name = data.Name;
            newDep.OU = data.OU;
            newDep.ParentDepertmentId = data.ParentDepartmentId;
            newDep.Code = data.Code;
            User chief = null;
            
            if (data.ChiefId != null)
            {
                chief = userRep.Single(u => u.UserId == data.ChiefId.Value);
                newDep.ChiefUserId = chief.UserId;
                if (newDep.Users.Where(u => u.Key.UserId == chief.UserId).Count() == 0)
                {
                    newDep.Users.Add(chief, null);
                }
            }
            
            var depId = depRep.Insert(newDep);
 
        }

        public static void Save(DepartmentViewModel data)
        {
            var depRep = RepositoryFactory.GetRepository<Department>();
            var userRep = RepositoryFactory.GetRepository<User>();
            var dep = depRep.Single(d => d.Id == data.Id);


            if (dep.ChiefUserId == null && data.ChiefId != null ||
                (dep.ChiefUserId != null && dep.ChiefUserId != data.ChiefId) ||
                (dep.ChiefUserId != null && data.ChiefId == null))
            {
                User newChief = null;
                if (data.ChiefId != null)
                    newChief = userRep.Single(u => u.UserId == data.ChiefId.Value);
                if (newChief != null && dep.Users.Where(u => u.Key.UserId == newChief.UserId).Count() == 0)
                {
                   dep.Users.Add(newChief, null);
                }
                dep.ChiefUserId = newChief == null ? null : (Guid?)newChief.UserId;
            }
            
            dep.Name = data.Name;
            dep.OU = data.OU;
            dep.ParentDepertmentId = data.ParentDepartmentId;
            dep.Code = data.Code;
            depRep.update(dep);
        }

    }
}