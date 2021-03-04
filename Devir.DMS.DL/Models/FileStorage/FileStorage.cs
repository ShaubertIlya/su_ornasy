using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Devir.DMS.DL.Models.FileStorage
{
    public class FileStorage : ModelBase
    {
        public string FileName { get; set; }
        //public string StoreFolder { get; set; }
        public string Description { get; set; }
        public MimeType MimeType { get; set; }
        public ObjectId? PDFVersion { get; set; }
        public bool IsPDFWorkerCompleted { get; set; }
        public ObjectId OId { get; set; }
        //public string FileContentForFTS { get; set; }

        public String StoreFolder;
    }
}
