using System;

namespace Devir.DMS.DL.Models.Document
{
    public class DocumentViewModelItem
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string Header { get; set; }
        public string AuthorName { get; set; }
        public DateTime Date { get; set; }
        public DateTime innerSortDate { get; set; }
        public string gDate { get; set; }
        public string sDate { get; set; }
        public string TypeName { get; set; }
        public string CurrentStage { get; set; }
        public string gNumber { get; set; }
        public int docStateColor { get; set; }
        public bool isNew { get; set; }
        public bool isUrgent { get; set; }
        public Guid AddColumnId { get; set; }
        public string AddColumn { get; set; }
    }
}