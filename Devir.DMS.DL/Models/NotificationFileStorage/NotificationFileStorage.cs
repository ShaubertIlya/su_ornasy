using Devir.DMS.DL.Models.FileStorage;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.NotificationFileStorage
{
    public class NotificationFileStorage : ModelBase
    {
        public string FileName { get; set; }
        public int NumberOfVersion { get; set; }
        public bool? isCumulative { get; set; }
        public MimeType MimeType { get; set; }
        public ObjectId OId { get; set; }
    }
}
