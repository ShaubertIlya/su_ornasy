using Devir.DMS.DL.Models.Document.Route;
using Devir.DMS.DL.Models.References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.Document
{
    public class RouteUserSignResultViewModel
    {
        public Guid Id { get; set; }
        public string comment { get; set; }
        public Guid documentId { get; set; }
        public string documentNumber { get; set; }
        public string documentType { get; set; }
        public List<Guid> Attachments { get; set; }
        public DateTime documentDate { get; set; }
        public RouteStageUser mainStageUser { get; set; }
        public List<RouteStageUser> secondaryStageUsers { get; set; }
        public List<RouteStage> secondaryRouteStages { get; set; }
        public List<RouteAction> UsersRouteActions { get; set; }
    }
}