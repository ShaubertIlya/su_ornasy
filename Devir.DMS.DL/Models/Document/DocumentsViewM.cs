using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document
{
    public class DocumentsViewM
    {
        public object Group { get; internal set; }
        public bool Visible { get; internal set; }
        public List<DocumentViewModelItem> Values { get; internal set; }
        public int DataCount { get; internal set; }


    }
}
