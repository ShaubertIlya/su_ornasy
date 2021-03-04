using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document
{
    public enum DocumentState
    {
        Draft = 0,
        InWork = 1,
        FinishedOk = 2,
        FinishedWithError = 3        
    }
}
