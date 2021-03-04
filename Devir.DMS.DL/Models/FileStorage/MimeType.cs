using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devir.DMS.DL.Repositories;

namespace Devir.DMS.DL.Models.FileStorage
{
    public class MimeType : ModelBase
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public Byte[] Icon { get; set; }

        //public List<MimeType> MimeTypes { get; set; }

        
    }
}
