using Devir.DMS.DL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.Document
{
    public class DocumentTemplate: ModelBase
    {

        public Guid UserId { get; set; }

        public DocumentViewModel ViewModel { get; set; }

        public String Name { get; set; }


    }
}