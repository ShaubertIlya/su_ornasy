using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document
{
    public class DocumentViewer
    {
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public DateTime? ViewDateTime { get; set; }
       
        public Guid UserId { get; set; }
    }
}
