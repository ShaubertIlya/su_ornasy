using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document
{
    public class DocumentTypeCount :ModelBase
    {
        public Guid DocumentTypeId { get; set; }
        public int Count { get; set; }
        public Guid UserId { get; set; }
        public int Year { get; set; }
    }   
}
