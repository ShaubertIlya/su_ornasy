using Devir.DMS.DL.Models;
using Devir.DMS.Web.Models.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models
{
    public class DocumentViewModelDB : ModelBase
    {
        public Guid docId { get; set; }
        public DocumentViewModel ViewModel { get; set; }
    }
}