using Devir.DMS.DL.Models.References.OrganizationStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document.DocumentNotifications
{
    public class Notifications: ModelBase
    {
        public User ForWho { get; set; }
        public String Text { get; set; }
        public String LinkText{get;set;}
        public DateTime? ViewDateTime { get; set; }
        public Guid DocumentId { get; set; }
    }    
}
