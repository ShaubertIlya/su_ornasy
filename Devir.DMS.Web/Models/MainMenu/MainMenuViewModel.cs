using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.MainMenu
{
    public class MainMenuViewModel
    {
        public long newNotificationsCount { get; set; }
        public long workingdocumentsCount { get; set; }
        public long newDocumentsCount { get; set; }
        public long workingTaskCount { get; set; }
        public long newTasksCount { get; set; }
        public long badTasksCount { get; set; }
        public long workingMyTasksCount { get; set; }
        public long badMyTasksCount { get; set; }
    }
}