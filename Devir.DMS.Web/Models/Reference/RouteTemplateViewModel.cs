using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.DocumentTemplates;
using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Repositories;

namespace Devir.DMS.Web.Models.Reference
{
    public class RouteTemplateViewModel
    {
        [Required]
        public Guid RouteTemplateId { get; set; }

        public List<RouteTemplate> RouteTemplates { get; set; } 
        
        public List<RouteType> RouteTypes { get; set; }
        public List<FieldTemplate> FieldTemplates { get; set; }

        public static void SaveToDocumentType(RouteTemplateViewModel data)
        {
            var docTypeRepo = RepositoryFactory.GetRepository<DocumentType>();
            var tmpDocType = docTypeRepo.Single(m => m.Id == data.RouteTemplateId);

            tmpDocType.RouteTemplates = new List<RouteTemplate>();

            data.RouteTemplates.ForEach(m =>
            {
                if (m.Id == Guid.Empty)
                    m.Id = Guid.NewGuid();

                tmpDocType.RouteTemplates.Add(new RouteTemplate() { FieldOrder = m.FieldOrder, Id=m.Id, TypeOfTheRoute=RepositoryFactory.GetRepository<RouteType>().Single(n => n.Id == m.TypeOfTheRouteId), DocumentFieldTemplate=m.DocumentFieldTemplate });
            });

            docTypeRepo.update(
                       tmpDocType
                       );
        }
    }
}