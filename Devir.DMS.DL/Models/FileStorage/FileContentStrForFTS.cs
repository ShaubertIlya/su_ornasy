using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.FileStorage
{
    public class FileContentStrForFTS:ModelBase
    {
        public Guid FileStrorageId {get;set;}
        public string Content { get; set; }
    }
}
