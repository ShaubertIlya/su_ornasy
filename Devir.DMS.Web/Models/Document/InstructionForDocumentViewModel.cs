using Devir.DMS.DL.Models.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.Document
{
    public class InstructionForDocumentViewModel
    {
        public Guid Id{get;set;}
        public bool isPerformControlAction { get; set; }
        public Guid CurrentSigner { get;set; }
        public string CurrentSignerFIO { get; set; }
        public string UserNameFrom { get; set; }
        public string UserNameFor {get;set;}
        public Guid? ParentId { get; set; }
        public DateTime DateBefore { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }        
        public int level { get; set; }
        public DocumentState DocState { get; set; }

        public DateTime FinishDate { get; set; }

        public string SignResult { get; set; }
        public List<Guid> Attachments { get; set; }
        public Guid RouteStageId { get; set; }
        public Guid RouteStageUserId { get; set; }
        public Guid FinishedRouteUserId { get; set; }
    }

}