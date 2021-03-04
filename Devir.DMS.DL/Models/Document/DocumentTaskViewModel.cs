using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document
{
    public class DocumentTaskViewModel
    {
        public Guid Id { get; set; }
        public string CalculatedId { get; set; }
        public string Header { get; set; }
        public string Name { get; set; }
        public string FinishDate { get; set; }
        public string Number { get; set; }
        public string AuthorName { get; set; }
        public int Group { get; set; }
        public string Classes { get; set; }
        public string hrefAddress { get; set; }

        public DateTime Date { get; set; }
        public string gDate { get; set; }
        public string sDate { get; set; }
        public bool isNew { get; set; }
        public bool isUrgent { get; set; }
        public bool isExpired { get; set; }
        public TimeSpan ExpiredTimeSpan { get; set; }
    }
}
