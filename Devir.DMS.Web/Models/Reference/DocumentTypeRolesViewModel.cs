using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.Reference
{
    public class DocumentTypeRolesViewModel
    {
        public Guid DocumentTypeId { get; set; }
        public List<Guid> RoleIds { get; set; }
        public List<Role> AllRoles { get; set; }

        public void SaveToDocumentType()
        {
            var tmpDoc = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == this.DocumentTypeId);
            tmpDoc.Roles = RepositoryFactory.GetRepository<Role>().List(m => RoleIds.Contains(m.Id)).ToList();
            RepositoryFactory.GetRepository<DocumentType>().update(tmpDoc);
        }
    }
}