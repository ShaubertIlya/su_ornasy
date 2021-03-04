using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document
{
    public class DocumentInstructionViewModel
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string DocumentType { get; set; }
        public string Header { get; set; }
        public Guid RootDocumentId { get; set; }
        public string UserFor { get; set; }
        public string CreateDate  { get; set; }
        public int OutOfDate { get; set; }
        public string FinishDate { get; set; }
        public string CompletedDate { get; set; }
        public string ApproveDate { get; set; }
        public string Classes { get; set; }

        public DateTime Date { get; set; }
        public string gDate { get; set; }
        public string sDate { get; set; }
        public bool isNew { get; set; }
        public bool isUrgent { get; set; }
        public bool isExpired { get; set; }
        public TimeSpan ExpiredTimeSpan { get; set; }
    }
}
