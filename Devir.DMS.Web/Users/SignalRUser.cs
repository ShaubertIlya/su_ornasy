using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Users
{
    public class SignalRUser
    {
        public string UserName { get; set; }
        public Guid UserId { get; set; }
        public Guid SessionId { get; set; }
    }
}