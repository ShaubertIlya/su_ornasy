using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;

namespace Devir.DMS.Web.Helpers
{
    public class UserStorageHelper
    {
        static Dictionary<String, Guid> UserList { get; set; }
    }    
}